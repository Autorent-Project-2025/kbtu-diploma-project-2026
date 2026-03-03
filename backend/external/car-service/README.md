# Car Service

## Назначение
Сервис каталога автомобилей. Отвечает за:
- получение списка машин и деталей;
- создание/обновление/удаление машин;
- получение справочника features;
- привязку изображений к машине.

## Стек
- ASP.NET Core (`net10.0`)
- PostgreSQL
- Flyway (миграции)
- JWT авторизация

## API
Нативный base path сервиса: `/`.
Через gateway сервис доступен по префиксу `/cars`.

Маршруты:
- `GET /` (query: `brand`, `model`, `sortBy`, `sortOrder`, `page`, `pageSize`)
- `GET /{id}`
- `POST /` (policy `cars:create`)
- `POST /$batch` (policy `cars:create`)
- `PUT /{id}` (policy `cars:update`)
- `DELETE /{id}` (policy `cars:delete`)
- `GET /features`
- `POST /{carId}/images` (policy `cars:image:create`)
- `GET /{carId}/images`

`sortBy` поддерживает значения enum: `Rating`, `PriceHour`, `Year`.

Пример создания машины:

```json
{
  "brand": "Toyota",
  "model": "Camry",
  "year": 2023,
  "priceHour": 2500,
  "priceDay": 30000,
  "description": "Sedan",
  "features": []
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
docker compose up --build car-db car-flyway car-service
```

### Автономно
Из `backend/external/car-service`:

```bash
cp .env.example .env
docker compose -f docker-compose.yaml up --build
```

Сервис будет доступен на порту `EXTERNAL_PORT` (по умолчанию `1298`).
