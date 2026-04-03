from __future__ import annotations

import math
import os
import re
import statistics
from dataclasses import dataclass
from datetime import datetime, timezone
from typing import Literal
from urllib.parse import urlencode

import requests
import uvicorn
from bs4 import BeautifulSoup
from fastapi import FastAPI, HTTPException, Query
from pydantic import BaseModel, Field, field_validator

CYRILLIC_TO_LATIN = str.maketrans(
    {
        "а": "a",
        "ә": "a",
        "б": "b",
        "в": "v",
        "г": "g",
        "ғ": "g",
        "д": "d",
        "е": "e",
        "ё": "e",
        "ж": "zh",
        "з": "z",
        "и": "i",
        "й": "i",
        "к": "k",
        "қ": "k",
        "л": "l",
        "м": "m",
        "н": "n",
        "ң": "n",
        "о": "o",
        "ө": "o",
        "п": "p",
        "р": "r",
        "с": "s",
        "т": "t",
        "у": "u",
        "ұ": "u",
        "ү": "u",
        "ф": "f",
        "х": "h",
        "һ": "h",
        "ц": "ts",
        "ч": "ch",
        "ш": "sh",
        "щ": "shch",
        "ы": "y",
        "і": "i",
        "э": "e",
        "ю": "yu",
        "я": "ya",
        "ь": "",
        "ъ": "",
    }
)

PRICE_SELECTORS = (
    ".a-card__price",
    "[data-test='a-card-price']",
    "[data-test='offer-price']",
)

PRICE_DIGITS_PATTERN = re.compile(r"\d+")
SLUG_SANITIZE_PATTERN = re.compile(r"[^a-z0-9]+")
SPACE_PATTERN = re.compile(r"\s+")


@dataclass(frozen=True)
class Settings:
    service_name: str
    base_url: str
    max_pages: int
    request_timeout_seconds: float
    user_agent: str

    @staticmethod
    def from_env() -> "Settings":
        base_url = os.getenv("KOLESA_BASE_URL", "https://kolesa.kz").strip().rstrip("/")
        max_pages = parse_positive_int(os.getenv("KOLESA_MAX_PAGES"), default=3)
        request_timeout_seconds = parse_positive_float(
            os.getenv("REQUEST_TIMEOUT_SECONDS"),
            default=15.0,
        )
        user_agent = os.getenv(
            "REQUEST_USER_AGENT",
            (
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) "
                "AppleWebKit/537.36 (KHTML, like Gecko) "
                "Chrome/135.0.0.0 Safari/537.36 AutoRentMarketValueService/1.0"
            ),
        ).strip()

        return Settings(
            service_name="car-market-value-service",
            base_url=base_url,
            max_pages=max_pages,
            request_timeout_seconds=request_timeout_seconds,
            user_agent=user_agent,
        )


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


def normalize_text(value: str) -> str:
    return SPACE_PATTERN.sub(" ", value.strip())


def slugify(value: str) -> str:
    normalized = normalize_text(value).lower().translate(CYRILLIC_TO_LATIN)
    normalized = normalized.replace("&", " and ").replace("'", "").replace('"', "")
    normalized = SLUG_SANITIZE_PATTERN.sub("-", normalized)
    return normalized.strip("-")


def build_kolesa_url(base_url: str, brand: str, model: str, year: int, page: int) -> str:
    brand_slug = slugify(brand)
    model_slug = slugify(model)
    if not brand_slug or not model_slug:
        raise ValueError("Brand and model must produce non-empty URL slugs.")

    query_params = {
        "year[from]": str(year),
        "year[to]": str(year),
    }
    if page > 1:
        query_params["page"] = str(page)

    return f"{base_url}/cars/{brand_slug}/{model_slug}/?{urlencode(query_params)}"


def extract_prices_from_html(html: str) -> list[int]:
    soup = BeautifulSoup(html, "html.parser")

    raw_prices: list[str] = []
    for selector in PRICE_SELECTORS:
        raw_prices = [node.get_text(" ", strip=True) for node in soup.select(selector)]
        if raw_prices:
            break

    prices: list[int] = []
    for raw_price in raw_prices:
        price = parse_price_to_kzt(raw_price)
        if price is not None:
            prices.append(price)

    return prices


def parse_price_to_kzt(raw_value: str) -> int | None:
    digits = "".join(PRICE_DIGITS_PATTERN.findall(raw_value.replace("\xa0", " ")))
    if not digits:
        return None

    parsed = int(digits)
    return parsed if parsed > 0 else None


def percentile(sorted_values: list[int], fraction: float) -> float:
    if not sorted_values:
        raise ValueError("At least one value is required.")

    if len(sorted_values) == 1:
        return float(sorted_values[0])

    position = (len(sorted_values) - 1) * fraction
    lower_index = math.floor(position)
    upper_index = math.ceil(position)

    lower_value = sorted_values[lower_index]
    upper_value = sorted_values[upper_index]
    if lower_index == upper_index:
        return float(lower_value)

    weight = position - lower_index
    return lower_value + (upper_value - lower_value) * weight


