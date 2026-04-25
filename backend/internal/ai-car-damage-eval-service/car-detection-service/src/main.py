import logging
from contextlib import asynccontextmanager
from datetime import datetime, timezone

import cv2
import numpy as np
from fastapi import Depends, FastAPI, File, Form, HTTPException, Request, UploadFile
from fastapi.concurrency import run_in_threadpool

from src.core.config import get_settings
from src.schemas.inspection import Damage, InspectionResult, PhotoSlot, RejectedPhoto, Verdict
from src.services.pipeline_detector import detect_car_and_obstacles, detect_damages, warmup_models
from src.services.quality import check_quality
from src.services.verification import verify_attributes


INTERNAL_API_KEY_HEADER = "x-internal-api-key"


logger = logging.getLogger(__name__)
settings = get_settings()


# Readiness latch: flipped True once the model warmup has completed
# successfully. /ready consults this instead of re-running warmup on
# every probe — avoids stampeding the model path and gives compose
# healthchecks an authoritative "models loaded" signal.
_models_warm: bool = False


@asynccontextmanager
async def lifespan(_app: FastAPI):
    global _models_warm
    _log_auth_mode()
    try:
        await run_in_threadpool(warmup_models)
        _models_warm = True
    except Exception:
        _models_warm = False
        logger.exception("model warmup failed during startup")
        raise
    yield


def _log_auth_mode() -> None:
    """Emit a clear one-line startup signal describing the auth posture."""
    if settings.internal_api_key:
        logger.info("ai-damage-eval: internal auth ENFORCED (INTERNAL_API_KEY set)")
        return

    if settings.is_dev_bypass_active():
        # Warn-level so it stands out in logs — dev convenience must be
        # obvious to anyone watching.
        logger.warning(
            "ai-damage-eval: DEV-BYPASS ACTIVE — accepting unauthenticated requests "
            "(ALLOW_UNAUTHENTICATED_INTERNAL_DEV=true + ENVIRONMENT=development). "
            "NEVER use this combination outside local development.",
        )
        return

    # No secret AND no explicit dev-bypass. The service boots but every
    # /inspect-session request will be 503'd by the guard. We log this
    # loudly so the misconfiguration is diagnosable.
    logger.error(
        "ai-damage-eval: MISCONFIGURED — INTERNAL_API_KEY is not set and "
        "dev-bypass is inactive. All /inspect-session requests will fail 503. "
        "Set INTERNAL_API_KEY or enable dev-bypass for local development.",
    )


app = FastAPI(title=settings.app_title, lifespan=lifespan)


async def require_internal_auth(request: Request) -> None:
    """Shared-secret guard matching the X-Internal-Api-Key pattern used by
    the rest of the monorepo.

    Fails closed by default: if the service has no INTERNAL_API_KEY set
    and the explicit dev-bypass combo is not active, every request gets
    503 Service Unavailable. This prevents accidentally deploying the
    service "open" because an env variable was forgotten.
    """
    if not settings.auth_is_enforced():
        # Dev-bypass path — warning is logged once at startup, not per-request.
        return

    if not settings.internal_api_key:
        # Auth enforced but no secret configured. Fail closed; do NOT
        # reveal whether a key exists via the error body.
        raise HTTPException(
            status_code=503,
            detail="internal auth is not configured",
        )

    provided = request.headers.get(INTERNAL_API_KEY_HEADER)
    if not provided or provided != settings.internal_api_key:
        raise HTTPException(status_code=401, detail="invalid or missing internal api key")


async def decode_image(upload_file: UploadFile) -> np.ndarray | None:
    contents = await upload_file.read()
    nparr = np.frombuffer(contents, np.uint8)
    return cv2.imdecode(nparr, cv2.IMREAD_COLOR)


def compute_iou(box_a: list[int], box_b: list[int]) -> float:
    x_left = max(box_a[0], box_b[0])
    y_top = max(box_a[1], box_b[1])
    x_right = min(box_a[2], box_b[2])
    y_bottom = min(box_a[3], box_b[3])

    if x_right <= x_left or y_bottom <= y_top:
        return 0.0

    intersection = (x_right - x_left) * (y_bottom - y_top)
    area_a = max(0, box_a[2] - box_a[0]) * max(0, box_a[3] - box_a[1])
    area_b = max(0, box_b[2] - box_b[0]) * max(0, box_b[3] - box_b[1])
    union = area_a + area_b - intersection
    return 0.0 if union == 0 else intersection / union


def deduplicate_damages(damages: list[Damage], iou_threshold: float = 0.5) -> list[Damage]:
    deduplicated: list[Damage] = []

    for candidate in sorted(damages, key=lambda item: item.confidence, reverse=True):
        duplicate_found = any(
            existing.type == candidate.type
            and existing.slot == candidate.slot
            and compute_iou(existing.bbox, candidate.bbox) >= iou_threshold
            for existing in deduplicated
        )
        if not duplicate_found:
            deduplicated.append(candidate)

    return deduplicated


