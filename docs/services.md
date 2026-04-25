# AutoRent Services Inventory

Документ описывает все сервисы платформы AutoRent: 3 frontend-приложения, 15 backend-сервисов (включая edge `api-gateway`) и runtime-инфраструктуру. Источники: исходный код сервисов, корневой `docker-compose.yml`, `ops/observability/*`, `backend/libraries/messaging-dotnet`.

**Глобальные факты, важные для всех backend-сервисов:**
- В корневом compose все backend-сервисы слушают **порт 8080 внутри Docker-сети** (`backend`/`data`). Порты вида `1244`, `1298`, `1821`, `1832` и т.п. — это **внешние host-порты** автономного `docker-compose.yaml` каждого сервиса; в продовом root compose они недоступны снаружи.
- Сети: `frontend`, `backend`, `data` (последняя — `internal: true`). Базы доступны только в `data`. Из всех backend-сервисов наружу опубликован **только** `api-gateway` (`9186` HTTP / `9443` HTTPS).
- Communication: synchronous HTTP (через DI-зарегистрированные `HttpClient`-классы) + RabbitMQ exchange `autorent.events` (topic).
- Internal API keys: у каждого получателя свой ключ (`local-<service>-key`); при S2S вызове отправитель шлёт `X-Internal-Api-Key` со значением **получателя**.
- JWT валидация: каждый user-facing сервис настроен на `Jwt__PublicKey` (RSA), который выпускает `identity-service` и публикует через `/.well-known/jwks.json`.

---

## Frontend Apps

### Service name: External Frontend (`frontend/external`)
Purpose: Публичный клиентский UI — каталог машин, AI-подбор, бронирование/оплата/завершение поездки, completion review с 5 фото, жалобы, чат, регистрация через тикет, partner cabinet (управление машинами и просмотр бронирований/payouts).
Service type: edge (frontend SPA)
Public access: public — порт `5173` (host), внутри docker-сети `frontend`.
Database: no
Main entities: Cars, PartnerCars, Bookings, Payments, CompletionReview (5 photos), Complaints, Conversations, Tickets, PartnerProfile
Main endpoints (UI routes):
- `/`, `/login`, `/apply`, `/activate`, `/cars`, `/cars/:id`, `/cars/partner-cars/:id`, `/ai`
- `/bookings`, `/bookings/:id`, `/bookings/:id/payment`, `/bookings/:id/complete`
- `/complaints`, `/complaints/:id`
- `/profile`, `/profile/user`, `/profile/partner`, `/partner/cars`, `/partner/cars/:id`, `/partner/bookings`
Authentication/authorization: JWT (claim `actor_type` определяет client vs partner UI). Permission `Booking.Create` обязателен для создания брони. Большинство `/bookings`/`/complaints`/`/partner` требуют валидного JWT.
Communicates with: только `api-gateway` (`VITE_API_URL=http://localhost:9186`). Никаких прямых вызовов в backend-сервисы.
Publishes events: no
Consumes events: no
Observability: client-side только.
Notes: Vue 3 + Vite + TypeScript + Tailwind. В compose запускается командой `npm run dev -- --host`.

---

### Service name: Internal Frontend (`frontend/internal`)
Purpose: CRM для менеджеров и super-менеджеров: очереди тикетов и жалоб, approve/reject, документы и фото, справочники Clients/Partners/PartnerCars, бронирования с advisory damage check, finance (charges), access requests/booking review, чат, локальный admin раздел.
Service type: edge (frontend SPA)
Public access: public (UI), порт `5174`. Бизнес-доступ — только пользователям с менеджерскими permissions.
Database: no
Main entities: Tickets, Complaints, AccessRequests, Clients, Partners, PartnerCars, Bookings, Charges, Conversations, Users (admin), Roles
Main endpoints (UI routes): `/login`, `/tickets`, `/clients[/:id]`, `/partners[/:id]`, `/cars[/:id]`, `/bookings[/:id]`, `/complaints[/...]`, `/finance`, `/super`, `/super/managers/:id`, `/admin`
Authentication/authorization: JWT + permission-based route guards: `Ticket.View|ViewAll|Approve|Reject`, `Client.View`, `Partner.View`, `PartnerCar.View`, `Booking.View`, `Complaint.View|Review`, `AccessRequest.Review`, `Payment.View`, `User.View`.
Communicates with: только `api-gateway`.
Publishes events: no
Consumes events: no
Observability: client-side только.
Notes: Vue 3 + TypeScript + Vite.

---

### Service name: Superadmin Frontend (`frontend/superadmin`)
Purpose: Управление пользователями (CRUD, активация/деактивация, role assignment), управление ролями (создание, role inheritance) и permissions.
Service type: edge (frontend SPA)
Public access: public (UI), порт `5175`. Доступ закрыт permissions.
Database: no
Main entities: Users, Roles, Permissions, RoleInheritance
Main endpoints (UI routes): `/login`, `/users`
Authentication/authorization: JWT. `User.View` для входа в `/users`. Дополнительно: `User.Create|Update|Activate|Deactivate|Delete|AssignRole|RemoveRole`, `Role.View|Create|AssignPermission`, `Permission.View`.
Communicates with: только `api-gateway` → `identity-service` (`/identity/users/*`, `/identity/roles/*`, `/identity/permissions`).
Publishes events: no
Consumes events: no
Observability: client-side только.
Notes: Vue 3 + TypeScript + Vite.

---

## Backend Edge

