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
    car_registry_path: Path = Field(default=BASE_DIR / "config" / "car_registry.json")


@lru_cache(maxsize=1)
def get_settings() -> Settings:
    return Settings()
