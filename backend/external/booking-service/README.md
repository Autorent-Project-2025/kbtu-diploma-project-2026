# Booking Service

## Назначение
Сервис бронирований автомобилей. Отвечает за:
- создание брони;
- получение своих бронирований;
- подтверждение, завершение и отмену брони;
- проверку доступности автомобиля по интервалу дат.

## Стек
- ASP.NET Core (`net10.0`)
- PostgreSQL
- Flyway (миграции)
- JWT авторизация

## API
Нативный base path сервиса: `/`.
Через gateway сервис доступен по префиксу `/bookings`.

Маршруты:
- `POST /` (policy `bookings:create`)
- `GET /my`
- `GET /{id:int}`
- `POST /{id:int}/cancel`
- `POST /{id:int}/confirm`
- `POST /{id:int}/complete`
- `GET /available?carId={id}&start={iso}&end={iso}` (`AllowAnonymous`)

Пример создания брони:

```json
{
  "carId": 12,
  "startDate": "2026-03-10T10:00:00Z",
  "endDate": "2026-03-10T14:00:00Z"
}
```

## Переменные окружения
См. `./.env.example`:
- `Jwt__PublicKey`
- `Cors__AllowedOrigins__0`
- `EXTERNAL_PORT`
- `POSTGRES_USER`
- `POSTGRES_PASSWORD`
- `POSTGRES_DB`
- `POSTGRES_PORT`

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

Сервис будет доступен на порту `EXTERNAL_PORT` (по умолчанию `1821`).
