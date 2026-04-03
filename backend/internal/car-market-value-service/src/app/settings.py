from __future__ import annotations

import os
from dataclasses import dataclass

DEFAULT_USER_AGENT = (
    "Mozilla/5.0 (Windows NT 10.0; Win64; x64) "
    "AppleWebKit/537.36 (KHTML, like Gecko) "
    "Chrome/135.0.0.0 Safari/537.36 AutoRentMarketValueService/1.0"
)


@dataclass(frozen=True)
class Settings:
    service_name: str
    base_url: str
    max_pages: int
    request_timeout_seconds: float
    user_agent: str
    port: int

    @staticmethod
    def from_env() -> "Settings":
        base_url = os.getenv("KOLESA_BASE_URL", "https://kolesa.kz").strip().rstrip("/")
        max_pages = parse_positive_int(os.getenv("KOLESA_MAX_PAGES"), default=3)
        request_timeout_seconds = parse_positive_float(
            os.getenv("REQUEST_TIMEOUT_SECONDS"),
            default=15.0,
        )
        user_agent = os.getenv("REQUEST_USER_AGENT", DEFAULT_USER_AGENT).strip()
        port = parse_positive_int(os.getenv("PORT"), default=8080)

        return Settings(
            service_name="car-market-value-service",
            base_url=base_url,
            max_pages=max_pages,
            request_timeout_seconds=request_timeout_seconds,
            user_agent=user_agent,
            port=port,
        )


def parse_positive_int(raw_value: str | None, default: int) -> int:
    if raw_value is None or not raw_value.strip():
        return default

    parsed = int(raw_value)
    if parsed <= 0:
        raise ValueError("Integer environment value must be positive.")

    return parsed


def parse_positive_float(raw_value: str | None, default: float) -> float:
    if raw_value is None or not raw_value.strip():
        return default

    parsed = float(raw_value)
    if parsed <= 0:
        raise ValueError("Float environment value must be positive.")

    return parsed
