import numpy as np

from src.services.verification import infer_car_color, verify_attributes


def test_infer_car_color_detects_white():
    image = np.full((100, 100, 3), 240, dtype=np.uint8)

    color = infer_car_color(image, [0, 0, 100, 100])

    assert color == "white"


def test_verify_attributes_checks_model_and_color():
    image = np.full((100, 100, 3), 240, dtype=np.uint8)

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


def test_verify_attributes_rejects_wrong_model():
    image = np.full((100, 100, 3), 240, dtype=np.uint8)

    is_valid, reason, details = verify_attributes(
        image=image,
        car_id="CAR_001",
        car_bbox=[0, 0, 100, 100],
        expected_model="BMW X5",
        expected_color="white",
    )

    assert is_valid is False
    assert reason == "Car model mismatch"
    assert details
