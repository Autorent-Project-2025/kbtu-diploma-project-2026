import json
import re
from functools import lru_cache
from pathlib import Path

import numpy as np

from src.core.config import get_settings


# `car_id` is the license plate of the partner car (e.g. "A123BC" or
# "123ABC77"). Booking-service strips whitespace before sending, so only
# latin letters, digits, dashes and underscores end up here. 3-char
# minimum filters out obviously bogus values.
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

# Color families — when the caller-supplied color and the detected color
# both belong to the same family, we accept the photo even if the labels
# differ. This is the single biggest source of false INVALID_SESSION
# rejects: neutral-colored cars under indoor lighting routinely swing
# between "white/silver", "silver/gray", "gray/black".
COLOR_FAMILIES = {
    "black": "neutral",
    "white": "neutral",
    "gray": "neutral",
    "silver": "neutral",
    "red": "warm",
    "yellow": "warm",
    "blue": "cool",
    "green": "cool",
}


def _normalize_text(value: str) -> str:
    return NON_ALNUM_RE.sub("", value.strip().lower())


def _normalize_color(value: str) -> str:
    normalized = _normalize_text(value)
    return COLOR_ALIASES.get(normalized, normalized)


def _color_family(color: str) -> str:
    """Return one of: neutral | warm | cool | unknown."""
    return COLOR_FAMILIES.get(_normalize_color(color), "unknown")


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


def _analyze_color_crop(image: np.ndarray, car_bbox: list[int]) -> tuple[str, float, float]:
    """Return (label, brightness, saturation).

    Exposing brightness/saturation alongside the label lets the caller
    treat low-confidence detections (dim/desaturated lighting) as
    "unknown" instead of hard-rejecting on a noisy color guess.
    """
    x1, y1, x2, y2 = [int(value) for value in car_bbox]
    h, w = image.shape[:2]
    x1 = max(0, min(x1, w - 1))
    x2 = max(0, min(x2, w))
    y1 = max(0, min(y1, h - 1))
    y2 = max(0, min(y2, h))

    if x2 <= x1 or y2 <= y1:
        return "unknown", 0.0, 0.0

    crop = image[y1:y2, x1:x2]
    if crop.size == 0:
        return "unknown", 0.0, 0.0

    hsv_crop = crop.astype(np.float32)
    blue = hsv_crop[:, :, 0]
    green = hsv_crop[:, :, 1]
    red = hsv_crop[:, :, 2]

    brightness = float(np.mean((red + green + blue) / 3))
    max_channel = np.maximum(np.maximum(red, green), blue)
    min_channel = np.minimum(np.minimum(red, green), blue)
    saturation = float(np.mean(np.where(max_channel == 0, 0, (max_channel - min_channel) / max_channel * 255)))

    if brightness < 60:
        return "black", brightness, saturation
    if brightness > 190 and saturation < 40:
        return "white", brightness, saturation
    if saturation < 50:
        return "silver" if brightness > 150 else "gray", brightness, saturation

    red_mean = float(np.mean(red))
    green_mean = float(np.mean(green))
    blue_mean = float(np.mean(blue))

    if red_mean >= green_mean and red_mean >= blue_mean:
        if green_mean > blue_mean * 1.15:
            return "yellow", brightness, saturation
        return "red", brightness, saturation
    if green_mean >= red_mean and green_mean >= blue_mean:
        return "green", brightness, saturation
    return "blue", brightness, saturation


def infer_car_color(image: np.ndarray, car_bbox: list[int]) -> str:
    """Backwards-compatible wrapper used by existing callers and tests."""
    label, _, _ = _analyze_color_crop(image, car_bbox)
    return label


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

    # Image-level colour sanity check — tolerant by default.
    # We only hard-reject on CLEAR cross-family mismatch (warm vs cool,
    # e.g. red vs blue). Anything involving the neutral family, any
    # unknown-family detection, and anything shot under low-confidence
    # lighting is accepted — that's where false positives live.
    inferred_color, brightness, saturation = _analyze_color_crop(image, car_bbox)
    normalized_inferred_color = _normalize_color(inferred_color)

    # Exact label match — always accept.
    if normalized_inferred_color == normalized_expected_color:
        return True, "OK", []

    expected_family = _color_family(expected_color)
    inferred_family = _color_family(inferred_color)

    # Both sides in the neutral family (white/silver/gray/black) —
    # accept. This is the single biggest source of false rejects under
    # indoor / overcast lighting.
    if expected_family == "neutral" and inferred_family == "neutral":
        return True, "OK", []

    # Low-confidence detection: very dark scene (shadow), overexposed
    # scene, or low saturation all mean our single-crop HSV heuristic is
    # unreliable. Give the benefit of the doubt instead of invalidating
    # the whole session.
    if brightness < 50 or brightness > 205 or saturation < 35:
        return True, "OK", []

    # Unknown or empty-crop detection — don't penalise the user.
    if inferred_family == "unknown":
        return True, "OK", []

    # Same non-neutral family (e.g. two warm shades) — accept; we can't
    # reliably differentiate e.g. orange-red from red.
    if expected_family == inferred_family:
        return True, "OK", []

    # At this point expected and inferred are in different non-neutral
    # families. That's an obvious mismatch — reject.
    return False, "Car color mismatch", [f"detected color: {inferred_color}"]


def should_hard_reject_color_mismatch(expected: str, detected: str) -> bool:
    """Pure helper for tests and introspection. Mirrors the acceptance
    policy above without running the CV pipeline."""
    expected_norm = _normalize_color(expected)
    detected_norm = _normalize_color(detected)
    if expected_norm == detected_norm:
        return False
    expected_family = _color_family(expected)
    detected_family = _color_family(detected)
    if expected_family == "neutral" and detected_family == "neutral":
        return False
    if detected_family == "unknown":
        return False
    return expected_family != detected_family
