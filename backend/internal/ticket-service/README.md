# Ticket Service

## Назначение
Сервис заявок на регистрацию/верификацию клиента. Отвечает за:
- создание заявки пользователем;
- просмотр pending-заявок менеджером;
- approve/reject заявки;
- интеграцию с `identity-service` (provision пользователя);
- интеграцию с `email-service` (approved/rejected уведомления).

## Стек
- ASP.NET Core (`net10.0`)
- PostgreSQL
- Flyway (миграции через корневой `docker-compose.yml`)
- JWT авторизация

## API
Нативный base path сервиса: `/`.
Через gateway сервис доступен по префиксу `/tickets`.

Маршруты:
- `POST /` (`AllowAnonymous`) - создание заявки
- `GET /pending` (policy `tickets:view`)
- `GET /{id:guid}` (policy `tickets:view`)
- `POST /{id:guid}/approve` (policy `tickets:approve`)
- `POST /{id:guid}/reject` (policy `tickets:reject`)
- `GET /healthz`

Пример создания заявки:

```json
{
  "fullName": "Arlan User",
  "email": "arlan@example.com",
  "birthDate": "2000-01-15"
}
```

Пример reject:

```json
{
  "decisionReason": "Документы невалидны"
}
```

## Переменные окружения
См. `./.env.example`:
- `Jwt__PublicKey`
- `Cors__AllowedOrigins__0`
- `IdentityService__BaseUrl`
- `IdentityService__InternalApiKey`
- `EmailService__BaseUrl`
- `Activation__SetPasswordBaseUrl`
- `EXTERNAL_PORT`
- `POSTGRES_USER`
- `POSTGRES_PASSWORD`
- `POSTGRES_DB`
- `POSTGRES_PORT`

## Интеграции
- `POST {IdentityService__BaseUrl}/internal/users/provision` + header `X-Internal-Api-Key`.
- `POST {EmailService__BaseUrl}/emails/approved`.
- `POST {EmailService__BaseUrl}/emails/rejected`.

## Запуск
В папке сервиса отдельного `docker-compose` нет. Рекомендуемый запуск - из корня репозитория:

```bash
docker compose up --build ticket-db ticket-flyway ticket-service
```

Сервис будет доступен на порту `TICKET_SERVICE_PORT` (по умолчанию `1248`).
