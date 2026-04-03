from __future__ import annotations

import math
import re
import statistics
from datetime import datetime, timezone
from urllib.parse import urlencode

import requests
from bs4 import BeautifulSoup

from app.schemas import MarketValueEstimateResponse
from app.settings import Settings
from app.text import normalize_text, slugify

PRICE_SELECTORS = (
    ".a-card__price",
    "[data-test='a-card-price']",
    "[data-test='offer-price']",
)

PRICE_DIGITS_PATTERN = re.compile(r"\d+")


class InvalidComparableQueryError(ValueError):
    pass


class ComparableListingsNotFoundError(LookupError):
    pass


class UpstreamMarketplaceError(RuntimeError):
    pass


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

        try:
            first_page_url = build_kolesa_url(
                self._settings.base_url,
                normalized_brand,
                normalized_model,
                year,
                page=1,
            )
        except ValueError as exc:
            raise InvalidComparableQueryError(str(exc)) from exc

        all_prices: list[int] = []
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
            raise ComparableListingsNotFoundError(
                "Comparable listings were not found for the specified brand/model/year combination."
            )

        filtered_prices = filter_outliers(all_prices)
        median_price = int(round(statistics.median(filtered_prices)))
        average_price = int(round(sum(filtered_prices) / len(filtered_prices)))

        return MarketValueEstimateResponse(
            brand=normalized_brand,
            model=normalized_model,
            year=year,
            marketValueKzt=median_price,
            medianPriceKzt=median_price,
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
            raise UpstreamMarketplaceError(
                f"kolesa.kz returned HTTP {status_code} while fetching comparables."
            ) from exc
        except requests.RequestException as exc:
            raise UpstreamMarketplaceError(
                f"Failed to fetch comparables from kolesa.kz: {exc}"
            ) from exc

        return extract_prices_from_html(response.text)


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


def resolve_confidence(sample_count: int) -> str:
    if sample_count >= 15:
        return "high"
    if sample_count >= 7:
        return "medium"
    return "low"
