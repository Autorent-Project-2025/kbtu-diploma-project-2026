# Ticket Service

## Назначение
Сервис тикетов регистрации/верификации, жалоб и менеджерских review-потоков. Поддерживает типы:
- `Client` - регистрация клиента;
- `Partner` - регистрация партнера;
- `PartnerCar` - добавление машины партнером через согласование;
- booking completion review и partner cancellation review через ticket workflow;
- complaints и booking access requests.

Основные задачи:
- создание тикета;
- просмотр pending-очереди менеджером;
- approve/reject с фиксированием причины/менеджера/времени;
- интеграции с другими сервисами при approve/reject;
- выдача временных ссылок на документы тикета;
- очередь жалоб, action logs, reopen requests и доступ к booking review;
- создание/миграция chat conversations для complaint context.

### ERM Диаграмма
![ERM](./docs/images/erm.png)


## Стек
- ASP.NET Core (`net10.0`)
- PostgreSQL
- Flyway (миграции через корневой `docker-compose.yml`)
- JWT авторизация
- RabbitMQ outbox для workflow events

## API
Нативный base path сервиса: `/`.
Через gateway сервис доступен по префиксу `/tickets`.

Маршруты:
- `POST /` (`AllowAnonymous`) - создание тикета (`multipart/form-data`)
- `GET /all` (policy `tickets:view-all`)
- `GET /pending` (policy `tickets:view`)
- `GET /{id:guid}` (policy `tickets:view`)
- `GET /{id:guid}/documents/{documentType}/temporary-link` (policy `tickets:view`)
  - `documentType`: `identity` | `license` | `ownership`
- `POST /{id:guid}/approve` (policy `tickets:approve`)
- `POST /{id:guid}/reject` (policy `tickets:reject`)
- `POST /{id:guid}/issue-fine` (booking completion fine workflow)
- `GET /healthz`
- `GET /metrics`

### Complaints (`/complaints`)
- `POST /complaints` - создать жалобу (`multipart/form-data`)
- `GET /complaints/my`
- `GET /complaints/my/{id:guid}`
- `POST /complaints/my/{id:guid}/respond`
- `GET /complaints/my/by-booking/{bookingId:int}`
- `POST /complaints/my/{id:guid}/reopen-request`
- `GET /complaints/my/{id:guid}/reopen-requests`
- `GET /complaints/all` (manager queue)
- `GET /complaints/all/{id:guid}`
- `POST /complaints/all/{id:guid}/take`
- `POST /complaints/all/{id:guid}/request-info`
- `POST /complaints/all/{id:guid}/note`
- `POST /complaints/all/{id:guid}/resolve`
- `POST /complaints/all/{id:guid}/reject`
- `POST /complaints/all/{id:guid}/actions/cancel-booking`
- `POST /complaints/all/{id:guid}/actions/waive-charge`
- `POST /complaints/all/{id:guid}/actions/escalate`
- `POST /complaints/all/{id:guid}/actions/refund-charge`
- `GET /complaints/all/{id:guid}/action-logs`

### Booking access requests
- `POST /complaints/{complaintId:guid}/booking-access-requests`
- `GET /complaints/{complaintId:guid}/booking-access-requests/mine`
- `GET /complaints/{complaintId:guid}/booking-review`
- `GET /complaints/access-requests`
- `GET /complaints/access-requests/{id:guid}`
- `POST /complaints/access-requests/{id:guid}/approve`
- `POST /complaints/access-requests/{id:guid}/reject`
- `POST /complaints/access-requests/{id:guid}/revoke`

## Контракты
### Создание тикета (`POST /`)
Тип контента: `multipart/form-data`.

Общие поля:
- `ticketType` (`Client` | `Partner` | `PartnerCar`, optional, default `Client`)
- `email` (обязателен)

Для `Client`:
- `firstName`, `lastName`, `phoneNumber`, `birthDate` (обязательны)
- `identityDocumentFile` (PDF, обязателен)
- `driverLicenseFile` (PDF, обязателен)
- `avatarUrl` (optional)

Для `Partner`:
- `firstName`, `lastName`, `phoneNumber` (обязательны)
- `identityDocumentFile` (PDF, обязателен)
- `companyName`, `contactEmail` (optional)

Для `PartnerCar`:
- `carBrand`, `carModel`, `licensePlate` (обязательны)
- `ownershipDocumentFile` (PDF, обязателен)
- `carImageFiles[]` (минимум 1 изображение)
- `transmission`, `fuelType`, `seats`, `doors`, `bodyType`, `horsepower` (optional structured fields)
- `selectedTags[]` (optional preset semantic tags)

Важно:
- endpoint помечен как `AllowAnonymous`, но для `PartnerCar` требуется `Authorization` header:
  - сервис извлекает текущего партнера из `partner-service /me`;
  - email для уведомлений берется из authenticated user claims;
  - имя/фамилия/телефон владельца подтягиваются автоматически.

