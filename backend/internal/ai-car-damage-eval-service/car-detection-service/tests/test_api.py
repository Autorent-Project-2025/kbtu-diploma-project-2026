from io import BytesIO

from fastapi.testclient import TestClient

from src.main import app, deduplicate_damages
from src.schemas.inspection import Damage


def test_healthcheck(monkeypatch):
    monkeypatch.setattr("src.main.warmup_models", lambda: None)
    with TestClient(app) as client:
        response = client.get("/health")

    assert response.status_code == 200
    assert response.json() == {"status": "ok"}


def test_inspect_session_requires_photo_range(monkeypatch):
    monkeypatch.setattr("src.main.warmup_models", lambda: None)
    with TestClient(app) as client:
        response = client.post(
            "/inspect-session",
            data={"car_id": "CAR_001", "car_model": "Toyota Camry", "car_color": "white"},
            files=[("files", ("a.jpg", BytesIO(b"not-an-image"), "image/jpeg"))],
        )

    assert response.status_code == 400
    assert "Require 4 to 8 photos." in response.json()["detail"]


def test_deduplicate_damages_removes_overlapping_duplicates():
    damages = [
        Damage(type="scratch", confidence=0.91, bbox=[10, 10, 100, 100], source_file="a.jpg"),
        Damage(type="scratch", confidence=0.72, bbox=[12, 12, 98, 98], source_file="b.jpg"),
        Damage(type="dent", confidence=0.88, bbox=[200, 200, 260, 260], source_file="c.jpg"),
    ]

    result = deduplicate_damages(damages)

    assert len(result) == 2
    assert [item.type for item in result] == ["scratch", "dent"]