### Service name: API Gateway (`backend/external/reverse-proxy-service`)
Purpose: Единственная внешняя HTTP/HTTPS-точка входа. Прокси по префиксам, route rewrite (снимает префикс), CORS allowlist, IP rate limiting, security headers, request-id/traceparent propagation, edge-метрики/traces/JSON-логи.
Service type: edge
Public access: **public** — порты `9186` (HTTP) и `9443` (HTTPS, self-signed dev TLS), сети `frontend`+`backend`. Единственный публично доступный backend-компонент.
Database: no
Main entities: нет (route-config-only)
Main endpoints:
- `GET /healthz` — liveness
- `GET /metrics` — Prometheus
- Прокси: `/identity/* → identity-service`, `/cars/* → car-service`, `/ai/* → ai-search-service`, `/bookings/* → booking-service`, `/clients/* → client-service`, `/partners/* → partner-service`, `/tickets/* → ticket-service`, `/files/* → file-service`, `/chat/*` (включая WebSocket для SignalR) `→ chat-service`, `/payments/* → payment-service`, `/internal/* → image-service`
Authentication/authorization: gateway не делает доменную авторизацию и не проверяет permissions. Проверки выполняют upstream-сервисы.
Communicates with: все backend-сервисы по `<svc>:8080` через `*_SERVICE_URL`. Нет собственных доменных вызовов.
Publishes events: no
Consumes events: no
Observability:
- `GET /metrics` (Prometheus scrape job `api-gateway`)
- OTel traces в otel-collector (`OTEL_EXPORTER_OTLP_TRACES_ENDPOINT=http://otel-collector:4318/v1/traces`) → Tempo
- JSON-логи в `/tmp/autorent/api-gateway/*.jsonl` (volume `gateway_logs`) → Promtail → Loki
- Генерирует/прокидывает `X-Request-Id` и `traceparent`
Notes: Node.js + Express + http-proxy-middleware. Self-signed TLS в dev (`TLS_ENABLED=true`). `PROXY_TIMEOUT_MS=60000`, `RATE_LIMIT_MAX_REQUESTS=300/60s`. `TRUST_PROXY=loopback, linklocal, uniquelocal`.

---

## Backend — Shared

### Service name: Identity Service (`backend/shared/identity-service`)
Purpose: Аутентификация и авторизация. Выпускает JWT (RSA) и refresh tokens, публикует JWKS, активирует учётки, управляет users/roles/permissions/role inheritance, хранит `subject_type` и `actor_type`. Предоставляет internal provisioning для `ticket-service`. Публикует событие `user.deleted` при удалении пользователя.
Service type: shared
Public access: internal only (через gateway `/identity/*`); сети `backend`+`data`.
Database: yes — `identity-db` (PostgreSQL 16) — миграции через `identity-flyway` из `src/Migrations`.
Main entities: `users`, `subject_types`, `actor_types`, `roles`, `permissions`, `user_roles`, `role_permissions`, `role_inheritance` (transitive permission resolution, кольцевое наследование запрещено), `refresh_tokens`, `activation_tokens`, `user_provision_requests`
Main endpoints:
- `POST /auth/login`, `POST /auth/refresh`, `POST /auth/activate`
- `GET /.well-known/jwks.json`
- `GET/POST/PUT /users`, `PATCH /users/{id}/activate|deactivate`, `DELETE /users/{id}`
- `POST/DELETE /users/{id}/roles/{roleId}`
- `GET/POST /roles`, `POST/DELETE /roles/{id}/permissions`, `POST/DELETE /roles/{id}/parents`
- `GET/POST /permissions`
- `POST /internal/users/provision` (`X-Internal-Api-Key`)
- `GET /internal/users/{userId}` (`X-Internal-Api-Key`) — booking-service использует для lookup'а
- `GET /healthz`, `GET /metrics`
Authentication/authorization: JWT (RSA). Permissions: `User.View|Create|Update|AssignRole|RemoveRole|Activate|Deactivate|Delete`, `Role.View|Create|AssignPermission`, `Permission.View|Create`. Internal — `X-Internal-Api-Key`.
Communicates with (synchronous HTTP): **никого** не вызывает синхронно — только публикует JWT/JWKS, остальные сервисы валидируют их сами.
Publishes events:
- `user.deleted` (`UserDeletedEvent`) — `IdentityService.Infrastructure/Events/RabbitMqEventPublisher.cs`. Публикуется при `DELETE /users/{id}`.
Consumes events: no
Observability:
- `GET /metrics` (Prometheus scrape job `identity-service`)
- OTel traces (`OTEL_EXPORTER_OTLP_ENDPOINT=http://otel-collector:4318`) → Tempo
- JSON-логи в `/tmp/autorent/identity-service/*.jsonl` (volume `identity_logs`) → Promtail → Loki
- `X-Request-Id`/`traceparent` propagation
Notes: ASP.NET Core net10.0. Subject types: `user|service|api_key|system`. Actor types: `client|partner|admin|internal`. Эффективные permissions считаются транзитивно по `role_inheritance`. JWT claims включают `sub`, `username`, `subject_type`, `actor_type`, `permissions`.

---

### Service name: Chat Service (`backend/shared/chat-service`)
Purpose: Conversations и messages для жалоб/booking review/контекстных диалогов. Real-time доставка через SignalR. Вложения через `file-service`. Email-нотификации о новых сообщениях через RabbitMQ.
Service type: shared
Public access: internal only (через gateway `/chat/*`, включая WebSocket-hub `/chat/hubs/conversation`).
Database: yes — `chat-db` (MongoDB 7), database `chat_db`.
Main entities: `conversations` (с вложенными `participants`), `messages` (с вложенными `attachments`)
Main endpoints:
- Public (JWT): `GET /conversations/by-context/{contextType}/{contextId}`, `GET /conversations/{id}`, `GET /conversations/{id}/messages`, `POST /conversations/{id}/messages` (multipart), `GET /conversations/{id}/attachments/{attachmentId}/temporary-link`
- Internal (`X-Internal-Api-Key`): `POST /internal/conversations`, `POST/PATCH /internal/conversations/{id}/participants[/{userId}]`, `POST /internal/conversations/{id}/close|reopen`, `GET /internal/conversations/by-context/{contextType}/{contextId}`, `POST /internal/conversations/{id}/system-message`
- `GET /healthz`
Authentication/authorization: пользовательские endpoints — JWT (без отдельного permission). Internal — `X-Internal-Api-Key=local-chat-service-key`.
Communicates with (synchronous HTTP):
- `file-service` — загрузка вложений (на основе env `FileService__BaseUrl`/`FileService__InternalApiKey`).
Publishes events:
- `chat.email.new-message` (`ChatNewMessageEmailRequestedEvent`) — `ChatService.Infrastructure/Notifications/ChatNotificationPublisher.cs`. Потребитель: `email-service`.
Consumes events: no
Observability: `GET /healthz`. **Метрики/traces/JSON-логи на диск не настроены** — нет mount в Promtail.
Notes: ASP.NET Core. SignalR hub проксируется gateway. Вложения хранятся в `file-service`, в Mongo только metadata.

---