async def process_photo(
    file: UploadFile,
    slot: PhotoSlot,
    car_id: str,
    car_model: str,
    car_color: str,
) -> tuple[bool, list[Damage], RejectedPhoto | None]:
    filename = file.filename or f"{slot.value}.jpg"

    try:
        image = await decode_image(file)
    except Exception as exc:
        return False, [], RejectedPhoto(
            slot=slot,
            filename=filename,
            step=0,
            reason="Failed to read image",
            details=[str(exc)],
        )

    if image is None:
        return False, [], RejectedPhoto(slot=slot, filename=filename, step=0, reason="Invalid image format")

    try:
        is_good, reason = await run_in_threadpool(check_quality, image)
        if not is_good:
            return False, [], RejectedPhoto(slot=slot, filename=filename, step=1, reason=reason)

        # Interior shots do not require a visible whole car — skip the
        # exterior-only detection and verification stages.
        if slot is PhotoSlot.INTERIOR:
            raw_damages = await run_in_threadpool(detect_damages, image)
        else:
            is_valid_car, reason, car_data = await run_in_threadpool(detect_car_and_obstacles, image)
            if not is_valid_car:
                return False, [], RejectedPhoto(slot=slot, filename=filename, step=2, reason=reason)

            is_verified, reason, mismatches = await run_in_threadpool(
                verify_attributes,
                image,
                car_id,
                car_data["car_bbox"],
                car_model,
                car_color,
            )
            if not is_verified:
                return False, [], RejectedPhoto(slot=slot, filename=filename, step=3, reason=reason, details=mismatches)

            raw_damages = await run_in_threadpool(detect_damages, image)
    except FileNotFoundError as exc:
        raise HTTPException(status_code=500, detail=str(exc)) from exc
    except Exception as exc:
        return False, [], RejectedPhoto(
            slot=slot,
            filename=filename,
            step=5,
            reason="Photo processing failed",
            details=[str(exc)],
        )

    damages = [Damage(**damage, slot=slot, source_file=filename) for damage in raw_damages]
    return True, damages, None


@app.post(
    "/inspect-session",
    response_model=InspectionResult,
    dependencies=[Depends(require_internal_auth)],
)
async def inspect_session(
    car_id: str = Form(...),
    car_model: str = Form(...),
    car_color: str = Form(...),
    photo_front: UploadFile = File(...),
    photo_back: UploadFile = File(...),
    photo_side_left: UploadFile = File(...),
    photo_side_right: UploadFile = File(...),
    photo_interior: UploadFile = File(...),
):
    slotted_files: list[tuple[PhotoSlot, UploadFile]] = [
        (PhotoSlot.FRONT, photo_front),
        (PhotoSlot.BACK, photo_back),
        (PhotoSlot.SIDE_LEFT, photo_side_left),
        (PhotoSlot.SIDE_RIGHT, photo_side_right),
        (PhotoSlot.INTERIOR, photo_interior),
    ]

    valid_photos_count = 0
    all_damages: list[Damage] = []
    rejected_photos: list[RejectedPhoto] = []
    for slot, file in slotted_files:
        is_valid_photo, damages, rejected_photo = await process_photo(
            file,
            slot,
            car_id,
            car_model,
            car_color,
        )
        if rejected_photo is not None:
            rejected_photos.append(rejected_photo)
            continue

        if is_valid_photo:
            valid_photos_count += 1
            all_damages.extend(damages)

    # Minimum: require at least four of the five slots to be accepted —
    # this gives the model redundancy for damage deduplication while
    # still tolerating one bad-quality photo.
    is_session_valid = valid_photos_count >= settings.min_photos
    unique_damages = deduplicate_damages(all_damages)

    if not is_session_valid:
        verdict = Verdict.INVALID_SESSION
    elif unique_damages:
        verdict = Verdict.DAMAGES_FOUND
    else:
        verdict = Verdict.OK

    return InspectionResult(
        verdict=verdict,
        damages=unique_damages,
        rejected_photos=rejected_photos,
        valid_photos_count=valid_photos_count,
        processed_at_utc=datetime.now(timezone.utc),
    )


@app.get("/health")
async def healthcheck() -> dict[str, str]:
    """Liveness signal: process is up. Does not assert model readiness —
    use /ready for that. A separate liveness endpoint lets orchestrators
    distinguish "still warming up" (503 ready, 200 live) from "crashed"
    (no response at all)."""
    return {"status": "ok"}


@app.get("/ready")
async def readiness_check() -> dict[str, str]:
    """Readiness signal backed by a one-shot warmup latch. Returns 503
    until the lifespan hook has successfully loaded the YOLO weights.
    Compose healthchecks hit this so the caller (booking-service) only
    sees the AI service "up" after model weights are in GPU/CPU memory.
    Never re-runs warmup — that is the lifespan's job."""
    if not _models_warm:
        raise HTTPException(status_code=503, detail="models are still warming up")
    return {"status": "ready"}
