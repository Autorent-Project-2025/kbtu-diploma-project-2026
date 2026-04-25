# AutoRent Services Inventory

Документ собирает обзорную информацию по всем сервисам платформы AutoRent: 3 frontend-приложения и 15 backend-сервисов (включая edge `api-gateway`). Источник данных — README каждого сервиса, корневой `README.md` и `docs/project-architecture.md`.

---

## Frontend Apps

### Service name: External Frontend (`frontend/external`)
Purpose: Публичный клиентский UI AutoRent — каталог машин, AI-подбор, бронирование/оплата/завершение поездки, жалобы и чаты, регистрация через тикет, кабинет партнера с управлением машинами и просмотром бронирований.
Service type: edge (frontend SPA)
Public access: public — открыт пользователям, доступен на `http://localhost:5173`
Database: no
Main entities: Cars, PartnerCars, Bookings, Payments, CompletionReview (5 photos), Complaints, Conversations, Tickets, Partner profile
Main endpoints (UI routes):
- `/`, `/login`, `/apply`, `/activate`
- `/cars`, `/cars/:id`, `/cars/partner-cars/:id`, `/ai`
- `/bookings`, `/bookings/:id`, `/bookings/:id/payment`, `/bookings/:id/complete`
- `/complaints`, `/complaints/:id`
- `/profile`, `/profile/user`, `/profile/partner`
- `/partner/cars`, `/partner/cars/:id`, `/partner/bookings`
Authentication/authorization: JWT через `identity-service`. UI читает claim `actor_type` из JWT (`partner` уводит в partner-секции, иначе — клиентский кабинет). Permission `Booking.Create` нужен для создания брони. Большинство страниц `/bookings/*`, `/complaints/*`, `/partner/*` требуют валидного JWT.
Communicates with: API Gateway (единственная точка входа). Через gateway вызывает `identity`, `cars`, `ai`, `bookings`, `tickets`, `chat` и др.
Publishes events: no
Consumes events: no
Observability: standard SPA-логи в браузере (на бэкенд не прокидываются)
Notes: Vue 3 + Vite + TypeScript + Tailwind. ENV: `VITE_API_URL`, `VITE_APP_NAME`, `VITE_DEFAULT_CAR_IMAGE`, `VITE_DEFAULT_BOOKING_HOURS`, `VITE_TOKEN_EXPIRY_HOURS`. Порт 5173.

---

### Service name: Internal Frontend (`frontend/internal`)
Purpose: CRM-интерфейс для менеджеров и супер-менеджеров: очереди тикетов и жалоб, approve/reject заявок, документы и фото, справочники клиентов/партнеров/машин, бронирования с AI-advisory damage check, финансы (charges), access requests/booking review, чат с вложениями, локальный admin-раздел.
Service type: edge (frontend SPA)
Public access: public (UI), но бизнес-доступ — только пользователям с менеджерскими permissions
Database: no
Main entities: Tickets, Complaints, AccessRequests, Clients, Partners, PartnerCars, Bookings, Charges, Conversations, Users (admin), Roles
Main endpoints (UI routes):
- `/login`, `/tickets`
- `/clients`, `/clients/:id`, `/partners`, `/partners/:id`
- `/cars`, `/cars/:id`, `/bookings`, `/bookings/:id`
- `/complaints`, `/complaints/access-requests`, `/complaints/:id`, `/complaints/:complaintId/booking-review`
- `/finance`, `/super`, `/super/managers/:id`, `/admin`
Authentication/authorization: JWT через `identity-service`. Маршруты гардятся permission-проверкой: `Ticket.View`, `Ticket.Approve`, `Ticket.Reject`, `Ticket.ViewAll`, `Client.View`, `Partner.View`, `PartnerCar.View`, `Booking.View`, `Complaint.View`, `Complaint.Review`, `AccessRequest.Review`, `Payment.View`, `User.View`.
Communicates with: API Gateway → `identity`, `tickets`, `clients`, `partners`, `cars`, `bookings`, `payments` (view), `chat`.
Publishes events: no
Consumes events: no
Observability: client-side только
Notes: Vue 3 + TypeScript + Vite. Порт 5174.

---

