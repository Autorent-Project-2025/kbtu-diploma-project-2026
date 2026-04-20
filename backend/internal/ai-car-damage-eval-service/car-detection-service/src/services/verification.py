import json
import re
from functools import lru_cache
from pathlib import Path

import numpy as np

from src.core.config import get_settings


CAR_ID_PATTERN = re.compile(r"^[A-Za-z0-9_-]{3,64}$")
NON_ALNUM_RE = re.compile(r"[^a-z0-9]+")

COLOR_ALIASES = {
    "black": "black",
    "white": "white",
    "gray": "gray",
    "grey": "gray",
    "silver": "silver",
    "red": "red",
    "blue": "blue",
    "green": "green",
    "yellow": "yellow",
}


def _normalize_text(value: str) -> str:
    return NON_ALNUM_RE.sub("", value.strip().lower())


def _normalize_color(value: str) -> str:
    normalized = _normalize_text(value)
    return COLOR_ALIASES.get(normalized, normalized)


@lru_cache(maxsize=1)
def load_car_registry() -> dict[str, dict[str, str]]:
    registry_path: Path = get_settings().car_registry_path
    if not registry_path.exists():
        return {}

    with registry_path.open("r", encoding="utf-8") as registry_file:
        raw_registry = json.load(registry_file)

    normalized_registry: dict[str, dict[str, str]] = {}
    for car_id, data in raw_registry.items():
        if not isinstance(data, dict):
            continue
        normalized_registry[car_id.strip()] = {
            "model": str(data.get("model", "")).strip(),
            "color": str(data.get("color", "")).strip(),
        }
    return normalized_registry


def infer_car_color(image: np.ndarray, car_bbox: list[int]) -> str:
    x1, y1, x2, y2 = [int(value) for value in car_bbox]
    h, w = image.shape[:2]
    x1 = max(0, min(x1, w - 1))
    x2 = max(0, min(x2, w))
    y1 = max(0, min(y1, h - 1))
    y2 = max(0, min(y2, h))

    if x2 <= x1 or y2 <= y1:
        return "unknown"

    crop = image[y1:y2, x1:x2]
    if crop.size == 0:
        return "unknown"

    hsv_crop = crop.astype(np.float32)
    blue = hsv_crop[:, :, 0]
    green = hsv_crop[:, :, 1]
    red = hsv_crop[:, :, 2]

    brightness = float(np.mean((red + green + blue) / 3))
    max_channel = np.maximum(np.maximum(red, green), blue)
    min_channel = np.minimum(np.minimum(red, green), blue)
    saturation = float(np.mean(np.where(max_channel == 0, 0, (max_channel - min_channel) / max_channel * 255)))

    if brightness < 60:
        return "black"
    if brightness > 190 and saturation < 40:
        return "white"
    if saturation < 50:
        return "silver" if brightness > 150 else "gray"

    red_mean = float(np.mean(red))
    green_mean = float(np.mean(green))
    blue_mean = float(np.mean(blue))

    if red_mean >= green_mean and red_mean >= blue_mean:
        if green_mean > blue_mean * 1.15:
            return "yellow"
        return "red"
    if green_mean >= red_mean and green_mean >= blue_mean:
        return "green"
    return "blue"


def verify_attributes(
    image: np.ndarray,
    car_id: str,
    car_bbox: list[int],
    expected_model: str,
    expected_color: str,
) -> tuple[bool, str, list[str]]:
    """Validate that the uploaded photo shows the expected car.

    The caller (booking-service) is the authoritative source for
    car_model/car_color — it reads them from car-service snapshots. We
    only verify that the photo is consistent with the *caller-supplied*
    attributes (car is present, detected color roughly matches).

    The optional local JSON registry is kept for dev smoke-tests; it is
    disabled in production via the USE_REGISTRY_VALIDATION flag.
    """
    settings = get_settings()

    normalized_car_id = car_id.strip()
    if not normalized_car_id:
        return False, "Car ID is empty", ["car_id must not be blank"]

    if not CAR_ID_PATTERN.fullmatch(normalized_car_id):
        return False, "Car ID format is invalid", ["allowed characters: letters, digits, '-' and '_'"]

    expected_model = expected_model.strip()
    expected_color = expected_color.strip()
    if not expected_model:
        return False, "Car model is empty", ["car_model must not be blank"]
    if not expected_color:
        return False, "Car color is empty", ["car_color must not be blank"]

    normalized_expected_color = _normalize_color(expected_color)

    # Optional local registry cross-check — only in dev/smoke-test mode.
    if settings.use_registry_validation:
        registry = load_car_registry()
        registry_entry = registry.get(normalized_car_id)
        if not registry_entry:
            return False, "Car is missing in registry", [f"register {normalized_car_id} in config/car_registry.json"]

        registered_model = registry_entry.get("model", "")
        if _normalize_text(registered_model) != _normalize_text(expected_model):
            return False, "Car model mismatch", [f"registered model: {registered_model or 'unknown'}"]

        normalized_registered_color = _normalize_color(registry_entry.get("color", ""))
        if normalized_registered_color and normalized_registered_color != normalized_expected_color:
            return False, "Car color mismatch with registry", [f"registered color: {registry_entry['color']}"]

    # Image-level colour sanity check — compares caller-provided colour
    # to what the model sees. This is the only check that runs in the
    # production (non-registry) path.
    inferred_color = infer_car_color(image, car_bbox)
    normalized_inferred_color = _normalize_color(inferred_color)

    if normalized_inferred_color != normalized_expected_color:
        return False, "Car color mismatch", [f"detected color: {inferred_color}"]

    return True, "OK", []
