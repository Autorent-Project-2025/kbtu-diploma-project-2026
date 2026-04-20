from dataclasses import dataclass
from functools import lru_cache
from pathlib import Path

import numpy as np
from ultralytics import YOLO

from src.core.config import Settings, get_settings


@dataclass(frozen=True)
class DetectorModels:
    coco_model: YOLO
    damage_model: YOLO


OBSTACLE_CLASSES = [0, 13, 56]


def _ensure_weights_exist(*paths: Path) -> None:
    missing_paths = [str(path) for path in paths if not path.exists()]
    if missing_paths:
        raise FileNotFoundError(f"Missing model weights: {', '.join(missing_paths)}")


@lru_cache(maxsize=1)
def get_models() -> DetectorModels:
    settings = get_settings()
    _ensure_weights_exist(settings.coco_weights_path, settings.damage_weights_path)
    return DetectorModels(
        coco_model=YOLO(str(settings.coco_weights_path)),
        damage_model=YOLO(str(settings.damage_weights_path)),
    )


def warmup_models() -> None:
    get_models()


def detect_car_and_obstacles(image: np.ndarray) -> tuple[bool, str, dict]:
    settings: Settings = get_settings()
    models = get_models()
    results = models.coco_model(image, verbose=False)[0]
    img_area = image.shape[0] * image.shape[1]

    car_box = None
    car_area = 0.0
    obstacles: list[list[float]] = []

    for box in results.boxes:
        cls_id = int(box.cls[0])
        conf = float(box.conf[0])
        coords = box.xyxy[0].tolist()

        if conf < settings.detection_confidence_threshold:
            continue

        if cls_id == 2:
            current_area = (coords[2] - coords[0]) * (coords[3] - coords[1])
            if current_area > car_area:
                car_box = coords
                car_area = current_area
        elif cls_id in OBSTACLE_CLASSES:
            obstacles.append(coords)

    if not car_box:
        return False, "Car not detected", {}

    if (car_area / img_area) < settings.min_car_area_ratio:
        return False, "Car takes too little of the frame", {}

    for obs in obstacles:
        x_left, y_top = max(car_box[0], obs[0]), max(car_box[1], obs[1])
        x_right, y_bottom = min(car_box[2], obs[2]), min(car_box[3], obs[3])
        if x_right > x_left and y_bottom > y_top:
            overlap_area = (x_right - x_left) * (y_bottom - y_top)
            if (overlap_area / car_area) > settings.max_obstacle_overlap_ratio:
                return False, "Obstacle blocks the car", {}

    return True, "OK", {"car_bbox": [round(coord) for coord in car_box]}


def detect_damages(image: np.ndarray) -> list[dict]:
    settings = get_settings()
    models = get_models()
    results = models.damage_model(
        image,
        conf=settings.damage_confidence_threshold,
        verbose=False,
    )[0]
    damages: list[dict] = []

    for box in results.boxes:
        damages.append(
            {
                "type": models.damage_model.names[int(box.cls[0])],
                "confidence": round(float(box.conf[0]), 2),
                "bbox": [round(x) for x in box.xyxy[0].tolist()],
            }
        )

    return damages