### Service name: Email Service (`backend/shared/email-service`)
Purpose: Отправка email через SMTP. Шаблоны: `approved`, `rejected`, `partner-approved/rejected`, `partner-car-approved/rejected`, `chat new message`, `custom`. Основные нотификации идут через RabbitMQ; HTTP API — для custom-сценариев (используется `booking-service`).
Service type: shared
Public access: internal only (нет gateway-route).
Database: no
Main entities: события + дедуп ключи (`EMAIL_EVENT_DEDUP_TTL_MS`).
Main endpoints:
- `GET /health`, `GET /healthz`
- `POST /emails/approved`, `POST /emails/rejected`
- `POST /emails/partners/approved|rejected`, `POST /emails/partners/cars/approved|rejected`
- `POST /emails/custom`
Authentication/authorization: ни JWT, ни API-key, ни permission claim — open HTTP API внутри backend-сети.
Communicates with: SMTP-провайдер (`SMTP_HOST`/`SMTP_PORT`/...). Никаких backend S2S вызовов.
Publishes events: no
Consumes events:
- queue `email-service.notifications` ← exchange `autorent.events` ← routing keys `ticket.email.client-approved|client-rejected|partner-approved|partner-rejected|partner-car-approved|partner-car-rejected`, `chat.email.new-message`. Реализация: `src/rabbitmq/consumer.ts`.
Observability:
- JSON-логи в `/tmp/autorent/email-service/*.jsonl` (volume `email_logs`) → Promtail → Loki.
- `/metrics` и OTel traces **не выставляются**.
Notes: Node.js + TypeScript + Nodemailer. Запускается через `node --experimental-strip-types`. Дедупликация событий (`EMAIL_EVENT_DEDUP_TTL_MS`).

---

### Service name: Image Service (`backend/shared/image-service`)
Purpose: Загрузка/удаление/выдача изображений (модели машин, partner cars, аватары). Поддерживает локальный диск (`/app/uploads`, volume `image_uploads`) и Google Cloud Storage. Через gateway отдаёт публичные URL по префиксу `/internal/*`.
Service type: shared
Public access: internal only для записи; через gateway `/internal/{fileName}` доступны публичные локальные файлы.
Database: no — хранение только в файловой системе или GCS.
Main entities: image objects (binary + url + storage backend)
Main endpoints:
- `POST /api/images` — upload (raw `application/octet-stream`, форматы `jpeg|png|webp`)
- `DELETE /api/images/{imageId}`
- `GET /public/{fileName}` (только локальный режим)
- `GET /healthz`
Authentication/authorization: JWT permissions `Image.Create` (upload), `Image.Delete` (удаление). `GET /public/...` без проверок.
Communicates with: GCS (опционально через `@google-cloud/storage`). Backend S2S — нет.
Publishes events: no
Consumes events: no
Observability: `GET /healthz`. **Метрики/traces/JSON-логи на диск не настроены.**
Notes: Node.js + Express + Sharp (конвертация в `webp`). Запускается через `node --experimental-strip-types`.

---

## Backend — External (Domain)

### Service name: Car Service (`backend/external/car-service`)
Purpose: Каталог машин и партнерских машин. CRUD `car_models` (с нормализованным `brands`/`models`), `partner_cars`, comments, images; partner cabinet `/my`; deterministic автоподбор `POST /match`; price estimate; events для AI-индекса; internal provisioning после approve тикета `PartnerCar`.
Service type: domain
Public access: internal only (через gateway `/cars/*`).
Database: yes — `car-db` (PostgreSQL 16) + Flyway миграции.
Main entities: `brands`, `models`, `car_models`, `partner_cars` (статусы `Available|Reserved|InTrip|Maintenance`), `car_model_images`, `partner_car_images`, `features`, `car_features`, `car_comments`
Main endpoints:
- Models: `GET /models[/{id}]` (anon), `POST/PUT/DELETE /models[/{id}]` (`CarModel.Create|Update|Delete`)
- Partner cars: `GET /partner-cars[/{id}]` (anon), `POST/PUT/DELETE /partner-cars[/{id}]` (`PartnerCar.Create|Update|Delete`)
- Comments: `GET/POST/PUT/DELETE /comments[...]` (`CarComment.Create|Update|Delete`)
- Images: `POST/PUT/DELETE /images/models/...` (`CarModel.Update|Delete`); `POST/PUT/DELETE /images/partner-cars/...` (`CarImage.Create|Update|Delete`)
- Partner cabinet: `GET /my[/{id}]` (`PartnerCar.ViewOwn`)
- Catalog/match (anon): `GET /available-models`, `GET /price-estimate`, `GET /recommendations`, `POST /match`
- Internal (`X-Internal-Api-Key`): `POST /internal/partner-cars/provision`, `GET /internal/partner-cars/{id}/snapshot`, `GET /internal/partner-cars/{id}/pricing-context`, `POST /internal/partner-cars/by-partner/{partnerUserId}/set-active`
Authentication/authorization: JWT permissions (см. выше). Internal — `X-Internal-Api-Key=local-car-service-key`.
Communicates with (synchronous HTTP):
- `partner-service` — `GET /me` (с JWT passthrough Authorization) — `PartnerContextClient.cs`
- `booking-service` — `GET /internal/bookings/counts`, `GET /internal/bookings/by-partner-car/{id}`, `POST /internal/bookings/check-availability` (`X-Internal-Api-Key`) — `BookingReadClient.cs`
- `client-service` — `GET /internal/clients/by-user/{userId}` (`X-Internal-Api-Key`) — `ClientProfileReadClient.cs`
- `car-market-value-service` — `GET /market-value/estimate?brand=&model=&year=` (no header) — `CarMarketValueClient.cs`
- `image-service` — для upload/delete/update изображений (через `ImageService__BaseUrl`)
Publishes events:
- `car.search.partner-car-upserted` и `car.search.partner-car-deleted` (`CarSearchPartnerCarUpserted`/`Deleted`) — `CarSearchIndexEventPublisher.cs`. Потребитель: `ai-search-service`.
Consumes events:
- `ticket.partner-car.provision-requested` ← `ticket-service` — `CarService.Api/Messaging/PartnerCarProvisionConsumer.cs`. После approve тикета `PartnerCar` создаёт `partner_car` + изображения; если пары `brand+model` нет — создаёт справочники и `car_models` с фото.
Observability:
- JSON-логи в `/tmp/autorent/car-service/*.jsonl` (volume `car_logs`) → Promtail → Loki.
- `/metrics` и OTel traces **не настроены**.
Notes: ASP.NET Core net10.0. Outbox для events не используется — публикация in-process после успешной транзакции.