### Service name: Superadmin Frontend (`frontend/superadmin`)
Purpose: Интерфейс супер-администратора AutoRent — управление пользователями (CRUD, активация/деактивация, role assignment), управление ролями (создание, role inheritance) и permissions.
Service type: edge (frontend SPA)
Public access: public (UI), доступ закрыт permissions
Database: no
Main entities: Users, Roles, Permissions, RoleInheritance
Main endpoints (UI routes): `/login`, `/users`
Authentication/authorization: JWT. Вход в `/users` требует `User.View`. Дополнительно: `User.Create`, `User.Update`, `User.Activate`, `User.Deactivate`, `User.Delete`, `User.AssignRole`, `User.RemoveRole`, `Role.View`, `Role.Create`, `Role.AssignPermission`, `Permission.View`.
Communicates with: API Gateway → `identity-service` (`/identity/users/*`, `/identity/roles/*`, `/identity/permissions`).
Publishes events: no
Consumes events: no
Observability: client-side
Notes: Vue 3 + TypeScript + Vite. Порт 5175.

---

## Backend Edge

### Service name: API Gateway (`backend/external/reverse-proxy-service`)
Purpose: Единственная внешняя HTTP/HTTPS-точка входа в backend. Маршрутизирует запросы фронтендов в backend-сервисы, делает route rewrite, добавляет security headers, rate limiting, CORS, request-id/traceparent propagation, edge-метрики и edge-traces.
Service type: edge
Public access: public — единственный публично доступный backend-компонент. HTTP `:9186`, HTTPS `:9443`.
Database: no
Main entities: нет доменных сущностей; маршруты как конфигурация.
Main endpoints:
- `GET /healthz` — liveness
- `GET /metrics` — Prometheus
- Прокси: `/identity/*`, `/cars/*`, `/ai/*`, `/bookings/*`, `/clients/*`, `/partners/*`, `/tickets/*`, `/files/*`, `/chat/*` (включая WebSocket для SignalR), `/payments/*`, `/internal/*` → `image-service`.
Authentication/authorization: gateway не выполняет доменную авторизацию и не проверяет `permissions`. Проверка прав делается целевыми backend-сервисами.
Communicates with: все backend-сервисы по `*_SERVICE_URL`. На запись/чтение HTTP не имеет прямых интеграций кроме прокси.
Publishes events: no
Consumes events: no
Observability:
- `GET /metrics` (Prometheus) — есть;
- distributed traces в OpenTelemetry Collector → Tempo;
- JSON-логи с `requestId`/`traceId` через Promtail → Loki;
- генерирует/прокидывает `X-Request-Id` и `traceparent`.
Notes: Node.js + Express + http-proxy-middleware. Self-signed TLS в dev (`TLS_ENABLED=true`). Префикс снимается перед проксированием (`/identity/auth/login` → `{IDENTITY_SERVICE_URL}/auth/login`).

---

## Backend — Shared

### Service name: Identity Service (`backend/shared/identity-service`)
Purpose: Аутентификация и авторизация платформы. Выпускает JWT и refresh-token, публикует JWKS, активирует учётки, управляет users/roles/permissions/role inheritance, хранит `subject_type` и `actor_type`, предоставляет внутренний provisioning пользователя для `ticket-service`.
Service type: shared
Public access: internal only (доступен внешне через gateway по `/identity/*`)
Database: yes — `identity-db` (PostgreSQL)
Main entities: `users`, `subject_types`, `actor_types`, `roles`, `permissions`, `user_roles`, `role_permissions`, `role_inheritance`, `refresh_tokens`, `activation_tokens`, `user_provision_requests`
Main endpoints:
- `POST /auth/login`, `POST /auth/refresh`, `POST /auth/activate`
- `GET /.well-known/jwks.json`
- `GET/POST/PUT /users`, `PATCH /users/{id}/activate|deactivate`, `DELETE /users/{id}`
- `POST/DELETE /users/{id}/roles/{roleId}`
- `GET/POST /roles`, `POST/DELETE /roles/{id}/permissions`, `POST/DELETE /roles/{id}/parents`
- `GET/POST /permissions`
- `POST /internal/users/provision` (`X-Internal-Api-Key`)
- `GET /healthz`, `GET /metrics`
Authentication/authorization: JWT (RSA), claim `permissions`. Permissions: `User.View|Create|Update|AssignRole|RemoveRole|Activate|Deactivate|Delete`, `Role.View|Create|AssignPermission`, `Permission.View|Create`. Internal provisioning защищён `X-Internal-Api-Key`.
Communicates with: `identity-db` (Postgres). Никого синхронно не вызывает; остальные сервисы валидируют его JWT/JWKS.
Publishes events: no
Consumes events: no
Observability: `GET /metrics`, JSON-логи с requestId/traceId, OpenTelemetry traces, propagation `X-Request-Id`/`traceparent`.
Notes: ASP.NET Core net10.0, Flyway. Эффективные permissions считаются транзитивно по `role_inheritance` (без циклов). Subject type: `user|service|api_key|system`; actor type: `client|partner|admin|internal`. Порт по умолчанию `1244`.

---