### Approve (`POST /{id}/approve`)
Body необязателен.

Для `PartnerCar` менеджер может передать правки перед approve:

```json
{
  "partnerCarData": {
    "carBrand": "Toyota",
    "carModel": "Camry",
    "licensePlate": "123ABC02",
    "fuelType": "petrol",
    "bodyType": "sedan",
    "horsepower": 181,
    "confirmedTags": ["business", "comfort"]
  }
}
```

### Reject (`POST /{id}/reject`)

```json
{
  "decisionReason": "Некорректные данные",
  "partnerCarData": {
    "carBrand": "Toyota",
    "carModel": "Camry",
    "licensePlate": "123ABC02",
    "confirmedTags": ["business", "comfort"]
  }
}
```

`partnerCarData` optional и используется для фиксации отредактированных менеджером значений.

## Интеграции
При обработке тикетов сервис вызывает:

- `identity-service`
  - `POST /internal/users/provision` (`X-Internal-Api-Key`)
- `client-service`
  - `POST /internal/clients/provision` (`X-Internal-Api-Key`)
- `partner-service`
  - `POST /internal/partners/provision` (`X-Internal-Api-Key`)
  - `GET /me` (с `Authorization`) для `PartnerCar` create
- `file-service`
  - `POST /api/internal/files/upload` (`X-Internal-Api-Key`)
  - `POST /api/internal/files/temporary-link` (`X-Internal-Api-Key`)
- `image-service`
  - `POST /api/images` (с `Authorization`) для загрузки фото `PartnerCar`
- `car-service`
  - provisioning `PartnerCar` выполняется через RabbitMQ-событие `ticket.partner-car-provision-requested`
- `booking-service`
  - booking completion review, partner cancellation review, cancel booking action
- `payment-service`
  - refund/waive/fine flows через booking/payment workflows
- `chat-service`
  - conversations для complaints и системные сообщения
- `email-service`
  - стандартные ticket email-уведомления отправляются через RabbitMQ-события;
  - часть custom booking/complaint уведомлений может отправляться прямым HTTP-клиентом.

## Переменные окружения
См. `./.env.example`:
- `Jwt__PublicKey`
- `Cors__AllowedOrigins__0`
- `IdentityService__BaseUrl`
- `IdentityService__InternalApiKey`
- `EmailService__BaseUrl`
- `ChatService__BaseUrl`
- `ChatService__InternalApiKey`
- `BookingService__BaseUrl`
- `BookingService__InternalApiKey`
- `ClientService__BaseUrl`
- `ClientService__InternalApiKey`
- `PartnerService__BaseUrl`
- `PartnerService__InternalApiKey`
- `FileService__BaseUrl`
- `FileService__InternalApiKey`
- `ImageService__BaseUrl`
- `CarService__BaseUrl`
- `CarService__InternalApiKey`
- `RabbitMq__HostName`
- `RabbitMq__Port`
- `RabbitMq__UserName`
- `RabbitMq__Password`
- `RabbitMq__ExchangeName`
- `Activation__SetPasswordBaseUrl`
- `EXTERNAL_PORT`
- `POSTGRES_USER`
- `POSTGRES_PASSWORD`
- `POSTGRES_DB`
- `POSTGRES_PORT`

## Наблюдаемость
- сервис принимает и возвращает `X-Request-Id`;
- принимает и продолжает `traceparent`;
- пишет JSON-логи с `requestId`/`traceId` для входящих запросов и исходящих S2S вызовов;
- публикует `Prometheus`-совместимые метрики на `GET /metrics`;
- экспортирует входящие HTTP spans и исходящие `HttpClient` spans в `OpenTelemetry Collector` и дальше в `Tempo`, если задан `OTEL_EXPORTER_OTLP_ENDPOINT`.

## Запуск
В папке сервиса отдельного `docker-compose` нет. Рекомендуемый запуск - из корня репозитория:

```bash
docker compose up --build ticket-db ticket-flyway ticket-service
```

Сервис доступен на порту `TICKET_SERVICE_PORT` (по умолчанию `1248`).

## Необходимые права
Права проверяются по claim `permissions` в JWT.

- `Ticket.View` - `GET /pending`, `GET /{id}`, `GET /{id}/documents/...`
- `Ticket.ViewAll` - `GET /all`, super-manager views
- `Ticket.Approve` - `POST /{id}/approve`
- `Ticket.Reject` - `POST /{id}/reject`
- `Complaint.View` - manager complaint queue
- `Complaint.Review` - complaint actions and booking review
- `AccessRequest.Review` - approve/reject/revoke booking access requests

Публичный маршрут без JWT:
- `POST /` (для `Client` и `Partner`)

Для `PartnerCar` create требуется валидный `Authorization` header текущего партнера.
