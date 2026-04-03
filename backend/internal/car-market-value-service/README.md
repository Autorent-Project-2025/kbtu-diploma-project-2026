# Car Market Value Service

## Назначение
Внутренний Python-сервис, который оценивает рыночную стоимость автомобиля по:
- марке;
- модели;
- году выпуска.

Источник данных: `kolesa.kz`.

Сервис не считает стоимость аренды. Он возвращает именно `market value`, который дальше можно использовать в `car-service` или `booking-service`.

## API

### `GET /market-value/estimate`

Query params:
- `brand`
- `model`
- `year`

Пример:

```http
GET /market-value/estimate?brand=Toyota&model=Camry&year=2017
```

Пример ответа:

```json
{
  "brand": "Toyota",
  "model": "Camry",
  "year": 2017,
  "marketValueKzt": 10500000,
  "medianPriceKzt": 10500000,
  "averagePriceKzt": 10612500,
  "minPriceKzt": 9400000,
  "maxPriceKzt": 11700000,
  "sampleCount": 16,
  "filteredSampleCount": 14,
  "outliersRemoved": 2,
  "confidence": "high",
  "currency": "KZT",
  "source": "kolesa.kz",
  "sourceUrl": "https://kolesa.kz/cars/toyota/camry/?year%5Bfrom%5D=2017&year%5Bto%5D=2017",
  "fetchedAt": "2026-04-03T12:00:00Z"
}
```

### `POST /market-value/estimate`

```json
{
  "brand": "Toyota",
  "model": "Camry",
  "year": 2017
}
```

### `GET /healthz`

Проверка доступности сервиса.

## Конфигурация

Через переменные окружения:
- `PORT` - порт приложения, по умолчанию `8080`
- `KOLESA_BASE_URL` - по умолчанию `https://kolesa.kz`
- `KOLESA_MAX_PAGES` - сколько страниц объявлений читать, по умолчанию `3`
- `REQUEST_TIMEOUT_SECONDS` - timeout внешнего HTTP-запроса, по умолчанию `15`
- `REQUEST_USER_AGENT` - user-agent для запросов к `kolesa.kz`

## Запуск

### Локально

```bash
cd backend/internal/car-market-value-service/src
pip install -r requirements.txt
uvicorn main:app --host 0.0.0.0 --port 8080
```

### Через Docker Compose

```bash
cd backend/internal/car-market-value-service
cp .env.example .env
docker compose -f docker-compose.yaml up --build
```
