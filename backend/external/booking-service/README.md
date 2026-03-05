# Booking Service

## Назначение
Сервис бронирований партнерских автомобилей. Отвечает за:
- создание брони по `partnerCarId`;
- получение бронирований текущего пользователя;
- смену статуса (confirm, complete, cancel);
- проверку доступности машины по временному интервалу;
- внутренние read-эндпоинты для `car-service`;
- массовую проверку доступности по списку машин (`check-availability`).

## Стек
- ASP.NET Core (`net10.0`)
- PostgreSQL
- Flyway (SQL миграции)
- JWT авторизация

## Важные изменения схемы
Актуальная схема бронирований внедрена в миграции `V3__refactor_bookings_for_car_sharing.sql`:
- `car_id` -> `partner_car_id`;
- `user_id` хранится как `UUID`;
- `start_date/end_date` -> `start_time/end_time` (`TIMESTAMPTZ`);
- добавлены `partner_id`, `booking_range`, `price_hour`, `total_price`, `created_at`;
- добавлен exclusion constraint `prevent_overlapping_bookings`:
  - не позволяет пересекать активные брони по одной машине;
  - статусы блокировки: `pending`, `confirmed`, `active`.

## API
Нативный base path сервиса: `/`.
Через gateway сервис обычно доступен по префиксу `/bookings`.

### Основные маршруты
- `POST /` (policy `bookings:create`)
- `GET /my`
- `GET /{id:int}`
- `POST /{id:int}/cancel`
- `POST /{id:int}/confirm`
- `POST /{id:int}/complete`
- `GET /available?partnerCarId={id}&startTime={iso}&endTime={iso}` (`AllowAnonymous`)

### Internal API (для межсервисного доступа)
Требуется заголовок `X-Internal-Api-Key`.

- `GET /internal/bookings/by-partner-car/{partnerCarId}`
- `GET /internal/bookings/by-car/{partnerCarId}` (alias для обратной совместимости)
- `GET /internal/bookings/counts?partnerCarIds=1,2,3`
- `GET /internal/bookings/counts?carIds=1,2,3` (alias для обратной совместимости)
- `POST /internal/bookings/check-availability`

## Контракты
### Создание брони (`POST /`)

```json
{
  "partnerCarId": 12,
  "partnerId": "2c51e4d3-250d-4f6b-9f4c-1c8c7e62e35a",
  "priceHour": 3500,
  "startTime": "2026-03-10T10:00:00Z",
  "endTime": "2026-03-10T14:00:00Z"
}
```

### Статусы бронирования
- `Pending`
- `Confirmed`
- `Active`
- `Completed`
- `Canceled`

### Массовая проверка доступности (`POST /internal/bookings/check-availability`)

Запрос:

```json
{
  "carIds": [1, 2, 3, 4],
  "startTime": "2026-03-10T10:00:00Z",
  "endTime": "2026-03-10T14:00:00Z"
}
```

Ответ:

```json
[
  {
    "partnerCarId": 1,
    "isAvailable": false,
    "nextAvailableFrom": "2026-03-10T15:30:00Z"
  },
  {
    "partnerCarId": 2,
    "isAvailable": true,
    "nextAvailableFrom": "2026-03-10T10:00:00Z"
  }
]
```

## Переменные окружения
См. `./.env.example`:
- `Jwt__PublicKey`
- `Cors__AllowedOrigins__0`
- `InternalAuth__ApiKey`
- `EXTERNAL_PORT`
- `POSTGRES_USER`
- `POSTGRES_PASSWORD`
- `POSTGRES_DB`
- `POSTGRES_PORT`

Дополнительно поддерживается fallback через `DATABASE_URL` (например для Heroku), если `ConnectionStrings:DbConnection` не задан.

## Запуск
### В составе всего проекта (рекомендуется)
Из корня репозитория:

```bash
docker compose up --build booking-db booking-flyway booking-service
```

### Автономно
Из `backend/external/booking-service`:

```bash
cp .env.example .env
docker compose -f docker-compose.yaml up --build
```

Сервис доступен на порту `EXTERNAL_PORT` (по умолчанию `1821`).

## Необходимые права
Сервис использует JWT-аутентификацию на уровне контроллера.

- Для `POST /` нужен permission `Booking.Create` (policy `bookings:create`).
- Для `GET /my`, `GET /{id}`, `POST /{id}/cancel`, `POST /{id}/confirm`, `POST /{id}/complete` требуется валидный JWT.
- `GET /available` доступен анонимно.