### Service name: Chat Service (`backend/shared/chat-service`)
Purpose: Conversations и messages для жалоб, booking review и контекстных диалогов. Real-time доставка сообщений через SignalR, вложения через `file-service`, email-нотификации через RabbitMQ.
Service type: shared
Public access: internal only (через gateway `/chat/*`, включая WebSocket-hub `/chat/hubs/conversation`)
Database: yes — `chat-db` (MongoDB)
Main entities: `conversations`, `conversation_participants` (вложены в conversation), `messages`, `message_attachments`
Main endpoints:
- Public (JWT): `GET /conversations/by-context/{contextType}/{contextId}`, `GET /conversations/{id}`, `GET /conversations/{id}/messages`, `POST /conversations/{id}/messages`, `GET /conversations/{id}/attachments/{attachmentId}/temporary-link`
- Internal (`X-Internal-Api-Key`): `POST /internal/conversations`, `POST /internal/conversations/{id}/participants`, `PATCH /internal/conversations/{id}/participants/{userId}`, `POST /internal/conversations/{id}/close|reopen`, `GET /internal/conversations/by-context/{contextType}/{contextId}`, `POST /internal/conversations/{id}/system-message`
- `GET /healthz`
Authentication/authorization: пользовательские endpoints — JWT (без отдельных permissions, только валидность). Internal — `X-Internal-Api-Key`.
Communicates with:
- `file-service` — загрузка вложений с `X-Internal-Api-Key` и temporary links;
- `identity-service` — валидация JWT по публичному RSA-ключу;
- `RabbitMQ` — публикация событий о новых сообщениях.
Publishes events: `chat.email.new-message` (RabbitMQ exchange `autorent.events`).
Consumes events: no
Observability: `GET /healthz`. Логирование стандартное.
Notes: ASP.NET Core. SignalR hub проксируется gateway. Вложения не хранятся как binary в Mongo — только metadata.

---

### Service name: Email Service (`backend/shared/email-service`)
Purpose: Отправка email через SMTP. Шаблоны: `approved`, `rejected`, `partner approved/rejected`, `partner car approved/rejected`, `chat new message`, `custom`. Основные нотификации идут через RabbitMQ; HTTP API оставлен для custom-сценариев.
Service type: shared
Public access: internal only (нет публичного gateway-route)
Database: no
Main entities: нет реляционной БД; eвenty получает из RabbitMQ
Main endpoints:
- `GET /health`, `GET /healthz`
- `POST /emails/approved`, `POST /emails/rejected`
- `POST /emails/partners/approved|rejected`
- `POST /emails/partners/cars/approved|rejected`
- `POST /emails/custom`
Authentication/authorization: текущая реализация не требует JWT/API-key/permission claim.
Communicates with: SMTP-провайдер.
Publishes events: no
Consumes events: RabbitMQ exchange `autorent.events` (по умолчанию), queue `email-service.notifications`, routing keys: `ticket.email.client-approved|client-rejected|partner-approved|partner-rejected|partner-car-approved|partner-car-rejected`, `chat.email.new-message`.
Observability: JSON-логи через Promtail → Loki. Метрик/трейсов на текущем уровне нет.
Notes: Node.js + TypeScript + Nodemailer. Дедупликация событий по `EMAIL_EVENT_DEDUP_TTL_MS`. Порт по умолчанию `9182`.

---

### Service name: Image Service (`backend/shared/image-service`)
Purpose: Загрузка/удаление/выдача изображений (модели машин, partner cars, аватары и т.д.). Поддерживает локальный диск и Google Cloud Storage. Через gateway отдает публичные URL по префиксу `/internal/*`.
Service type: shared
Public access: internal only для записи; локальный режим может отдавать публичные файлы через `/public/{fileName}`.
Database: no (logical store: `image_objects`, локальные файлы или GCS bucket)
Main entities: image objects (binary), public URL, storage backend
Main endpoints:
- `POST /api/images` — upload (raw body, `Content-Type: application/octet-stream`); поддерживает `jpeg|png|webp`
- `DELETE /api/images/{imageId}`
- `GET /public/{fileName}` (только локальное хранение)
Authentication/authorization: JWT permissions. `Image.Create` для upload, `Image.Delete` для удаления. `GET /public/...` без проверок.
Communicates with: GCS (опционально через `@google-cloud/storage`).
Publishes events: no
Consumes events: no
Observability: стандартные логи.
Notes: Node.js + Express + Sharp. Конвертирует исходники в `webp`. Порт по умолчанию `9181`.

---

## Backend — External (Domain)