---

### Service name: AI Search Service (`backend/external/ai-search-service`)
Purpose: AI-подбор машин по свободному тексту. Hybrid retrieval (RRF: 0.6 lexical + 0.4 vector), pgvector-индекс `ai_car_documents`, intent classifier, heuristic+LLM parser, query expansion, business rerank, опциональный LLM rerank, Redis-кэш, click tracking, персонализация через user-embeddings, periodic reindex.
Service type: domain (AI/support)
Public access: internal only (через gateway `/ai/*`).
Database: yes — `ai-search-db` (PostgreSQL 16 + pgvector, host port `1836`). Кэш: `ai-search-redis` (Redis 7, host port `6380`, `maxmemory 128mb`, `allkeys-lru`).
Main entities: `ai_car_documents` (`vector_embedding vector(1024)`, `lexical_document tsvector`, jsonb tags, GIN/B-tree/IVFFLAT индексы), `ai_chat_histories`, `brand_model_aliases`, `ai_recommendation_clicks`, `user_embeddings`
Main endpoints:
- Public: `POST /recommendations`, `POST /click`, `GET /history` (JWT), `PUT /history` (JWT), `GET /healthz`, `GET /metrics`
- Internal: `POST /internal/reindex`, `POST /internal/reindex/partner-cars/:id`, `POST /internal/refresh-user-embeddings`
Authentication/authorization: пользовательские endpoints — JWT валидация по publickey identity. Internal endpoints — без специальной защиты в коде (используются как операционные).
Communicates with (synchronous HTTP):
- `car-service` — **публичные** anonymous endpoints (no header): `GET /available-models`, `GET /models/{id}`, `GET /partner-cars?...`, `GET /partner-cars/{id}` — `catalogClient.ts`
- `partner-service` — `GET /public/by-related-user/{userId}` (anon) — `catalogClient.ts`
- `booking-service` — `GET /available?partnerCarId=&startTime=&endTime=` (anon) — `catalogClient.ts`
- `Ollama` — `http://ollama:11434` для chat (qwen2.5) и embeddings (bge-m3, 1024-dim) — `ollamaClient.ts`
- `Redis` — recommendation cache (TTL 300s)
- опционально OpenAI-compatible API (`OPENAI_BASE_URL`)
Publishes events: no
Consumes events:
- queue `ai-search-service.indexing` ← `car.search.partner-car-upserted` (upsert документа), `car.search.partner-car-deleted` (удаление) — `messaging/indexingConsumer.ts`. Errors → `nack(requeue=false)`.
Observability:
- `GET /metrics` (Prometheus) — но **scrape job не настроен** в `prometheus.yml` (`/metrics` сейчас экспонируется, но не собирается; в README говорится про observability только для api-gateway/ticket/identity).
- JSON-логи в `/tmp/autorent/ai-search-service/*.jsonl` (volume `ai_search_logs`) → Promtail → Loki.
- OTel traces **не настроены**.
Notes: Node.js + TypeScript. Полный reindex по таймеру `AUTO_REFRESH_INTERVAL_SECONDS=900` и при старте (`AUTO_INDEX_ON_STARTUP=true`). Зависит от `ollama-pull` (init-контейнер скачивает модели). GPU-режим через `docker-compose.gpu.yml`. Fallback: local LLM → OpenAI → heuristic; Redis → in-memory LRU.

---

### Service name: Booking Service (`backend/external/booking-service`)
Purpose: Бронирования и платежный flow. Создание/смена статуса (`Pending → Confirmed → Active → Completed | Canceled`), mock payment, price preview, completion review (5 фото + advisory damage check), review/complaint tickets, проверка доступности, outbox-синхронизация с `payment-service` через RabbitMQ. Subscription plans поддерживаются.
Service type: domain
Public access: internal only (через gateway `/bookings/*`).
Database: yes — `booking-db` (PostgreSQL 16) + Flyway. Exclusion constraint `prevent_overlapping_bookings` (статусы блокировки: `pending|confirmed|active`).
Main entities: `bookings` (`booking_range tstzrange`, `pricing_breakdown jsonb`, snapshots `car_brand`/`car_model`/`partner_name`/`cover_image_url`/`image_urls`), `subscription_plans`, `subscriptions`, `payment_sync_outbox_messages`
Main endpoints:
- JWT: `POST /` (`Booking.Create`), `GET /my[/stats]`, `GET /all`, `GET /{id}`, `GET /all/{id}`, `POST /{id}/cancel|confirm|complete|complete-review|partner-cancel`
- Payments: `POST /{id}/payment/start|submit`, `GET /{id}/payment/status`, `GET /{id}/charges`, `POST /{id}/charges/{chargeId}/pay`, `GET /price-preview`
- Anonymous: `GET /available?partnerCarId=&startTime=&endTime=`
- Internal (`X-Internal-Api-Key`): `GET /internal/bookings/by-partner-car/{id}`, `GET /internal/bookings/by-partner-user/{partnerUserId}`, `GET /internal/bookings/counts`, `POST /internal/bookings/check-availability`, `POST /internal/bookings/{id}/cancel`, `POST /internal/bookings/{id}/completion-review/approve|fine-issued`, `POST /internal/bookings/{id}/partner-cancellation/approve|reject`
Authentication/authorization: JWT, `Booking.Create` для `POST /`. Internal — `X-Internal-Api-Key=local-booking-service-key`.
Communicates with (synchronous HTTP):
- `car-service` — `GET /internal/partner-cars/{id}/pricing-context|snapshot` (`X-Internal-Api-Key`) — `PartnerCarReadClient.cs`
- `payment-service` — `POST /internal/mock-payments/start|{id}/submit`, `GET /internal/mock-payments/by-booking/{id}`, `POST /internal/payments/bookings/confirm|cancel|complete`, `POST /internal/payments/booking-charges`, `GET /internal/payments/bookings/{id}/charges`, `GET /internal/payments/users/{userId}/booking-charges`, `POST /internal/payments/booking-charges/{chargeId}/paid` — `PaymentSyncClient.cs`
- `identity-service` — `GET /internal/users/{userId}` (`X-Internal-Api-Key`) — `IdentityUserReadClient.cs`
- `client-service` — `GET /internal/clients/by-user/{userId}[/booking-access]`, `POST /internal/clients/by-user/{userId}/booking-access/block|unblock` (`X-Internal-Api-Key`) — `ClientBookingAccessClient.cs`
- `partner-service` — `GET /internal/partners/public-profile/by-related-user/{userId}` (`X-Internal-Api-Key`) — `PartnerProfileReadClient.cs`
- `ticket-service` — `POST /` multipart **без headers** (анонимный endpoint) — `BookingCompletionTicketClient.cs`, `PartnerBookingCancellationTicketClient.cs`
- `ai-damage-eval-service` — `POST /inspect-session` multipart (`X-Internal-Api-Key`, **timeout 15s**, fail-open) — `DamageEvaluationClient.cs`
- `email-service` — `POST /emails/custom` (no header) — `BookingEmailClient.cs`
Publishes events (через outbox `payment_sync_outbox_messages` → `PaymentSyncOutboxDispatcher`):
- `booking.payment.confirmed` (PaymentConfirmed) → `payment-service`
- `booking.payment.canceled` (PaymentCanceled) → `payment-service`
- `booking.payment.completed` (PaymentCompleted) → `payment-service`
Consumes events:
- `user.deleted` ← `identity-service` — `BookingService.Api/Messaging/UserDeletedConsumer.cs` (cleanup)
Observability:
- JSON-логи в `/tmp/autorent/booking-service/*.jsonl` (volume `booking_logs`) → Promtail → Loki.
- `/metrics` и OTel traces **не настроены**.
Notes: ASP.NET Core net10.0. `DamageEvalService__TimeoutSeconds=15` (< gateway 60s) — обязательное условие fail-open. AI service не указан в `depends_on` — booking-service стартует параллельно.

