"""Verification service tests — the production path (registry disabled)."""
import numpy as np
import pytest

from src.core import config
from src.services.verification import (
    infer_car_color,
    should_hard_reject_color_mismatch,
    verify_attributes,
)


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


def test_verify_attributes_accepts_exact_color_match():
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


# --- New tolerant colour policy ---------------------------------------
#
# These tests codify the relaxed production policy: only clear
# cross-family mismatches (warm vs cool) cause a hard reject.


def test_verify_attributes_accepts_neutral_family_mismatch_white_vs_silver():
    # Bright crop that reads as "white" from our heuristic.
    image = np.full((100, 100, 3), 240, dtype=np.uint8)

    is_valid, reason, _ = verify_attributes(
        image=image,
        car_id="CAR_001",
        car_bbox=[0, 0, 100, 100],
        expected_model="Toyota Camry",
        expected_color="silver",
    )

    assert is_valid is True, f"neutral/neutral should pass, got: {reason}"


def test_verify_attributes_accepts_neutral_family_mismatch_gray_vs_black():
    # Very dark crop — the heuristic reports "black"; user entered "gray".
    image = np.full((100, 100, 3), 20, dtype=np.uint8)

    is_valid, _, _ = verify_attributes(
        image=image,
        car_id="CAR_001",
        car_bbox=[0, 0, 100, 100],
        expected_model="Toyota Camry",
        expected_color="gray",
    )

    assert is_valid is True


def test_verify_attributes_tolerates_low_saturation_scene():
    # Desaturated outdoor-in-fog looking crop. Expected is a bright color
    # but the scene is too ambiguous to reject on — the policy gives the
    # benefit of the doubt.
    image = np.full((100, 100, 3), 128, dtype=np.uint8)  # gray, low saturation

    is_valid, _, _ = verify_attributes(
        image=image,
        car_id="CAR_001",
        car_bbox=[0, 0, 100, 100],
        expected_model="Toyota Camry",
        expected_color="red",
    )

    assert is_valid is True


def test_verify_attributes_still_rejects_warm_vs_cool_mismatch():
    # Saturated red crop, caller says "blue" — this is the canonical
    # example of a mismatch worth surfacing to the manager.
    image = np.zeros((100, 100, 3), dtype=np.uint8)
    image[:, :, 2] = 220  # red channel high in BGR

    is_valid, reason, _ = verify_attributes(
        image=image,
        car_id="CAR_001",
        car_bbox=[0, 0, 100, 100],
        expected_model="Toyota Camry",
        expected_color="blue",
    )

    assert is_valid is False
    assert reason == "Car color mismatch"


# --- should_hard_reject_color_mismatch pure helper --------------------


def test_hard_reject_policy_matches_doc():
    # Same label
    assert not should_hard_reject_color_mismatch("white", "white")
    # Neutral vs neutral
    assert not should_hard_reject_color_mismatch("white", "silver")
    assert not should_hard_reject_color_mismatch("gray", "black")
    # Unknown family inferred
    assert not should_hard_reject_color_mismatch("red", "magenta")  # "magenta" is not in the alias list
    # Same non-neutral family
    assert not should_hard_reject_color_mismatch("red", "yellow")  # both warm
    assert not should_hard_reject_color_mismatch("blue", "green")  # both cool
    # Cross-family (the only hard-reject case)
    assert should_hard_reject_color_mismatch("red", "blue")
    assert should_hard_reject_color_mismatch("yellow", "green")
    # Neutral vs non-neutral — hard reject too (different families)
    assert should_hard_reject_color_mismatch("white", "red")


# --- Unchanged input validation ---------------------------------------


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


def test_verify_attributes_accepts_typical_license_plates():
    """Booking-service now sends the sanitised license plate as car_id
    (e.g. "A123BC", "123ABC77"). The 3-char minimum regex accepts all
    realistic plate shapes."""
    image = np.full((100, 100, 3), 240, dtype=np.uint8)

    for car_id in ("A123BC", "123ABC77", "KZ01-ABC", "PCAR-42"):
        is_valid, reason, _ = verify_attributes(
            image=image,
            car_id=car_id,
            car_bbox=[0, 0, 100, 100],
            expected_model="Toyota Camry",
            expected_color="white",
        )
        assert is_valid is True, f"car_id={car_id} must be accepted, got: {reason}"
