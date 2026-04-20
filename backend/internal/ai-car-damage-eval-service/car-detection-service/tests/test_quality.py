import numpy as np

from src.services.quality import check_quality


def test_check_quality_rejects_low_resolution():
    image = np.zeros((100, 100, 3), dtype=np.uint8)

    is_good, reason = check_quality(image)

    assert not is_good
    assert "Low resolution" in reason


def test_check_quality_rejects_bad_lighting():
    image = np.full((600, 800, 3), 255, dtype=np.uint8)

    is_good, reason = check_quality(image)

    assert not is_good
    assert "Bad lighting" in reason
