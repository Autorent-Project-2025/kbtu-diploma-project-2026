"""Contract tests for the /inspect-session endpoint."""
from io import BytesIO

import pytest
from fastapi.testclient import TestClient

from src.core import config
from src.main import app, deduplicate_damages
from src.schemas.inspection import Damage, PhotoSlot


def _slot_files() -> list[tuple[str, tuple[str, BytesIO, str]]]:
    """The five slot-labelled file fields the endpoint requires."""
    return [
        ("photo_front", ("front.jpg", BytesIO(b"fake"), "image/jpeg")),
        ("photo_back", ("back.jpg", BytesIO(b"fake"), "image/jpeg")),
        ("photo_side_left", ("side_left.jpg", BytesIO(b"fake"), "image/jpeg")),
        ("photo_side_right", ("side_right.jpg", BytesIO(b"fake"), "image/jpeg")),
        ("photo_interior", ("interior.jpg", BytesIO(b"fake"), "image/jpeg")),
    ]


def _reset_settings(monkeypatch, **env):
    """Helper to repoint src.main.settings at a freshly-loaded Settings
    instance reflecting the given env vars. Used so auth-mode tests can
    switch between enforced / dev-bypass / misconfigured postures."""
    from src import main as main_module

    for key in (
        "INTERNAL_API_KEY",
        "ENVIRONMENT",
        "ALLOW_UNAUTHENTICATED_INTERNAL_DEV",
    ):
        monkeypatch.delenv(key, raising=False)
    for key, value in env.items():
        monkeypatch.setenv(key, value)
    config.get_settings.cache_clear()
    main_module.settings = config.get_settings()


@pytest.fixture(autouse=True)
def _skip_warmup(monkeypatch):
    """Unit tests don't need to actually load YOLO weights."""
    monkeypatch.setattr("src.main.warmup_models", lambda: None)
    yield


def test_healthcheck_is_always_liveness_only():
    with TestClient(app) as client:
        response = client.get("/health")

    assert response.status_code == 200
    assert response.json() == {"status": "ok"}


def test_readiness_signals_model_warmup(monkeypatch):
    from src import main as main_module

    # Simulate pre-warmup state.
    monkeypatch.setattr(main_module, "_models_warm", False, raising=False)
    with TestClient(app) as client:
        # Lifespan ran warmup (mocked no-op), latch becomes True.
        response = client.get("/ready")
    assert response.status_code == 200


def test_inspect_session_requires_all_five_slot_files(monkeypatch):
    _reset_settings(monkeypatch, ENVIRONMENT="development", ALLOW_UNAUTHENTICATED_INTERNAL_DEV="true")
    with TestClient(app) as client:
        response = client.post(
            "/inspect-session",
            data={"car_id": "CAR_001", "car_model": "Toyota Camry", "car_color": "white"},
            files=[("photo_front", ("front.jpg", BytesIO(b"x"), "image/jpeg"))],
        )

    assert response.status_code == 422


def test_inspect_session_invalid_session_when_all_photos_broken(monkeypatch):
    _reset_settings(monkeypatch, ENVIRONMENT="development", ALLOW_UNAUTHENTICATED_INTERNAL_DEV="true")
    with TestClient(app) as client:
        response = client.post(
            "/inspect-session",
            data={"car_id": "CAR_001", "car_model": "Toyota Camry", "car_color": "white"},
            files=_slot_files(),
        )

    assert response.status_code == 200
    body = response.json()
    assert body["verdict"] == "INVALID_SESSION"
    assert body["valid_photos_count"] == 0
    # Each rejected photo carries its slot label — critical for the
    # client UI to highlight the right dropzone.
    slots_seen = {photo["slot"] for photo in body["rejected_photos"]}
    assert slots_seen == {"front", "back", "side_left", "side_right", "interior"}
    assert "processed_at_utc" in body


# --- Auth posture --------------------------------------------------------


def test_auth_enforced_rejects_missing_header(monkeypatch):
    """When INTERNAL_API_KEY is set, requests without the header get 401."""
    _reset_settings(monkeypatch, INTERNAL_API_KEY="secret-123")

    with TestClient(app) as client:
        response = client.post(
            "/inspect-session",
            data={"car_id": "C1", "car_model": "M", "car_color": "white"},
            files=_slot_files(),
        )
    assert response.status_code == 401


def test_auth_enforced_accepts_correct_header(monkeypatch):
    _reset_settings(monkeypatch, INTERNAL_API_KEY="secret-123")

    with TestClient(app) as client:
        response = client.post(
            "/inspect-session",
            data={"car_id": "C1", "car_model": "M", "car_color": "white"},
            files=_slot_files(),
            headers={"X-Internal-Api-Key": "secret-123"},
        )
    # Photos are garbage so we expect a verdict, but we must have passed auth.
    assert response.status_code == 200


def test_auth_fails_closed_when_neither_secret_nor_dev_bypass(monkeypatch):
    """The regression that prompted this fix: no secret + no dev bypass
    must 503, not silently open the endpoint."""
    _reset_settings(monkeypatch)  # both env vars cleared, environment defaults to production

    with TestClient(app) as client:
        response = client.post(
            "/inspect-session",
            data={"car_id": "C1", "car_model": "M", "car_color": "white"},
            files=_slot_files(),
        )
    assert response.status_code == 503


def test_dev_bypass_requires_both_flags_and_environment(monkeypatch):
    # Flag on, environment not development — still fails closed.
    _reset_settings(monkeypatch, ALLOW_UNAUTHENTICATED_INTERNAL_DEV="true", ENVIRONMENT="production")

    with TestClient(app) as client:
        response = client.post(
            "/inspect-session",
            data={"car_id": "C1", "car_model": "M", "car_color": "white"},
            files=_slot_files(),
        )
    assert response.status_code == 503


def test_dev_bypass_allows_when_fully_opted_in(monkeypatch):
    _reset_settings(monkeypatch, ENVIRONMENT="development", ALLOW_UNAUTHENTICATED_INTERNAL_DEV="true")

    with TestClient(app) as client:
        response = client.post(
            "/inspect-session",
            data={"car_id": "C1", "car_model": "M", "car_color": "white"},
            files=_slot_files(),
        )
    assert response.status_code == 200


# --- Deduplication --------------------------------------------------------


def test_deduplicate_damages_removes_overlapping_duplicates():
    damages = [
        Damage(type="scratch", confidence=0.91, bbox=[10, 10, 100, 100], slot=PhotoSlot.FRONT, source_file="a.jpg"),
        Damage(type="scratch", confidence=0.72, bbox=[12, 12, 98, 98], slot=PhotoSlot.FRONT, source_file="b.jpg"),
        Damage(type="dent", confidence=0.88, bbox=[200, 200, 260, 260], slot=PhotoSlot.BACK, source_file="c.jpg"),
    ]

    result = deduplicate_damages(damages)

    assert len(result) == 2
    assert [item.type for item in result] == ["scratch", "dent"]


def test_deduplicate_keeps_same_type_across_different_slots():
    """Damage on front and back both labelled "scratch" is NOT a duplicate —
    they describe two separate real-world locations."""
    damages = [
        Damage(type="scratch", confidence=0.91, bbox=[10, 10, 100, 100], slot=PhotoSlot.FRONT),
        Damage(type="scratch", confidence=0.87, bbox=[10, 10, 100, 100], slot=PhotoSlot.BACK),
    ]

    result = deduplicate_damages(damages)

    assert len(result) == 2
