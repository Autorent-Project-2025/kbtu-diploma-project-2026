# Car Detection Service

FastAPI service for car inspection sessions with photo quality checks, vehicle detection, and damage detection.

## What It Does

- Accepts 4 to 8 photos for one inspection session.
- Rejects photos with invalid format, low quality, blocked cars, invalid `car_id`, wrong car model, or wrong color.
- Runs damage detection on valid photos.
- Deduplicates repeated damage detections across photos.
- Exposes `/health` and `/ready` endpoints for monitoring.

## Run Locally

```bash
python -m venv venv
venv\Scripts\activate
pip install -r requirements.txt
uvicorn src.main:app --reload
```

## Required Weights

Place these files in `weights/`:

- `yolov8n.pt`
- `yolov8m_damage_v1.pt`

## Main Endpoint

`POST /inspect-session`

Form fields:

- `car_id`: string
- `car_model`: string
- `car_color`: string
- `files`: 4 to 8 uploaded image files

## Registry

The service checks the submitted `car_model` against `config/car_registry.json` using `car_id`.
The submitted `car_color` is validated both against the registry and against the detected dominant car color on the photo.

## Response

The service returns:

- `verdict`: `OK`, `DAMAGES_FOUND`, or `INVALID_SESSION`
- `damages`: detected damage objects
- `rejected_photos`: per-photo rejection reasons
- `valid_photos_count`: number of accepted photos

## Test

```bash
pytest
```