---

### Service name: Client Service (`backend/external/client-service`)
Purpose: Профиль клиента — first/last name, birth date, identity/license file names, phone, avatar URL, link to identity user. Booking-access (block/unblock) для штрафов и блокировок.
Service type: domain
Public access: internal only (через gateway `/clients/*`).
Database: yes — `client-db` (PostgreSQL 16) + Flyway.
Main entities: `clients` (с booking-access state)
Main endpoints:
- JWT permissions: `GET /` и `GET /{id}` (`Client.View`), `POST /` (`Client.Create`), `PUT /{id}` (`Client.Update`), `DELETE /{id}` (`Client.Delete`)
- JWT: `GET /me`
- Internal (`X-Internal-Api-Key`):
  - `POST /internal/clients/provision`
  - `GET /internal/clients/by-user/{userId}` — booking-service использует
  - `GET /internal/clients/by-user/{userId}/booking-access`
  - `POST /internal/clients/by-user/{userId}/booking-access/block|unblock`
Authentication/authorization: JWT permissions (`Client.*`); Internal — `X-Internal-Api-Key=local-client-service-key`.
Communicates with (synchronous HTTP): `image-service` для аватара (`ImageStorageClient` зарегистрирован, фактическое использование ограниченное).
Publishes events: no
Consumes events: no
Observability: `/healthz` only. **Метрики/traces/JSON-логи на диск не настроены** — нет mount в Promtail.
Notes: ASP.NET Core net10.0. Provisioning вызывается `ticket-service` при approve `Client` тикета.

---

## Backend — Internal (Domain)

### Service name: Partner Service (`backend/internal/partner-service`)
Purpose: Профиль партнера + фасад кабинета партнера: профиль, временные ссылки на документы (через `file-service`), wallet/ledger/payouts (через `payment-service`), partner bookings (через `booking-service`). Каскадная деактивация машин при изменении статуса партнёрства.
Service type: domain (internal)
Public access: internal only (через gateway `/partners/*`); `/public/by-related-user/{relatedUserId}` — анонимный.
Database: yes — `partner-db` (PostgreSQL 16) + Flyway.
Main entities: `partners`
Main endpoints:
- JWT permissions: `GET /` и `GET /{id}` (`Partner.View`), `POST /` (`Partner.Create`), `PUT /{id}` (`Partner.Update`), `DELETE /{id}` (`Partner.Delete`)
- JWT: `GET /me`
- Anonymous: `GET /public/by-related-user/{relatedUserId}` — публичный профиль перевозчика
- Internal (`X-Internal-Api-Key`): `POST /internal/partners/provision`, `GET /internal/partners/public-profile/by-related-user/{userId}` (booking-service использует)
Authentication/authorization: JWT permissions; Internal — `X-Internal-Api-Key=local-partner-service-key`.
Communicates with (synchronous HTTP):
- `file-service` — `POST /api/internal/files/temporary-link` (`X-Internal-Api-Key`) — `FileStorageClient.cs`
- `payment-service` — `GET /internal/payments/wallets/{partnerUserId}`, `GET /internal/payments/ledger/{partnerUserId}`, `POST /internal/payments/payouts/request`, `GET /internal/payments/payouts/{id}`, `GET /internal/payments/payouts/by-partner/{partnerUserId}`, `POST /internal/payments/payouts/{id}/cancel` (`X-Internal-Api-Key`) — `PartnerPaymentClient.cs`
- `booking-service` — `GET /internal/bookings/by-partner-user/{partnerUserId}` (`X-Internal-Api-Key`) — `PartnerBookingClient.cs`
- `car-service` — `POST /internal/partner-cars/by-partner/{partnerUserId}/set-active` (`X-Internal-Api-Key`) — `CarServiceClient.cs` — массовое включение/выключение машин при изменении статуса партнерства.
Publishes events: no
Consumes events: no
Observability: `/healthz` only. **Метрики/traces/JSON-логи на диск не настроены.**
Notes: ASP.NET Core net10.0. Доменная классификация actor — через JWT claim `actor_type=partner`, не через пробный `/me`.

---