### Service name: Car Service (`backend/external/car-service`)
Purpose: Каталог моделей машин и партнерских машин. CRUD `car_models` (с нормализованным справочником `brands`/`models`), `partner_cars`, комментарии, изображения; partner cabinet `/my`; deterministic автоподбор `POST /match`; price estimate; events для AI-индекса; internal provisioning после approve тикета `PartnerCar`.
Service type: domain
Public access: internal only (через gateway `/cars/*`)
Database: yes — `car-db` (PostgreSQL)
Main entities: `brands`, `models`, `car_models`, `partner_cars`, `car_model_images`, `partner_car_images`, `features`, `car_features`, `car_comments`
Main endpoints:
- Models: `GET/POST/PUT/DELETE /models`, `GET /models/{id}`
- Partner cars: `GET/POST/PUT/DELETE /partner-cars`, `GET /partner-cars/{id}` (статусы `Available|Reserved|InTrip|Maintenance`)
- Comments: `GET/POST/PUT/DELETE /comments`, `GET /comments/partner-cars/{partnerCarId}`
- Images: `POST/PUT/DELETE /images/models/...`, `POST/PUT/DELETE /images/partner-cars/...`, `GET /images/...`
- Partner cabinet: `GET /my`, `GET /my/{id}`
- Catalog: `GET /available-models`, `GET /price-estimate`, `GET /recommendations`
- Matching: `POST /match` — deterministic подбор по `modelId+startTime+endTime`
- Internal (`X-Internal-Api-Key`): `POST /internal/partner-cars/provision`, `GET /internal/partner-cars/{id}/snapshot`, `GET /internal/partner-cars/{id}/pricing-context`, `POST /internal/partner-cars/by-partner/{partnerUserId}/set-active`
Authentication/authorization: JWT permissions: `CarModel.Create|Update|Delete`, `PartnerCar.Create|Update|Delete|ViewOwn`, `CarComment.Create|Update|Delete`, `CarImage.Create|Update|Delete`. Анонимны: `GET /models`, `GET /partner-cars`, `GET /available-models`, `POST /match`. Internal — `X-Internal-Api-Key`.
Communicates with:
- `partner-service` — context для `/my`;
- `booking-service` — `POST /internal/bookings/check-availability`, агрегаты по `/my`;
- `image-service` — upload/delete images;
- `car-market-value-service` — оценка рыночной стоимости;
- `RabbitMQ` — публикация событий.
Publishes events: `car.search.partner-car-upserted`, `car.search.partner-car-deleted` (для `ai-search-service`).
Consumes events: `ticket.partner-car-provision-requested` (после approve `PartnerCar` тикета — создаёт partner_car).
Observability: JSON-логи через Promtail → Loki. Метрики/трейсы пока не выставлены отдельно.
Notes: ASP.NET Core net10.0 + Flyway. Порт `1298`. Если пары `brand+model` нет — provision создаёт справочные записи и копирует фото в `car_model_images`.

---

### Service name: AI Search Service (`backend/external/ai-search-service`)
Purpose: AI-подбор машин по свободному тексту. Hybrid retrieval (RRF: 0.6 lexical + 0.4 vector), pgvector-индекс `ai_car_documents`, intent classifier, heuristic+LLM parser, query expansion, business rerank, опциональный LLM rerank, Redis-кэш, click tracking, персонализация через user-embeddings.
Service type: domain (AI/support)
Public access: internal only (через gateway `/ai/*`)
Database: yes — `ai-search-db` (PostgreSQL + pgvector, host port `1836`); + `ai-search-redis` (Redis, host port `6380`)
Main entities: `ai_car_documents` (vector_embedding(1024), tsvector, jsonb tags), `ai_chat_histories`, `brand_model_aliases`, `ai_recommendation_clicks`, `user_embeddings`
Main endpoints:
- Public: `POST /recommendations`, `POST /click`, `GET /history` (JWT), `PUT /history` (JWT), `GET /healthz`, `GET /metrics`
- Internal: `POST /internal/reindex`, `POST /internal/reindex/partner-cars/:id`, `POST /internal/refresh-user-embeddings`
Authentication/authorization: пользовательские endpoints используют JWT (валидируется по publickey identity). Permission claim не требуется отдельным policy. Internal-эндпоинты — без специальной защиты по README (используются как операционные).
Communicates with:
- `car-service` — каталог + partner cars для индексирования;
- `partner-service` — partner metadata;
- `booking-service` — availability filter;
- `Ollama` — LLM (qwen2.5) и embeddings (bge-m3, 1024-dim);
- `Redis` — recommendation cache (TTL 300s, `allkeys-lru`);
- опционально OpenAI-compatible API.
Publishes events: no
Consumes events: RabbitMQ queue `ai-search-service.indexing`, routing keys `car.search.partner-car-upserted` (upsert документа) и `car.search.partner-car-deleted` (удаление).
Observability: `GET /metrics`, structured logs (`observabilityLogger`); JSON-логи в Loki.
Notes: Node.js + TypeScript. Полный reindex по таймеру `AUTO_REFRESH_INTERVAL_SECONDS`/при старте. Метрики качества: recall@5=0.822, precision@5=0.770, MRR=0.933 (golden set 30 запросов). Поддерживает GPU для Ollama через `docker-compose.gpu.yml`. Fallback стратегия: local LLM → OpenAI → heuristic; Redis → in-memory LRU.

