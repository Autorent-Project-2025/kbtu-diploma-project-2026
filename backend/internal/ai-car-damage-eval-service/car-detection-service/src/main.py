from contextlib import asynccontextmanager

import cv2
import numpy as np
from fastapi import FastAPI, File, Form, HTTPException, UploadFile
from fastapi.concurrency import run_in_threadpool

from src.core.config import get_settings
from src.schemas.inspection import Damage, InspectionResult, RejectedPhoto, Verdict
from src.services.pipeline_detector import detect_car_and_obstacles, detect_damages, warmup_models
from src.services.quality import check_quality
from src.services.verification import verify_attributes


settings = get_settings()


@asynccontextmanager
async def lifespan(_app: FastAPI):
    await run_in_threadpool(warmup_models)
    yield


app = FastAPI(title=settings.app_title, lifespan=lifespan)


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
            existing.type == candidate.type and compute_iou(existing.bbox, candidate.bbox) >= iou_threshold
            for existing in deduplicated
        )
        if not duplicate_found:
            deduplicated.append(candidate)

    return deduplicated


async def process_photo(
    file: UploadFile,
    car_id: str,
    car_model: str,
    car_color: str,
) -> tuple[bool, list[Damage], RejectedPhoto | None]:
    filename = file.filename or "unknown"

    try:
        image = await decode_image(file)
    except Exception as exc:
        return False, [], RejectedPhoto(
            filename=filename,
            step=0,
            reason="Failed to read image",
            details=[str(exc)],
        )

    if image is None:
        return False, [], RejectedPhoto(filename=filename, step=0, reason="Invalid image format")

    try:
        is_good, reason = await run_in_threadpool(check_quality, image)
        if not is_good:
            return False, [], RejectedPhoto(filename=filename, step=1, reason=reason)

        is_valid_car, reason, car_data = await run_in_threadpool(detect_car_and_obstacles, image)
        if not is_valid_car:
            return False, [], RejectedPhoto(filename=filename, step=2, reason=reason)

        is_verified, reason, mismatches = await run_in_threadpool(
            verify_attributes,
            image,
            car_id,
            car_data["car_bbox"],
            car_model,
            car_color,
        )
        if not is_verified:
            return False, [], RejectedPhoto(filename=filename, step=3, reason=reason, details=mismatches)

        raw_damages = await run_in_threadpool(detect_damages, image)
    except FileNotFoundError as exc:
        raise HTTPException(status_code=500, detail=str(exc)) from exc
    except Exception as exc:
        return False, [], RejectedPhoto(
            filename=filename,
            step=5,
            reason="Photo processing failed",
            details=[str(exc)],
        )

    damages = [Damage(**damage, source_file=filename) for damage in raw_damages]
    return True, damages, None


@app.post("/inspect-session", response_model=InspectionResult)
async def inspect_session(
    car_id: str = Form(...),
    car_model: str = Form(...),
    car_color: str = Form(...),
    files: list[UploadFile] = File(...),
):
    if not (settings.min_photos <= len(files) <= settings.max_photos):
        raise HTTPException(
            status_code=400,
            detail=f"Require {settings.min_photos} to {settings.max_photos} photos.",
        )

    valid_photos_count = 0
    all_damages: list[Damage] = []
    rejected_photos: list[RejectedPhoto] = []
    for file in files:
        is_valid_photo, damages, rejected_photo = await process_photo(
            file,
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
    )


@app.get("/health")
async def healthcheck() -> dict[str, str]:
    return {"status": "ok"}


@app.get("/ready")
async def readiness_check() -> dict[str, str]:
    try:
        await run_in_threadpool(warmup_models)
    except Exception as exc:
        raise HTTPException(status_code=503, detail=str(exc)) from exc

    return {"status": "ready"}
