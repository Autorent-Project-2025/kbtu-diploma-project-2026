"""Verification service tests — the production path (registry disabled)."""
import numpy as np
import pytest

from src.core import config
from src.services.verification import infer_car_color, verify_attributes


@pytest.fixture(autouse=True)
def _disable_registry(monkeypatch):
    """Production default: the JSON registry is not consulted."""
    monkeypatch.setenv("USE_REGISTRY_VALIDATION", "false")
    config.get_settings.cache_clear()
    yield
    config.get_settings.cache_clear()


def test_infer_car_color_detects_white():
    image = np.full((100, 100, 3), 240, dtype=np.uint8)

    color = infer_car_color(image, [0, 0, 100, 100])

    assert color == "white"


def test_verify_attributes_accepts_when_detected_color_matches_caller():
    """The caller is the authoritative source for car attributes — we
    pass by simply matching what they said against what we see."""
    image = np.full((100, 100, 3), 240, dtype=np.uint8)  # white

    is_valid, reason, details = verify_attributes(
        image=image,
        car_id="CAR_001",
        car_bbox=[0, 0, 100, 100],
        expected_model="Toyota Camry",
        expected_color="white",
    )

    assert is_valid is True
    assert reason == "OK"
    assert details == []


def test_verify_attributes_rejects_when_visible_color_differs():
    """Even without the registry we still catch caller/reality mismatch."""
    image = np.full((100, 100, 3), 240, dtype=np.uint8)  # white

    is_valid, reason, _ = verify_attributes(
        image=image,
        car_id="CAR_001",
        car_bbox=[0, 0, 100, 100],
        expected_model="Toyota Camry",
        expected_color="black",
    )

    assert is_valid is False
    assert reason == "Car color mismatch"


def test_verify_attributes_rejects_empty_model():
    image = np.full((100, 100, 3), 240, dtype=np.uint8)

    is_valid, reason, _ = verify_attributes(
        image=image,
        car_id="CAR_001",
        car_bbox=[0, 0, 100, 100],
        expected_model="",
        expected_color="white",
    )

    assert is_valid is False
    assert reason == "Car model is empty"


def test_verify_attributes_rejects_malformed_car_id():
    image = np.full((100, 100, 3), 240, dtype=np.uint8)

    is_valid, reason, _ = verify_attributes(
        image=image,
        car_id="!!invalid!!",
        car_bbox=[0, 0, 100, 100],
        expected_model="Toyota Camry",
        expected_color="white",
    )

    assert is_valid is False
    assert reason == "Car ID format is invalid"
