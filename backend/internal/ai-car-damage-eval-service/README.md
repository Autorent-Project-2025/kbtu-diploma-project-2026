# AI Car Damage Evaluation Service

Internal FastAPI service that inspects the five booking-completion photos, verifies they show the expected car, and produces an advisory damage assessment for the booking review flow.

Called synchronously by `booking-service` during `SubmitCompletionReview`. The service is **advisory-only** — the human manager still makes the final call on fines.

## What it does

- Accepts **five slot-labelled photos** (`front`, `back`, `side_left`, `side_right`, `interior`) in a single multipart POST.
- Rejects photos with invalid format, low quality, obstructed car, wrong color, or mismatched `car_id` format.
- Runs a YOLO damage detector on accepted photos and deduplicates overlapping detections per-slot.
- Returns a structured result: `verdict`, per-slot damages, per-slot rejected photos, `valid_photos_count`, `processed_at_utc`.

## Integration contract

`POST /inspect-session` — multipart form:

| Field              | Type        | Required |
|--------------------|-------------|----------|
| `car_id`           | form string | yes      |
| `car_model`        | form string | yes      |
| `car_color`        | form string | yes      |
| `photo_front`      | file        | yes      |
| `photo_back`       | file        | yes      |
| `photo_side_left`  | file        | yes      |
| `photo_side_right` | file        | yes      |
| `photo_interior`   | file        | yes      |

Headers:
- `X-Internal-Api-Key: <shared-secret>` — required when `INTERNAL_API_KEY` is set (prod).

Response (`InspectionResult`):
```jsonc
{
  "verdict": "OK" | "DAMAGES_FOUND" | "INVALID_SESSION",
  "damages": [{ "type": "scratch", "confidence": 0.87, "bbox": [x1,y1,x2,y2], "slot": "front", "source_file": "..." }],
  "rejected_photos": [{ "slot": "back", "filename": "...", "step": 1, "reason": "...", "details": [] }],
  "valid_photos_count": 4,
  "processed_at_utc": "2026-05-01T12:34:56.789Z"
}
```

### Verdict semantics

- `OK` — all valid photos, no damages detected. Ticket is created, manager can approve quickly.
- `DAMAGES_FOUND` — valid photos, at least one damage detected. Ticket is created with findings for manager to inspect.
- `INVALID_SESSION` — fewer than 4 of the 5 slots were usable (blurry / obstructed / wrong car). **Booking-service returns HTTP 400 to the client** with per-slot rejection details so the user can re-upload.

## Authoritative data source

The caller (`booking-service`) fetches `car_model` and `car_color` from **`car-service`'s partner-car snapshot** before invoking this service. The local `config/car_registry.json` is kept only for local development smoke-tests and is disabled in production via `USE_REGISTRY_VALIDATION=false`. In the production path we validate the caller-supplied color against the color detected in the photo — nothing else.

## Docker-compose integration

Added as `ai-damage-eval-service` in the root `docker-compose.yml`:

```yaml
ai-damage-eval-service:
  build:
    context: ./backend/internal/ai-car-damage-eval-service/car-detection-service
    dockerfile: ./Dockerfile
  environment:
    INTERNAL_API_KEY: local-ai-damage-eval-service-key
    USE_REGISTRY_VALIDATION: "false"
  expose:
    - "8000"
  healthcheck:
    test: ["CMD", "curl", "-fsS", "http://127.0.0.1:8000/health"]
```

Booking-service does **not** list this service in `depends_on` — the AI service does model warmup for ~30 seconds on start, and the booking flow tolerates AI being unavailable (fail-open).

## Environment variables

- `INTERNAL_API_KEY` — shared secret for the `X-Internal-Api-Key` header. Unset = auth disabled.
- `USE_REGISTRY_VALIDATION` — when `true`, additionally cross-checks `car_id`/`car_model` against `config/car_registry.json`. Default `false`.
- `MIN_PHOTOS` — minimum valid photos required for session to be valid. Default `4`.
- `MAX_PHOTOS` — cap (unused in slot-based API, retained for backward compat).
- `MIN_WIDTH`, `MIN_HEIGHT`, `MIN_SHARPNESS`, `MIN_BRIGHTNESS`, `MAX_BRIGHTNESS` — quality thresholds.
- `COCO_WEIGHTS_PATH`, `DAMAGE_WEIGHTS_PATH` — model checkpoint paths.

## Run locally

```bash
python -m venv venv
venv\Scripts\activate
pip install -r car-detection-service/requirements.txt
uvicorn src.main:app --reload --app-dir car-detection-service
```

## Model weights

Place these files in `car-detection-service/weights/`:

- `yolov8n.pt` — COCO pretrained for car detection
- `yolov8m_damage_v1.pt` — fine-tuned damage detector

## Health endpoints

- `GET /health` — 200 always
- `GET /ready` — 200 once models are loaded, 503 otherwise

## Tests

```bash
cd car-detection-service
pytest tests/ -v
```

`tests/test_api.py` covers the contract: missing slot → 422, missing auth → 401, all-broken session → `INVALID_SESSION` with slot-labelled rejections.
