from __future__ import annotations

from fastapi import FastAPI, HTTPException, Query

from app.schemas import MarketValueEstimateRequest, MarketValueEstimateResponse
from app.services import (
    ComparableListingsNotFoundError,
    InvalidComparableQueryError,
    KolesaMarketValueService,
    UpstreamMarketplaceError,
)
from app.settings import Settings


def create_app(settings: Settings | None = None) -> FastAPI:
    resolved_settings = settings or Settings.from_env()
    market_value_service = KolesaMarketValueService(resolved_settings)

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
            "service": resolved_settings.service_name,
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
        return estimate_market_value(market_value_service, request)

    @app.post("/market-value/estimate", response_model=MarketValueEstimateResponse)
    def estimate_market_value_post(
        request: MarketValueEstimateRequest,
    ) -> MarketValueEstimateResponse:
        return estimate_market_value(market_value_service, request)

    return app


def estimate_market_value(
    market_value_service: KolesaMarketValueService,
    request: MarketValueEstimateRequest,
) -> MarketValueEstimateResponse:
    try:
        return market_value_service.estimate_market_value(
            request.brand,
            request.model,
            request.year,
        )
    except InvalidComparableQueryError as exc:
        raise HTTPException(status_code=400, detail=str(exc)) from exc
    except ComparableListingsNotFoundError as exc:
        raise HTTPException(status_code=404, detail=str(exc)) from exc
    except UpstreamMarketplaceError as exc:
        raise HTTPException(status_code=502, detail=str(exc)) from exc
