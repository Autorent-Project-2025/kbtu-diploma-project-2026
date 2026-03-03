# Reverse Proxy Service (API Gateway)

## Назначение
Edge-сервис, который проксирует запросы фронтендов к backend-сервисам и делает route rewrite.

## Стек
- Node.js
- TypeScript
- Express
- `http-proxy-middleware`

## Маршрутизация
Входящие префиксы:
- `/identity/*` -> `IDENTITY_SERVICE_URL`
- `/cars/*` -> `CAR_SERVICE_URL`
- `/bookings/*` -> `BOOKING_SERVICE_URL`
- `/clients/*` -> `CLIENT_SERVICE_URL`
- `/partners/*` -> `PARTNER_SERVICE_URL`
- `/tickets/*` -> `TICKET_SERVICE_URL`
- `/files/*` -> `FILE_SERVICE_URL`
- `/internal/*` -> `INTERNAL_SERVICE_URL`

Gateway удаляет префикс перед проксированием.
Пример: `/identity/auth/login` -> `{IDENTITY_SERVICE_URL}/auth/login`.

## Переменные окружения
См. `./.env.example` и `src/index.ts`:
- `IDENTITY_SERVICE_URL`
- `CAR_SERVICE_URL`
- `BOOKING_SERVICE_URL`
- `CLIENT_SERVICE_URL`
- `PARTNER_SERVICE_URL`
- `TICKET_SERVICE_URL`
- `FILE_SERVICE_URL`
- `INTERNAL_SERVICE_URL`
- `PORT`
- `EXTERNAL_PORT`

Важно: в `src/index.ts` все `*_SERVICE_URL` обязательны.
Если переменных нет в вашем `.env`, добавьте вручную.

## Запуск
### Локально
Из `backend/external/reverse-proxy-service/src`:

```bash
npm ci
npm run dev
```

### Через compose сервиса
Из `backend/external/reverse-proxy-service`:

```bash
docker compose up --build
```

### В составе всего проекта
Из корня репозитория:

```bash
docker compose up --build api-gateway
```

Порт gateway в корневом compose: `API_GATEWAY_PORT` (по умолчанию `9186`).

## Необходимые права
Gateway не выполняет собственную авторизацию и не проверяет permissions.

Проверка прав выполняется целевыми backend-сервисами после проксирования запроса.