### Service name: Ticket Service (`backend/internal/ticket-service`)
Purpose: Тикеты регистрации/верификации (`Client`, `Partner`, `PartnerCar`), очереди жалоб, booking completion review, partner cancellation review, booking access requests. Orchestrator онбординг-потоков: при approve синхронно создаёт user+profile, складывает документы и фото, затем через outbox публикует workflow-события для email-уведомлений и provisioning машин партнера.
Service type: domain (internal)
Public access: internal only (через gateway `/tickets/*`).
Database: yes — `ticket-db` (PostgreSQL 16) + Flyway.
Main entities: `tickets`, `ticket_workflow_outbox_messages`, `complaints`, `complaint_attachments`, `complaint_booking_access_requests`, `complaint_reopen_requests`, `complaint_action_logs`
Main endpoints:
- Tickets: `POST /` (Anonymous, multipart), `GET /all` (`Ticket.ViewAll`), `GET /pending` (`Ticket.View`), `GET /{id}`, `GET /{id}/documents/{identity|license|ownership}/temporary-link`, `POST /{id}/approve` (`Ticket.Approve`), `POST /{id}/reject` (`Ticket.Reject`), `POST /{id}/issue-fine`, `GET /healthz`, `GET /metrics`
- Complaints (user): `POST /complaints`, `GET /complaints/my[/...]`, `POST /complaints/my/{id}/respond|reopen-request`
- Complaints (manager, `Complaint.View`/`Complaint.Review`): `GET/POST /complaints/all/...`, actions: `take|request-info|note|resolve|reject|cancel-booking|waive-charge|escalate|refund-charge`, `GET /complaints/all/{id}/action-logs`
- Booking access requests (`AccessRequest.Review`): `POST/GET /complaints/{id}/booking-access-requests/...`, `POST /complaints/access-requests/{id}/approve|reject|revoke`
Authentication/authorization: JWT permissions (см. выше). `POST /` для `Client`/`Partner` — Anonymous; для `PartnerCar` нужен валидный `Authorization` (партнер — данные подтягиваются из `partner-service /me`).
Communicates with (synchronous HTTP):
- `identity-service` — `POST /internal/users/provision` (`X-Internal-Api-Key`)
- `client-service` — `POST /internal/clients/provision` (`X-Internal-Api-Key`)
- `partner-service` — `POST /internal/partners/provision` (`X-Internal-Api-Key`), `GET /me` (Authorization passthrough)
- `file-service` — `POST /api/internal/files/upload`, `POST /api/internal/files/temporary-link` (`X-Internal-Api-Key`) — `FileStorageClient.cs`
- `image-service` — `POST /api/images` (Authorization passthrough)
- `chat-service` — `POST /internal/conversations`, `POST /internal/conversations/{id}/participants|close|reopen|system-message`, `GET /internal/conversations/by-context/{type}/{id}` (`X-Internal-Api-Key`) — `ChatServiceClient.cs`
- `booking-service` — `GET /internal/bookings/{bookingId}` (`X-Internal-Api-Key`) — `BookingReadClient.cs`. Вызовы для cancel-booking action и completion review approve используют существующие internal endpoints.
- `payment-service` — refund/waive/fine flows через `payment-service` internal API
- `car-service` — provisioning через RabbitMQ (см. ниже), не синхронно
- `email-service` — стандартные нотификации через RabbitMQ; часть custom — прямым HTTP (опционально)
Publishes events (через outbox `ticket_workflow_outbox_messages` → `TicketWorkflowOutboxDispatcher`, 7 типов событий):
- `ticket.email.client-approved` / `ticket.email.client-rejected`
- `ticket.email.partner-approved` / `ticket.email.partner-rejected`
- `ticket.email.partner-car-approved` / `ticket.email.partner-car-rejected`
- `ticket.partner-car.provision-requested` (для `car-service`, после approve `PartnerCar`)
Consumes events: no
Observability:
- `GET /metrics` (Prometheus scrape job `ticket-service`)
- OTel traces (incoming HTTP + outgoing HttpClient spans) → Tempo
- JSON-логи в `/tmp/autorent/ticket-service/*.jsonl` (volume `ticket_logs`) → Promtail → Loki
- `X-Request-Id`/`traceparent` propagation
Notes: ASP.NET Core net10.0. Outbox + RabbitMQ для надёжной доставки workflow-событий. Стартует только после `identity|email|client|partner|file|image|car|chat-service` healthy (depends_on).

---

### Service name: Payment Service (`backend/internal/payment-service`)
Purpose: Внутренний финансовый учёт. Кошелек партнера, ledger, customer payments, payouts, mock-эквайринг, booking charges (damage/fines). Pure internal — не выходит в другие сервисы синхронно. Реагирует на booking events для пересчёта wallet'а.
Service type: domain (internal)
Public access: internal only — большая часть API под `X-Internal-Api-Key`. View-эндпоинты доступны через gateway `/payments/view/*` с permission `Payment.View`.
Database: yes — `payment-db` (PostgreSQL 16) + Flyway.
Main entities: `partner_wallets` (pending/available/reserved), `customer_payments` (с `platform_commission_rate=0.10` и `partner_amount`), `partner_payouts` (idempotency через `request_key`), `partner_ledger_entries`, `mock_payment_attempts` (`session_key` UNIQUE), `booking_charges`, `processed_integration_events` (idempotency для входящих RabbitMQ событий)
Main endpoints:
- Internal (`X-Internal-Api-Key`): `POST /internal/payments/bookings/{confirm|cancel|complete}`, `POST /internal/mock-payments/start`, `GET /internal/mock-payments/by-booking/{id}`, `POST /internal/mock-payments/{id}/submit`, `POST /internal/payments/booking-charges`, `POST /internal/payments/booking-charges/{id}/{paid|cancel|refund}`, `GET /internal/payments/bookings/{id}/charges`, `GET /internal/payments/users/{userId}/booking-charges`, `GET /internal/payments/wallets/{partnerUserId}`, `GET /internal/payments/ledger/{partnerUserId}`, `POST /internal/payments/payouts/request`, `POST /internal/payments/payouts/{id}/{processing|paid|failed|cancel}`, `GET /internal/payments/payouts/{id}`, `GET /internal/payments/payouts/by-partner/{partnerUserId}`
- View (JWT + `Payment.View`): `GET /view/bookings/{bookingId}/charges`
Authentication/authorization: внутренние операции — `X-Internal-Api-Key=local-payment-service-key`. View — JWT + `Payment.View`.
Communicates with (synchronous HTTP): **никого** не вызывает синхронно.
Publishes events: no (но синхронно отвечает на booking RPC)
Consumes events:
- `booking.payment.confirmed` (BookingPaymentConfirmed)
- `booking.payment.canceled` (BookingPaymentCanceled)
- `booking.payment.completed` (BookingPaymentCompleted)
Реализация: `PaymentService.Api/Messaging/BookingPaymentConsumer.cs`. Pending → reverse pending → pending → available перевод. Idempotency через `processed_integration_events.event_id`.
Observability: `/healthz` only. **Метрики/traces/JSON-логи на диск не настроены.**
Notes: ASP.NET Core net10.0. `Payment__PlatformCommissionRate=0.10`, `Payment__Currency=KZT` (из docker-compose).

