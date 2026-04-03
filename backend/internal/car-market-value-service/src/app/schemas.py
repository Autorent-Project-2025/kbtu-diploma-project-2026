from __future__ import annotations

from datetime import datetime, timezone
from typing import Literal

from pydantic import BaseModel, Field, field_validator

from app.text import normalize_text


class MarketValueEstimateRequest(BaseModel):
    brand: str = Field(min_length=1, max_length=100)
    model: str = Field(min_length=1, max_length=100)
    year: int = Field(ge=1886)

    @field_validator("brand", "model")
    @classmethod
    def normalize_name(cls, value: str) -> str:
        normalized = normalize_text(value)
        if not normalized:
            raise ValueError("Value must not be empty.")

        return normalized

    @field_validator("year")
    @classmethod
    def validate_year(cls, value: int) -> int:
        max_allowed_year = datetime.now(timezone.utc).year + 1
        if value > max_allowed_year:
            raise ValueError(f"Year must be less than or equal to {max_allowed_year}.")

        return value


class MarketValueEstimateResponse(BaseModel):
    brand: str
    model: str
    year: int
    marketValueKzt: int
    medianPriceKzt: int
    averagePriceKzt: int
    minPriceKzt: int
    maxPriceKzt: int
    sampleCount: int
    filteredSampleCount: int
    outliersRemoved: int
    confidence: Literal["low", "medium", "high"]
    currency: str = "KZT"
    source: str = "kolesa.kz"
    sourceUrl: str
    fetchedAt: datetime