---

### Service name: Booking Service (`backend/external/booking-service`)
Purpose: Бронирования партнерских машин и платежный flow. Создание/просмотр/смена статуса (Pending → Confirmed → Active → Completed | Canceled), mock payment (`start/submit`), price preview, completion review с 5 фото и advisory damage check, review/complaint tickets, проверка доступности, outbox-синхронизация с `payment-service` через RabbitMQ.
Service type: domain
Public access: internal only (через gateway `/bookings/*`)
Database: yes — `booking-db` (PostgreSQL)
Main entities: `bookings` (с `booking_range tstzrange`, exclusion constraint `prevent_overlapping_bookings`, `pricing_breakdown jsonb`), `subscription_plans`, `subscriptions`, `payment_sync_outbox_messages`
Main endpoints:
- Public/JWT: `POST /` (`Booking.Create`), `GET /my`, `GET /my/stats`, `GET /all`, `GET /{id}`, `GET /all/{id}`, `POST /{id}/cancel|confirm|complete|complete-review|partner-cancel`
- Payments: `POST /{id}/payment/start`, `GET /{id}/payment/status`, `POST /{id}/payment/submit`, `GET /{id}/charges`, `POST /{id}/charges/{chargeId}/pay`, `GET /price-preview`
- Anonymous: `GET /available?partnerCarId=&startTime=&endTime=`
- Internal (`X-Internal-Api-Key`): `GET /internal/bookings/by-partner-car/{id}`, `GET /internal/bookings/counts`, `POST /internal/bookings/check-availability`, completion-review approve/fine-issued, partner-cancellation approve/reject
Authentication/authorization: JWT. `POST /` требует permission `Booking.Create` (policy `bookings:create`). Большинство `/my|/{id}` — валидный JWT. Internal — `X-Internal-Api-Key`.
Communicates with:
- `car-service` — partner-car snapshot, pricing context, доступность;
- `client-service`, `partner-service` — профильный context;
- `identity-service` — внутренние lookups;
- `payment-service` — mock-эквайринг, charges, ledger, fines, payouts;
- `ticket-service` — review tickets, complaints, manager decisions;
- `ai-damage-eval-service` — `POST /inspect-session` (timeout 15s, fail-open);
- `email-service` — отдельные booking-уведомления;
- `RabbitMQ` — payment sync outbox.
Publishes events: payment-sync events (booking confirmed/canceled/completed) → `payment-service`.
Consumes events: no (RabbitMQ только публикует).
Observability: JSON-логи через Promtail → Loki.
Notes: ASP.NET Core net10.0 + Flyway. Порт `1821`. Exclusion constraint блокирует пересечение броней по статусам `pending|confirmed|active`. Поддерживает subscription plans.

---

### Service name: Client Service (`backend/external/client-service`)
Purpose: Профиль клиента — first/last name, birth date, identity/license file names, phone, avatar, link to identity user.
Service type: domain
Public access: internal only (через gateway `/clients/*`)
Database: yes — `client-db` (PostgreSQL)
Main entities: `clients`
Main endpoints:
- `GET /` (`Client.View`), `GET /{id}` (`Client.View`)
- `POST /` (`Client.Create`), `PUT /{id}` (`Client.Update`), `DELETE /{id}` (`Client.Delete`)
- `GET /me` (валидный JWT)
- `POST /internal/clients/provision` (`X-Internal-Api-Key`)
Authentication/authorization: JWT permissions `Client.View|Create|Update|Delete`. `/me` — только валидный JWT. Internal endpoint — `X-Internal-Api-Key`.
Communicates with: `identity-service` для validate JWT (через JWKS); `file-service` (по именам файлов хранит ссылки, сам не вызывает); `image-service` (для avatar URL).
Publishes events: no
Consumes events: no
Observability: стандартные логи.
Notes: ASP.NET Core net10.0 + Flyway. Порт `1831`. Provisioning вызывается `ticket-service` при approve `Client` тикета.

---

## Backend — Internal