---

### Service name: File Service (`backend/internal/file-service`)
Purpose: Хранение приватных файлов (документы тикетов, identity, license, contracts партнеров, ownership PartnerCar). Принимает raw upload, отдаёт временные ссылки, удаляет файлы. Локальный диск (`/app/uploads`, volume `file_uploads`) или Google Cloud Storage.
Service type: domain (internal)
Public access: internal only (через gateway `/files/*`).
Database: no — хранилище на диске или в GCS.
Main entities: stored files, signed URL'ы (TTL `SIGNED_URL_TTL_SECONDS`)
Main endpoints:
- Public (JWT): `POST /api/files` (raw body, header `x-file-name`), `POST /api/files/temporary-link`, `DELETE /api/files/{fileName}`
- Internal (`X-Internal-Api-Key`): `POST /api/internal/files/upload`, `POST /api/internal/files/temporary-link`
Authentication/authorization: JWT permissions `File.Create|Read|Delete`. Internal — `X-Internal-Api-Key=local-file-service-key`.
Communicates with: GCS через `@google-cloud/storage` (опционально). Backend S2S — нет.
Publishes events: no
Consumes events: no
Observability: `/healthz` only. **Метрики/traces/JSON-логи на диск не настроены.**
Notes: Node.js + Express + TypeScript. Default `USE_WEB_STORAGE=true` (GCS). `PUBLIC_BASE_URL=http://localhost:9186/files` (через gateway).

---

### Service name: Car Market Value Service (`backend/internal/car-market-value-service`)
Purpose: Оценка рыночной стоимости машины (KZT) по `brand+model+year` через скрейпинг `kolesa.kz`: парсит цены, удаляет выбросы методом IQR, возвращает median/average и confidence (`low|medium|high`). Используется `car-service` в pricing-расчётах.
Service type: shared/support (internal)
Public access: internal only (через `car-service`; нет gateway-route).
Database: no
Main entities: market value estimates per request (in-memory)
Main endpoints:
- `GET /market-value/estimate?brand=&model=&year=`
- `POST /market-value/estimate` (JSON body)
- `GET /healthz`
Authentication/authorization: нет (внутренний сервис, доступен только из docker `backend` сети).
Communicates with: `kolesa.kz` (`KOLESA_BASE_URL`) — `GET /cars/{brand}/{model}/?year[from]=&year[to]=` (внешний scraping).
Publishes events: no
Consumes events: no
Observability: `/healthz`. Стандартные логи.
Notes: Python + FastAPI (uvicorn). `KOLESA_MAX_PAGES=3`, `REQUEST_TIMEOUT_SECONDS=15`. На `backend` сети, без `data` (нет БД).

---

### Service name: AI Car Damage Eval Service (`backend/internal/ai-car-damage-eval-service`)
Purpose: Advisory-only AI-проверка пяти completion-фото после поездки (`front`, `back`, `side_left`, `side_right`, `interior`). Валидация формата/качества/обструкции/цвета/car context, затем YOLO pipeline (`yolov8n.pt` + `yolov8m_damage_v1.pt`) с дедупликацией пересекающихся detections per-slot. Возвращает `OK|DAMAGES_FOUND|INVALID_SESSION`. Финальное решение — за менеджером.
Service type: domain (internal AI)
Public access: internal only (вызывается только `booking-service`).
Database: no
Main entities: inspection sessions (in-memory), detected damages per slot, rejected photos
Main endpoints:
- `POST /inspect-session` (multipart: `car_id`, `car_model`, `car_color`, `photo_*`) — header `X-Internal-Api-Key`
- `GET /health` — liveness
- `GET /ready` — readiness (200 только после warmup YOLO weights, до 2 мин на CPU)
Authentication/authorization: fail-closed по умолчанию через `X-Internal-Api-Key=local-ai-damage-eval-service-key`. Dev-bypass только при `ENVIRONMENT=development` + `ALLOW_UNAUTHENTICATED_INTERNAL_DEV=true`. Без обоих — 503.
Communicates with: **никого** не вызывает (только локальный inference).
Publishes events: no
Consumes events: no
Observability: `/health`, `/ready`. Метрик/traces/file logs в Promtail нет.
Notes: Python + FastAPI. `MIN_PHOTOS=4` из 5. `USE_REGISTRY_VALIDATION=false` в проде. `booking-service` НЕ указывает в `depends_on` — стартует параллельно (warmup до 2 мин). На `backend` сети, без `data`.

---

## Runtime Infrastructure

### Service name: RabbitMQ
Purpose: Брокер сообщений. Topic exchange `autorent.events` для всех событий платформы.
Service type: infrastructure
Public access: internal only (порт `5672` AMQP в backend сети). Management UI на `:15672` (host `15672`).
Database: persistent volume `rabbitmq_data`.
Main entities: exchange `autorent.events` (topic, durable), queues:
- `email-service.notifications` ← `ticket.email.client-approved|client-rejected|partner-approved|partner-rejected|partner-car-approved|partner-car-rejected`, `chat.email.new-message`
- `ai-search-service.indexing` ← `car.search.partner-car-upserted|deleted`
- `payment-service.booking-payments` ← `booking.payment.confirmed|canceled|completed`
- `booking-service.user-deleted` ← `user.deleted`
- `car-service.partner-car-provision` ← `ticket.partner-car.provision-requested`
Authentication/authorization: AMQP credentials (`RabbitMq__UserName/Password`, defaults `autorent/autorent`).
Communicates with: подключаются 6 publisher'ов (`identity`, `chat`, `car`, `booking`, `ticket`, `payment`-listener) и 5 consumer'ов (`email`, `ai-search`, `payment`, `car`, `booking`).
Publishes events: n/a (брокер)
Consumes events: n/a (брокер)
Observability: management UI на `:15672`.
Notes: Outbox-паттерн используется в `ticket-service` (`ticket_workflow_outbox_messages`) и `booking-service` (`payment_sync_outbox_messages`) для надёжной доставки. Topology создаётся при старте сервисов через `backend/libraries/messaging-dotnet`.

