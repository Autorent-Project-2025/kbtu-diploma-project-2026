from functools import lru_cache
from pathlib import Path

from pydantic import Field
from pydantic_settings import BaseSettings, SettingsConfigDict


BASE_DIR = Path(__file__).resolve().parents[2]


class Settings(BaseSettings):
    model_config = SettingsConfigDict(
        env_file=".env",
        env_file_encoding="utf-8",
        extra="ignore",
    )

    app_title: str = "Carsharing AI Inspection API"
    min_photos: int = 4
    max_photos: int = 8

    min_width: int = 640
    min_height: int = 480
    min_sharpness: float = 80.0
    min_brightness: float = 40.0
    max_brightness: float = 220.0

    detection_confidence_threshold: float = 0.5
    min_car_area_ratio: float = 0.15
    max_obstacle_overlap_ratio: float = 0.05
    damage_confidence_threshold: float = 0.25
    color_match_threshold: float = 0.5

    coco_weights_path: Path = Field(default=BASE_DIR / "weights" / "yolov8n.pt")
    damage_weights_path: Path = Field(default=BASE_DIR / "weights" / "yolov8m_damage_v1.pt")

    # Static registry kept only for local development smoke-tests. In
    # production the booking-service is authoritative — it passes
    # car_model / car_color in the request and we validate against that,
    # not against the JSON file.
    car_registry_path: Path = Field(default=BASE_DIR / "config" / "car_registry.json")
    use_registry_validation: bool = False

    # Shared-secret internal auth. Matches the X-Internal-Api-Key pattern
    # used by the rest of the monorepo. When unset, auth is disabled
    # (local development). Set in docker-compose env for real deployments.
    internal_api_key: str | None = None


@lru_cache(maxsize=1)
def get_settings() -> Settings:
    return Settings()