### Service name: Partner Service (`backend/internal/partner-service`)
Purpose: Профиль партнера и фасад кабинета партнера. Хранит owner_first_name/last_name, contract/owner identity file names, dates, phone, link to identity user. Также агрегирует данные кабинета (профиль, временные ссылки на документы, wallet/ledger/payouts, partner bookings).
Service type: domain (internal)
Public access: internal only (через gateway `/partners/*`); `/public/by-related-user/{relatedUserId}` доступен анонимно.
Database: yes — `partner-db` (PostgreSQL)
Main entities: `partners`
Main endpoints:
- `GET /` (`Partner.View`), `GET /{id}` (`Partner.View`)
- `POST /` (`Partner.Create`), `PUT /{id}` (`Partner.Update`), `DELETE /{id}` (`Partner.Delete`)
- `GET /me` (валидный JWT)
- `GET /public/by-related-user/{relatedUserId}` (`AllowAnonymous`, базовый профиль перевозчика)
- `POST /internal/partners/provision` (`X-Internal-Api-Key`)
Authentication/authorization: JWT permissions `Partner.View|Create|Update|Delete`. `/me` — JWT. Internal — `X-Internal-Api-Key`.
Communicates with:
- `file-service` — temporary links на документы;
- `payment-service` — wallet/ledger/payouts партнера;
- `booking-service` — partner bookings.
Publishes events: no
Consumes events: no
Observability: стандартные логи.
Notes: ASP.NET Core net10.0 + Flyway. Порт `1832`. Доменная классификация actor — через JWT claim `actor_type=partner`, не через пробный `/me`.

---

### Service name: Ticket Service (`backend/internal/ticket-service`)
Purpose: Тикеты регистрации/верификации (`Client`, `Partner`, `PartnerCar`), очереди жалоб, booking completion review, partner cancellation review, booking access requests. Orchestrator онбординг-потоков: при approve синхронно создаёт user/profile, складывает документы и фото, затем через outbox публикует workflow-события в RabbitMQ для email-уведомлений и provisioning машин партнера.
Service type: domain (internal)
Public access: internal only (через gateway `/tickets/*`)
Database: yes — `ticket-db` (PostgreSQL)
Main entities: `tickets`, `ticket_workflow_outbox_messages`, `complaints`, `complaint_attachments`, `complaint_booking_access_requests`, `complaint_reopen_requests`, `complaint_action_logs`
Main endpoints:
- Tickets: `POST /` (`AllowAnonymous`, multipart), `GET /all` (`Ticket.ViewAll`), `GET /pending` (`Ticket.View`), `GET /{id}`, `GET /{id}/documents/{identity|license|ownership}/temporary-link`, `POST /{id}/approve` (`Ticket.Approve`), `POST /{id}/reject` (`Ticket.Reject`), `POST /{id}/issue-fine`, `GET /healthz`, `GET /metrics`
- Complaints (user): `POST /complaints`, `GET /complaints/my[/...]`, `POST /complaints/my/{id}/respond|reopen-request`
- Complaints (manager): `GET/POST /complaints/all/...` (`Complaint.View`, `Complaint.Review`); actions: `take`, `request-info`, `note`, `resolve`, `reject`, `cancel-booking`, `waive-charge`, `escalate`, `refund-charge`, `action-logs`
- Booking access requests: `POST/GET /complaints/{id}/booking-access-requests/...`, `POST /complaints/access-requests/{id}/approve|reject|revoke` (`AccessRequest.Review`)
Authentication/authorization: JWT permissions: `Ticket.View|ViewAll|Approve|Reject`, `Complaint.View|Review`, `AccessRequest.Review`. `POST /` — публично для `Client`/`Partner`; для `PartnerCar` нужен валидный `Authorization` (партнер).
Communicates with:
- `identity-service` — `POST /internal/users/provision`;
- `client-service` — `POST /internal/clients/provision`;
- `partner-service` — `POST /internal/partners/provision`, `GET /me`;
- `file-service` — `POST /api/internal/files/upload|temporary-link`;
- `image-service` — `POST /api/images` (с Authorization);
- `booking-service` — completion review, partner cancellation review, cancel booking;
- `payment-service` — refund/waive/fine flows;
- `chat-service` — conversations и system messages для complaint context;
- `email-service` — стандартные нотификации идут через RabbitMQ; часть custom-уведомлений — прямым HTTP.
Publishes events: ticket workflow events в exchange `autorent.events`: `ticket.email.client-approved|client-rejected|partner-approved|partner-rejected|partner-car-approved|partner-car-rejected`, `ticket.partner-car-provision-requested`.
Consumes events: no
Observability: `GET /metrics` (Prometheus), JSON-логи в Loki, OpenTelemetry traces (incoming + outgoing HttpClient spans), `X-Request-Id`/`traceparent` propagation.
Notes: ASP.NET Core net10.0 + Flyway. Порт `1248`. Outbox + RabbitMQ для надёжной публикации событий.

