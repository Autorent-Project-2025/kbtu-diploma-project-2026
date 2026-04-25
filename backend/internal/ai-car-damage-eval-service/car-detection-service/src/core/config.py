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
    # used by the rest of the monorepo. Must be set in every real
    # deployment (docker-compose wires it in automatically). Missing the
    # secret is fail-closed unless the explicit dev-bypass opt-in below
    # is active.
    internal_api_key: str | None = None

    # Runtime environment tag. Only "development" permits the dev-bypass
    # flag. Any other value (including empty) is treated as production.
    environment: str = "production"

    # Dev-bypass: explicitly allow unauthenticated requests during local
    # development. Takes effect only when both:
    #   ALLOW_UNAUTHENTICATED_INTERNAL_DEV=true
    #   ENVIRONMENT=development
    # Any other combination fails closed.
    allow_unauthenticated_internal_dev: bool = False

    def auth_is_enforced(self) -> bool:
        """True when the request guard must check X-Internal-Api-Key."""
        if self.internal_api_key:
            return True
        # No secret set: enforce auth (=> all requests 503) unless the
        # explicit dev-bypass combo is active.
        if self.is_dev_bypass_active():
            return False
        return True

    def is_dev_bypass_active(self) -> bool:
        return (
            self.allow_unauthenticated_internal_dev
            and self.environment.strip().lower() == "development"
        )


@lru_cache(maxsize=1)
def get_settings() -> Settings:
    return Settings()