def filter_outliers(prices: list[int]) -> list[int]:
    sorted_prices = sorted(prices)
    if len(sorted_prices) < 4:
        return sorted_prices

    q1 = percentile(sorted_prices, 0.25)
    q3 = percentile(sorted_prices, 0.75)
    iqr = q3 - q1
    if iqr <= 0:
        return sorted_prices

    lower_bound = max(0.0, q1 - 1.5 * iqr)
    upper_bound = q3 + 1.5 * iqr
    filtered = [price for price in sorted_prices if lower_bound <= price <= upper_bound]

    minimum_viable_size = max(3, math.ceil(len(sorted_prices) * 0.6))
    return filtered if len(filtered) >= minimum_viable_size else sorted_prices


def resolve_confidence(sample_count: int) -> Literal["low", "medium", "high"]:
    if sample_count >= 15:
        return "high"
    if sample_count >= 7:
        return "medium"
    return "low"


class KolesaMarketValueService:
    def __init__(self, settings: Settings) -> None:
        self._settings = settings
        self._session = requests.Session()
        self._session.headers.update(
            {
                "User-Agent": settings.user_agent,
                "Accept": "text/html,application/xhtml+xml,application/xml;q=0.9,*/*;q=0.8",
                "Accept-Language": "ru-RU,ru;q=0.9,en-US;q=0.8,en;q=0.7",
                "Cache-Control": "no-cache",
                "Pragma": "no-cache",
            }
        )

    def estimate_market_value(self, brand: str, model: str, year: int) -> MarketValueEstimateResponse:
        normalized_brand = normalize_text(brand)
        normalized_model = normalize_text(model)

        all_prices: list[int] = []
        try:
            first_page_url = build_kolesa_url(
                self._settings.base_url,
                normalized_brand,
                normalized_model,
                year,
                page=1,
            )
        except ValueError as exc:
            raise HTTPException(status_code=400, detail=str(exc)) from exc

        for page in range(1, self._settings.max_pages + 1):
            page_url = build_kolesa_url(
                self._settings.base_url,
                normalized_brand,
                normalized_model,
                year,
                page=page,
            )
            page_prices = self._fetch_page_prices(page_url)
            if not page_prices:
                break
            all_prices.extend(page_prices)

        if not all_prices:
            raise HTTPException(
                status_code=404,
                detail=(
                    "Comparable listings were not found for the specified "
                    "brand/model/year combination."
                ),
            )

        filtered_prices = filter_outliers(all_prices)
        market_value = int(round(statistics.median(filtered_prices)))
        average_price = int(round(sum(filtered_prices) / len(filtered_prices)))

        return MarketValueEstimateResponse(
            brand=normalized_brand,
            model=normalized_model,
            year=year,
            marketValueKzt=market_value,
            medianPriceKzt=int(round(statistics.median(filtered_prices))),
            averagePriceKzt=average_price,
            minPriceKzt=min(filtered_prices),
            maxPriceKzt=max(filtered_prices),
            sampleCount=len(all_prices),
            filteredSampleCount=len(filtered_prices),
            outliersRemoved=max(0, len(all_prices) - len(filtered_prices)),
            confidence=resolve_confidence(len(filtered_prices)),
            sourceUrl=first_page_url,
            fetchedAt=datetime.now(timezone.utc),
        )

    def _fetch_page_prices(self, url: str) -> list[int]:
        try:
            response = self._session.get(
                url,
                timeout=self._settings.request_timeout_seconds,
            )
            response.raise_for_status()
        except requests.HTTPError as exc:
            status_code = exc.response.status_code if exc.response is not None else 502
            raise HTTPException(
                status_code=502,
                detail=f"kolesa.kz returned HTTP {status_code} while fetching comparables.",
            ) from exc
        except requests.RequestException as exc:
            raise HTTPException(
                status_code=502,
                detail=f"Failed to fetch comparables from kolesa.kz: {exc}",
            ) from exc

        return extract_prices_from_html(response.text)


settings = Settings.from_env()
market_value_service = KolesaMarketValueService(settings)
app = FastAPI(
    title="Car Market Value Service",
    description=(
        "Internal service that estimates a car market value from kolesa.kz "
        "using brand, model and year."
    ),
    version="1.0.0",
)


@app.get("/")
def get_root() -> dict[str, str]:
    return {
        "service": settings.service_name,
        "status": "ok",
    }


@app.get("/healthz")
def get_health() -> dict[str, str]:
    return {"status": "ok"}


@app.get("/market-value/estimate", response_model=MarketValueEstimateResponse)
def estimate_market_value_get(
    brand: str = Query(min_length=1, max_length=100),
    model: str = Query(min_length=1, max_length=100),
    year: int = Query(ge=1886),
) -> MarketValueEstimateResponse:
    request = MarketValueEstimateRequest(brand=brand, model=model, year=year)
    return market_value_service.estimate_market_value(
        request.brand,
        request.model,
        request.year,
    )


@app.post("/market-value/estimate", response_model=MarketValueEstimateResponse)
def estimate_market_value_post(
    request: MarketValueEstimateRequest,
) -> MarketValueEstimateResponse:
    return market_value_service.estimate_market_value(
        request.brand,
        request.model,
        request.year,
    )


if __name__ == "__main__":
    port = parse_positive_int(os.getenv("PORT"), default=8080)
    uvicorn.run("main:app", host="0.0.0.0", port=port, reload=False)
