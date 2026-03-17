# Reverse Proxy Service (API Gateway)

## Назначение
Edge-сервис, который является единственной внешней точкой входа для backend, проксирует запросы фронтендов к backend-сервисам и делает route rewrite.

## Стек
- Node.js
- TypeScript
- Express
- `http-proxy-middleware`

## Что умеет
- route rewrite для backend-сервисов;
- `GET /healthz` для compose/liveness;
- `GET /metrics` в формате Prometheus;
- rate limiting по IP;
- базовые security headers;
- CORS по allowlist;
- генерацию и проброс `X-Request-Id`/`traceparent`;
- JSON-логи с `requestId`/`traceId` для корреляции в `Loki`;
- экспорт edge spans в `OpenTelemetry Collector`/`Tempo`;
- HTTP (`PORT`) и HTTPS (`HTTPS_PORT`) listeners;
- self-signed TLS-сертификат в dev, если `TLS_ENABLED=true` и не переданы готовые `TLS_CERT_PATH`/`TLS_KEY_PATH`.

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
- `ALLOWED_ORIGINS`
- `RATE_LIMIT_WINDOW_MS`
- `RATE_LIMIT_MAX_REQUESTS`
- `PROXY_TIMEOUT_MS`
- `REQUEST_TIMEOUT_MS`
- `PORT`
- `HTTPS_PORT`
- `TLS_ENABLED`
- `HTTP_TO_HTTPS_REDIRECT`
- `TLS_CERT_PATH`
- `TLS_KEY_PATH`
- `TLS_CERT_CN`
- `TLS_CERT_DAYS`
- `TRUST_PROXY`
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

Порты gateway в корневом compose:
- `API_GATEWAY_PORT` (по умолчанию `9186`, HTTP)
- `API_GATEWAY_TLS_PORT` (по умолчанию `9443`, HTTPS)

## Необходимые права
Gateway не выполняет доменную авторизацию и не проверяет `permissions`.

Проверка прав выполняется целевыми backend-сервисами после проксирования запроса.
