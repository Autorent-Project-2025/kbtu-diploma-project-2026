"""Contract tests for the /inspect-session endpoint."""
from io import BytesIO

from fastapi.testclient import TestClient

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


def test_healthcheck(monkeypatch):
    monkeypatch.setattr("src.main.warmup_models", lambda: None)
    with TestClient(app) as client:
        response = client.get("/health")

    assert response.status_code == 200
    assert response.json() == {"status": "ok"}


def test_inspect_session_requires_all_five_slot_files(monkeypatch):
    """Missing any slot field must produce a 422 from FastAPI validation."""
    monkeypatch.setattr("src.main.warmup_models", lambda: None)
    with TestClient(app) as client:
        response = client.post(
            "/inspect-session",
            data={"car_id": "CAR_001", "car_model": "Toyota Camry", "car_color": "white"},
            files=[("photo_front", ("front.jpg", BytesIO(b"x"), "image/jpeg"))],
        )

    assert response.status_code == 422


def test_inspect_session_invalid_session_when_all_photos_broken(monkeypatch):
    """Corrupted images are rejected and the session verdict is INVALID_SESSION."""
    monkeypatch.setattr("src.main.warmup_models", lambda: None)
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


def test_inspect_session_internal_auth_rejects_missing_key(monkeypatch):
    """When the shared secret is configured, requests without it are 401."""
    monkeypatch.setattr("src.main.warmup_models", lambda: None)
    # Re-create the settings cache with the env var set so FastAPI
    # dependency guard picks up the secret.
    from src.core import config

    config.get_settings.cache_clear()
    monkeypatch.setenv("INTERNAL_API_KEY", "secret-123")
    config.get_settings.cache_clear()
    # Reimport the module-level ``settings`` so the dependency guard
    # sees the new key.
    from src import main as main_module

    main_module.settings = config.get_settings()

    try:
        with TestClient(app) as client:
            response = client.post(
                "/inspect-session",
                data={"car_id": "C1", "car_model": "M", "car_color": "white"},
                files=_slot_files(),
            )
        assert response.status_code == 401
    finally:
        monkeypatch.delenv("INTERNAL_API_KEY", raising=False)
        config.get_settings.cache_clear()
        main_module.settings = config.get_settings()


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
    they describe two separate real-world locations. The dedup keys on
    (type, slot) so both survive."""
    damages = [
        Damage(type="scratch", confidence=0.91, bbox=[10, 10, 100, 100], slot=PhotoSlot.FRONT),
        Damage(type="scratch", confidence=0.87, bbox=[10, 10, 100, 100], slot=PhotoSlot.BACK),
    ]

    result = deduplicate_damages(damages)

    assert len(result) == 2