---

### Service name: Payment Service (`backend/internal/payment-service`)
Purpose: Внутренний финансовый учёт партнера: кошелек, ledger, customer payments, payouts, mock-эквайринг, booking charges (damages/fines), интеграция с outbox `booking-service` через RabbitMQ.
Service type: domain (internal)
Public access: internal only — большая часть API под `X-Internal-Api-Key`. Read-only view-эндпоинты доступны через gateway `/payments/view/*` с permission `Payment.View`.
Database: yes — `payment-db` (PostgreSQL)
Main entities: `partner_wallets`, `customer_payments`, `partner_payouts`, `partner_ledger_entries`, `mock_payment_attempts`, `booking_charges`, `processed_integration_events`
Main endpoints:
- Internal (`X-Internal-Api-Key`): `POST /internal/payments/bookings/confirm|cancel|complete`, `POST /internal/mock-payments/start`, `GET /internal/mock-payments/by-booking/{id}`, `POST /internal/mock-payments/{id}/submit`, `POST /internal/payments/booking-charges`, `POST /internal/payments/booking-charges/{id}/paid|cancel|refund`, `GET /internal/payments/bookings/{id}/charges`, `GET /internal/payments/users/{userId}/booking-charges`, `GET /internal/payments/wallets/{partnerUserId}`, `GET /internal/payments/ledger/{partnerUserId}`, `POST /internal/payments/payouts/request`, `POST /internal/payments/payouts/{id}/processing|paid|failed|cancel`, `GET /internal/payments/payouts/...`
- View (JWT + `Payment.View`): `GET /view/bookings/{bookingId}/charges`
Authentication/authorization: внутренние операции — `X-Internal-Api-Key`. Read-only view — JWT + `Payment.View`.
Communicates with: вызывается `booking-service`, `partner-service`, `ticket-service`. Сам не делает HTTP-вызовов в другие сервисы (по README).
Publishes events: no (синхронно отвечает); `processed_integration_events` — для дедупликации входящих RabbitMQ событий.
Consumes events: RabbitMQ — booking payment events (`booking-service` outbox: confirmed/canceled/completed → пересчёт wallet pending/available).
Observability: стандартные логи.
Notes: Бизнес-логика wallet: `Confirmed → pending`, `Canceled → reverse pending`, `Completed → pending → available`. Damage/fine charges — отдельный `booking_charges`. Idempotency через `request_key` (payouts), `event_key` (outbox), `event_id` (processed events).

---

### Service name: File Service (`backend/internal/file-service`)
Purpose: Хранение приватных файлов (документы тикетов, идентификация, лицензии, контракты партнеров, ownership-документы PartnerCar и т.п.). Принимает raw upload, отдаёт временные ссылки, удаляет файлы. Поддерживает локальный диск и Google Cloud Storage.
Service type: domain (internal)
Public access: internal only (через gateway `/files/*`)
Database: no (логическая модель `stored_files`, `temporary_links`; хранение — `file_uploads` или GCS)
Main entities: stored files, temporary signed URLs
Main endpoints:
- Public (JWT): `POST /api/files` (raw body, `x-file-name`, returns `fileName`), `POST /api/files/temporary-link`, `DELETE /api/files/{fileName}`
- Internal (`X-Internal-Api-Key`): `POST /api/internal/files/upload`, `POST /api/internal/files/temporary-link`
Authentication/authorization: JWT permissions `File.Create|Read|Delete`. Internal — `X-Internal-Api-Key`.
Communicates with: GCS (через `@google-cloud/storage`).
Publishes events: no
Consumes events: no
Observability: стандартные логи.
Notes: Node.js + Express + TypeScript. Default `USE_WEB_STORAGE=true` (GCS). `SIGNED_URL_TTL_SECONDS` управляет TTL временных ссылок.

---

### Service name: Car Market Value Service (`backend/internal/car-market-value-service`)
Purpose: Оценка рыночной стоимости машины (KZT) по `brand+model+year` через скрейпинг `kolesa.kz`: парсит цены, удаляет выбросы методом IQR, возвращает median/average и confidence (`low|medium|high`). Используется `car-service` в pricing-расчётах.
Service type: shared/support (internal)
Public access: internal only
Database: no
Main entities: market value estimates (in-memory / per-request)
Main endpoints:
- `GET /market-value/estimate?brand=&model=&year=`
- `POST /market-value/estimate` (JSON body)
- `GET /healthz`
Authentication/authorization: нет специальной защиты по README (внутренний сервис).
Communicates with: `kolesa.kz` (внешний провайдер).
Publishes events: no
Consumes events: no
Observability: `GET /healthz`. Стандартные логи.
Notes: Python + FastAPI (uvicorn). Конфиг: `KOLESA_BASE_URL`, `KOLESA_MAX_PAGES`, `REQUEST_TIMEOUT_SECONDS`, `REQUEST_USER_AGENT`. Порт по умолчанию `8080`.