---

### Service name: Ollama
Purpose: Локальная LLM/embedding runtime для `ai-search-service`. Модели: `qwen2.5:1.5b` (chat) и `bge-m3` (embeddings, 1024-dim). Опциональный GPU-режим через `docker-compose.gpu.yml`.
Service type: infrastructure / AI provider
Public access: internal only (`:11434` в backend сети).
Database: volume `ollama_data` (хранит модели).
Main entities: модели (pulled via init-контейнер `ollama-pull`).
Main endpoints: `POST /api/pull`, `POST /api/embeddings`, `POST /api/chat` и т.п. (HTTP API Ollama).
Authentication/authorization: нет (внутренняя сеть).
Communicates with: HuggingFace/registry для скачивания моделей при старте.
Publishes events: no
Consumes events: no
Observability: healthcheck `ollama list`.
Notes: `OLLAMA_KEEP_ALIVE=30m`. Init-контейнер `ollama-pull` — one-shot, гарантирует наличие моделей до старта `ai-search-service`.

---

### Observability Stack

#### Prometheus (`ops/observability/prometheus/prometheus.yml`)
Service type: infrastructure (observability)
Public access: host порт `9090`.
Scrape jobs:
- `prometheus` → `prometheus:9090/metrics`
- `api-gateway` → `api-gateway:8080/metrics`
- `ticket-service` → `ticket-service:8080/metrics`
- `identity-service` → `identity-service:8080/metrics`
Notes: только 3 backend-сервиса собирают метрики (gateway, ticket, identity). `ai-search-service` экспонирует `/metrics`, но **не** включён в scrape config.

#### OpenTelemetry Collector (`ops/observability/otel-collector/config.yml`)
Service type: infrastructure (observability)
Public access: internal — gRPC `:4317`, HTTP `:4318`, healthcheck `:13133`.
Pipeline:
- Receivers: OTLP (gRPC + HTTP)
- Processors: batch
- Exporters: `otlp/tempo` → `tempo:4317`
- Pipeline: traces only
Notes: только 3 сервиса экспортируют traces (api-gateway, ticket-service, identity-service — единственные с `OTEL_EXPORTER_OTLP_*` env).

#### Tempo (`ops/observability/tempo/tempo.yml`)
Service type: infrastructure (observability)
Public access: host порт `3200`.
Storage: volume `tempo_data`. Принимает traces от OTel Collector.

#### Loki (`ops/observability/loki/config.yml`)
Service type: infrastructure (observability)
Public access: host порт `3100`. Storage: volume `loki_data`.

#### Promtail (`ops/observability/promtail/promtail.yml`)
Service type: infrastructure (observability)
Public access: internal.
Tailed log files (через volumes):
- `/tmp/autorent/api-gateway/*.jsonl` → `service=api-gateway`
- `/tmp/autorent/ticket-service/*.jsonl` → `service=ticket-service`
- `/tmp/autorent/identity-service/*.jsonl` → `service=identity-service`
- `/tmp/autorent/car-service/*.jsonl` → `service=car-service`
- `/tmp/autorent/booking-service/*.jsonl` → `service=booking-service`
- `/tmp/autorent/email-service/*.jsonl` → `service=email-service`
- `/tmp/autorent/ai-search-service/*.jsonl` → `service=ai-search-service`
Notes: 7 сервисов из 15 пишут JSON-логи на диск и собираются Promtail. Остальные (chat, client, partner, payment, file, image, car-market-value, ai-damage-eval) — нет.

#### Grafana (`ops/observability/grafana/`)
Service type: infrastructure (observability)
Public access: host порт `3000`. Datasources (provisioned):
- Prometheus (uid `prometheus`, default)
- Loki (uid `loki`, с derived field `TraceId` для перехода в Tempo)
- Tempo (uid `tempo`)
Dashboards: `autorent-observability.json` (home dashboard).

---

### Per-Service Observability Matrix

| Service | `/metrics` (scraped) | OTel traces | JSON file logs | Promtail tail |
|---|---|---|---|---|
| api-gateway | ✅ | ✅ | ✅ | ✅ |
| identity-service | ✅ | ✅ | ✅ | ✅ |
| ticket-service | ✅ | ✅ | ✅ | ✅ |
| car-service | — | — | ✅ | ✅ |
| booking-service | — | — | ✅ | ✅ |
| email-service | — | — | ✅ | ✅ |
| ai-search-service | endpoint есть, но не scrape'ится | — | ✅ | ✅ |
| chat-service | — | — | — | — |
| client-service | — | — | — | — |
| partner-service | — | — | — | — |
| payment-service | — | — | — | — |
| file-service | — | — | — | — |
| image-service | — | — | — | — |
| car-market-value-service | — | — | — | — |
| ai-car-damage-eval-service | — | — | — | — |

---

### Databases (database-per-service)
Все PostgreSQL-сервисы — `postgres:16` с пользователем `postgres/postgres` и DB `postgres_db`. Миграции — Flyway (отдельный one-shot контейнер `*-flyway` per service).
- `identity-db`, `client-db`, `partner-db`, `ticket-db`, `booking-db`, `payment-db`, `car-db` — PostgreSQL 16 в `data` сети (`internal: true`).
- `ai-search-db` — `pgvector/pgvector:pg16`, host port `1836`.
- `chat-db` — MongoDB 7 в `data` сети.
- `ai-search-redis` — Redis 7-alpine, host port `6380`.
- `file_uploads`/`image_uploads` — Docker volumes (или GCS, опционально).

### Backend Library
- `backend/libraries/messaging-dotnet` (`AutoRent.Messaging`) — общие RabbitMQ contracts, exchange/queue topology, publisher abstraction для всех .NET сервисов. Centralizes routing keys и event payload contracts.