---

### Service name: AI Car Damage Eval Service (`backend/internal/ai-car-damage-eval-service`)
Purpose: Advisory-only AI-проверка пяти completion-фото после поездки (`front`, `back`, `side_left`, `side_right`, `interior`). Валидация формата/качества/обструкции/цвета/car context, затем YOLO pipeline (`yolov8n.pt` + `yolov8m_damage_v1.pt`) с дедупликацией пересекающихся detections per-slot. Возвращает `OK|DAMAGES_FOUND|INVALID_SESSION`. Финальное решение всегда за менеджером.
Service type: domain (internal AI)
Public access: internal only (вызывается `booking-service`)
Database: no
Main entities: inspection sessions (in-memory per request), detected damages per slot, rejected photos
Main endpoints:
- `POST /inspect-session` (multipart: `car_id`, `car_model`, `car_color`, photo_*) — header `X-Internal-Api-Key`
- `GET /health` — liveness
- `GET /ready` — readiness (200 только после warmup YOLO weights)
Authentication/authorization: fail-closed по умолчанию через `X-Internal-Api-Key`. Dev-bypass только при `ENVIRONMENT=development` + `ALLOW_UNAUTHENTICATED_INTERNAL_DEV=true`. Без обоих — 503.
Communicates with: вызывается только `booking-service` (timeout 15s, fail-open).
Publishes events: no
Consumes events: no
Observability: `GET /health`, `GET /ready` (warmup latch).
Notes: Python + FastAPI. Порт `8000`. Color validation — family-based tolerance (нейтральные white/silver/gray/black, тёплые red/yellow, холодные blue/green). `MIN_PHOTOS=4` из 5. Booking-service не указывает в `depends_on` (warmup до 2 минут на CPU).

---

## Runtime Infrastructure (для контекста)

Не "сервисы" в строгом смысле, но критичны для работы платформы и упоминаются как взаимодействующие компоненты.

### Service name: RabbitMQ
Purpose: Брокер сообщений. Exchange `autorent.events` для всех событий платформы (ticket workflows, partner-car provisioning, payment sync, AI search index refresh, email notifications).
Service type: infrastructure
Public access: internal only (порты `5672` AMQP / `15672` management UI)
Database: no
Main entities: queues (`email-service.notifications`, `ai-search-service.indexing`, etc.), exchange `autorent.events`, routing keys `ticket.email.*`, `ticket.partner-car-provision-requested`, `chat.email.new-message`, `car.search.partner-car-upserted|deleted`, payment sync events.
Authentication/authorization: AMQP credentials (`RabbitMq__UserName/Password`).
Notes: Outbox-паттерн используется в `ticket-service` и `booking-service` для надёжной доставки.

### Service name: Ollama
Purpose: Локальная LLM/embedding runtime для `ai-search-service`. Модели: `qwen2.5` (chat) и `bge-m3` (embeddings, 1024-dim). Опциональный GPU-режим через `docker-compose.gpu.yml`.
Service type: infrastructure / AI provider
Public access: internal only (`:11434`)

### Observability Stack
- **Prometheus** (`:9090`) — метрики (выставляются `api-gateway`, `ticket-service`, `identity-service`, `ai-search-service`).
- **Grafana** (`:3000`) — dashboards и log↔trace correlation.
- **Loki** (`:3100`) — централизованные JSON-логи (Promtail собирает с `api-gateway`, `ticket-service`, `identity-service`, `car-service`, `booking-service`, `email-service`, `ai-search-service`).
- **Tempo** (`:3200`) — distributed traces (через OpenTelemetry Collector).
- **OpenTelemetry Collector** (`:4318`) — приём метрик/traces, маршрутизация в Tempo.
- **Promtail** — доставка логов в Loki.

### Databases
- `identity-db`, `car-db`, `client-db`, `partner-db`, `ticket-db`, `booking-db`, `payment-db` — PostgreSQL (database-per-service).
- `ai-search-db` — PostgreSQL + pgvector (host `:1836`).
- `chat-db` — MongoDB.
- `ai-search-redis` — Redis (host `:6380`).
- `file_uploads`/`image_uploads` — локальный том или Google Cloud Storage.
