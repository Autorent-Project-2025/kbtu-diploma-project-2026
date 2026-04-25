# SYSTEM DESIGN AND ARCHITECTURE

Дата анализа: 25 апреля 2026.

Документ описывает фактическое состояние проекта по исходному коду, `docker-compose.yml`, миграциям, frontend-приложениям, gateway, сервисам, permissions, событиям и observability-конфигурации. Формулировки ниже можно использовать в дипломной работе как source of truth, но важно сохранять разделение между тем, что уже реализовано, и тем, что является планируемым развитием.

## 1. Общая архитектура

Проект реализован как микросервисная платформа AutoRent для аренды автомобилей с несколькими пользовательскими поверхностями: клиентский/партнёрский внешний frontend, внутренний frontend для операционных ролей и отдельный superadmin frontend.

На уровне runtime система состоит из:

- `api-gateway` как единой публичной точки входа для HTTP/HTTPS и WebSocket-трафика;
- набора backend-сервисов с отдельными зонами ответственности;
- отдельных баз данных для доменных сервисов;
- RabbitMQ для асинхронных интеграционных событий;
- frontend-приложений на Vue;
- observability-стека: Prometheus, Grafana, Loki, Tempo, OpenTelemetry Collector и Promtail.

В проекте уже есть готовая architecture diagram: [`docs/project-architecture.md`](project-architecture.md). Диаграмма актуально показывает три frontend-приложения, gateway, backend-сервисы, базы данных, RabbitMQ, AI-инфраструктуру и observability. Ее можно адаптировать для диплома.

## 2. Реальные роли и frontend-приложения

### 2.1. Важное уточнение по ролям

В проекте есть два разных понятия:

- RBAC role: роль в identity-service, которая даёт permissions.
- actor type: тип пользователя в доменной модели, например `client` или `partner`.

Поэтому `customer/client` и `partner` сейчас не являются отдельными RBAC-ролями. Они реализованы через `actor_type` в JWT и доменный профиль. Оба обычно используют RBAC-роль `user`, но отличаются `actor_type=client` или `actor_type=partner`.

### 2.2. Реализованные роли

| Роль / тип | Статус | Где используется | Комментарий |
|---|---:|---|---|
| `user` | Реализовано | External frontend | Базовая RBAC-роль для обычных пользователей. Используется и для клиента, и для партнёра. |
| `client` / `customer` | Реализовано как `actor_type`, не как RBAC-role | External frontend | Клиентский профиль. Может смотреть каталог и создавать бронирования. |
| `partner` | Реализовано как `actor_type`, не как RBAC-role | External frontend, partner cabinet | Партнёрский профиль. Может добавлять машины, смотреть свои бронирования и финансы. |
| `manager` | Реализовано | Internal frontend | Операционная роль для обработки tickets, complaints и части клиентских действий. |
| `supermanager` | Реализовано | Internal frontend | Расширенная операционная роль. В коде имя без дефиса: `supermanager`, а не `super-manager`. |
| `admin` | Реализовано | Internal frontend / admin sections by permissions | Административная роль есть в RBAC, но отдельного `admin frontend` нет. |
| `data-manager` | Реализовано | Internal frontend | Роль для работы с данными клиентов, бронирований, партнёрских машин. Отдельного frontend-приложения нет. |
| `superadmin` | Реализовано | Superadmin frontend | Полный доступ ко всем permissions и управлению users/roles/permissions. |
| `super-manager` | Не реализовано как отдельное имя | - | В проекте фактическое имя роли: `supermanager`. |

### 2.3. Frontend-приложения по ролям

| Frontend | Путь | Public port по умолчанию | Основная аудитория |
|---|---|---:|---|
| External frontend | `frontend/external` | `5173` | Customer/client и partner. |
| Internal frontend | `frontend/internal` | `5174` | Manager, supermanager, admin, data-manager. |
| Superadmin frontend | `frontend/superadmin` | `5175` | Superadmin и пользователи с permission `User.View`. |

External frontend обслуживает как клиентские сценарии, так и partner cabinet. Partner routes дополнительно проверяют `actor_type=partner`.

Internal frontend использует permission-based routing:

- `/tickets` требует `Ticket.View`;
- `/clients` требует `Client.View`;
- `/partners` требует `Partner.View`;
- `/cars` требует `PartnerCar.View`;
- `/bookings` требует `Booking.View`;
- `/complaints` требует `Complaint.View`;
- `/complaints/access-requests` требует `AccessRequest.Review`;
- `/finance` использует доступ через `Partner.View`;
- `/super` требует `Ticket.ViewAll`;
- `/admin` требует `User.View`.

Superadmin frontend сфокусирован на управлении пользователями, ролями и permissions.

### 2.4. Основные действия по ролям

| Роль / actor type | Основные действия |
|---|---|
| Customer / client | Просмотр каталога, просмотр деталей машины, AI-рекомендации, создание booking, mock payment, просмотр своих бронирований, отмена/подтверждение/старт/завершение бронирования, completion review с фото, создание complaint, чат, управление профилем. |
| Partner | Действия обычного пользователя плюс partner profile, добавление и редактирование своих машин, загрузка изображений, отправка машины на moderation через ticket workflow, просмотр partner bookings, wallet/ledger/payouts, запрос отмены partner booking, участие в complaints/chat. |
| Manager | Просмотр tickets, approve/reject tickets, просмотр временных ссылок на документы, обработка complaints, review/resolve complaint, request booking access, блокировка клиентов. |
| Supermanager | Возможности manager плюс расширенный обзор tickets, доступ к `Ticket.ViewAll`, review access requests, управление payment-related данными, деактивация партнёров, data-manager сценарии. |
| Admin | Административные сценарии по users/roles/permissions и доменным сущностям в зависимости от выданных permissions. Отдельного frontend-приложения нет. |
| Data-manager | Просмотр/обновление клиентских данных, партнёрских машин, bookings, complaint read access. |
| Superadmin | Управление users, roles, permissions, role inheritance, активация/деактивация/удаление пользователей, полный доступ ко всем permissions. |

## 3. Technology stack

| Слой | Технологии |
|---|---|
| Backend .NET services | .NET 10, ASP.NET Core, EF Core, Npgsql/PostgreSQL, JWT Bearer authentication. |
| API Gateway | Node.js, TypeScript, Express, `http-proxy-middleware`, OpenTelemetry. |
| Node.js services | TypeScript, Express, RabbitMQ clients, PostgreSQL/Redis clients where needed. |
| Python services | FastAPI, Uvicorn, requests/BeautifulSoup for market value, Ultralytics YOLO/OpenCV for damage evaluation. |
| Frontend | Vue 3, Vite, TypeScript, Vue Router, Axios, Tailwind CSS, SignalR where chat/realtime is needed. |
| Databases | PostgreSQL 16, MongoDB 7, Redis, pgvector for AI search. |
| Message broker | RabbitMQ 3.13 with management UI. |
| Deployment/local runtime | Docker Compose. |
| Observability | Prometheus, Grafana, Loki, Tempo, OpenTelemetry Collector, Promtail. |
| Migrations | Flyway containers for SQL migrations in PostgreSQL-backed services. |

## 4. Полный актуальный список backend-сервисов

В `docker-compose.yml` сейчас описано 15 application/backend services, включая gateway.

| Service | Тип | Краткое описание | Хранилище | Core/supporting |
|---|---|---|---|---|
| `api-gateway` | Node.js/TypeScript | Единая публичная точка входа, reverse proxy, CORS, rate limit, security headers, request id, metrics, tracing. | Нет своей БД | Edge/platform |
| `identity-service` | .NET | Аутентификация, JWT, users, roles, permissions, activation, refresh tokens, JWKS. | `identity-db` PostgreSQL | Core/platform |
| `chat-service` | .NET | Conversations, messages, SignalR chat, email notification events. | `chat-db` MongoDB | Supporting/business |
| `email-service` | Node.js/TypeScript | Отправка email по событиям tickets и chat. | Нет своей БД | Supporting |
| `image-service` | Node.js/TypeScript | Upload/delete/serve изображений, image processing через Sharp, local/GCS storage. | Local volume или GCS | Supporting |
| `file-service` | Node.js/TypeScript | Upload/read/delete документов и временные ссылки на файлы, local/GCS storage. | Local volume или GCS | Supporting |
| `car-service` | .NET | Brands, models, partner cars, car images, comments, car provisioning after approval. | `car-db` PostgreSQL | Core |
| `ai-search-service` | Node.js/TypeScript | AI search, embeddings, recommendations, semantic search, indexing partner cars. | `ai-search-db` PostgreSQL/pgvector, `ai-search-redis` | Supporting/product |
| `booking-service` | .NET | Booking lifecycle, booking status transitions, customer/partner booking flows, booking-payment outbox. | `booking-db` PostgreSQL | Core |
| `client-service` | .NET | Client profiles, identity document metadata, booking access/blocking state. | `client-db` PostgreSQL | Core |
| `partner-service` | .NET | Partner profiles, documents, wallet/ledger/payout read workflows, activation/deactivation. | `partner-db` PostgreSQL | Core |
| `ticket-service` | .NET | Moderation workflow for clients, partners, partner cars, complaints, access requests, completion/cancellation review. | `ticket-db` PostgreSQL | Core |
| `payment-service` | .NET | Mock customer payments, partner wallets, ledger entries, payouts, booking charges. | `payment-db` PostgreSQL | Core |
| `car-market-value-service` | Python/FastAPI | Market value estimation using external car market data scraping. | Нет своей БД | Supporting/AI-pricing |
| `ai-damage-eval-service` | Python/FastAPI | AI damage evaluation from images using computer vision model. | Нет своей БД | Supporting/AI |

## 5. Infrastructure services

| Service | Назначение |
|---|---|
| `rabbitmq` | Message broker for integration events. |
| `identity-db`, `car-db`, `booking-db`, `client-db`, `partner-db`, `payment-db`, `ticket-db` | PostgreSQL databases owned by corresponding services. |
| `chat-db` | MongoDB database for chat. |
| `ai-search-db` | PostgreSQL with pgvector for AI search. |
| `ai-search-redis` | Redis cache for AI search. |
| `ollama`, `ollama-pull` | Local LLM/embedding model runtime and model pull helper. |
| `*-flyway` containers | Database migrations for PostgreSQL services. |
| `prometheus`, `grafana`, `loki`, `tempo`, `otel-collector`, `promtail` | Observability stack. |

## 6. Shared libraries and shared contracts

В проекте есть shared .NET messaging library:

- `backend/libraries/messaging-dotnet`;
- package/library name: `AutoRent.Messaging`;
- содержит RabbitMQ topology, routing keys, event contracts и publisher abstraction.

Основные shared event contracts:

- `ClientApprovedEmailRequested`;
- `ClientRejectedEmailRequested`;
- `PartnerApprovedEmailRequested`;
- `PartnerRejectedEmailRequested`;
- `PartnerCarApprovedEmailRequested`;
- `PartnerCarRejectedEmailRequested`;
- `PartnerCarProvisionRequested`;
- `BookingPaymentConfirmed`;
- `BookingPaymentCanceled`;
- `BookingPaymentCompleted`;
- `PartnerCarSearchDocumentChanged`;
- `UserDeleted`.

Единой общей auth-библиотеки для всех языков нет. .NET-сервисы используют похожие подходы к JWT/claims/permissions, а Node.js services имеют собственные middleware для JWT и permissions.

## 7. API Gateway

### 7.1. Реализация

Gateway реализован как custom Node.js/TypeScript reverse proxy:

- путь: `backend/external/reverse-proxy-service`;
- framework: Express;
- proxy library: `http-proxy-middleware`;
- это не YARP, не Nginx и не Spring Cloud Gateway.

### 7.2. Public ports

| Назначение | Host port по умолчанию | Container port |
|---|---:|---:|
| HTTP gateway | `9186` | `8080` |
| HTTPS gateway | `9443` | `8443` |

Gateway также имеет:

- `GET /healthz`;
- `GET /metrics`.

### 7.3. Gateway routes

Gateway удаляет внешний prefix и проксирует запрос в target service. Например, `/identity/auth/login` превращается в `/auth/login` внутри identity-service.

| Public prefix | Target service |
|---|---|
| `/identity` | `identity-service` |
| `/cars` | `car-service` |
| `/ai` | `ai-search-service` |
| `/bookings` | `booking-service` |
| `/clients` | `client-service` |
| `/partners` | `partner-service` |
| `/tickets` | `ticket-service` |
| `/files` | `file-service` |
| `/chat` | `chat-service` |
| `/internal` | `image-service` public media route |
| `/payments` | `payment-service` |

### 7.4. Security and observability features in gateway

Реализовано:

- CORS allowlist для frontend origins;
- rate limiting per IP, default: 300 requests per 60 seconds;
- security headers:
  - `X-Content-Type-Options: nosniff`;
  - `X-Frame-Options: DENY`;
  - `Referrer-Policy: strict-origin-when-cross-origin`;
  - `X-Permitted-Cross-Domain-Policies: none`;
  - `Cross-Origin-Opener-Policy: same-origin`;
  - HSTS для HTTPS;
- `X-Request-Id` generation/propagation;
- `traceparent` propagation через OpenTelemetry;
- JSON request logging;
- Prometheus metrics;
- optional TLS listener.

Gateway не является основным authorization enforcement point. Он проксирует запросы, а JWT/permissions проверяются в целевых сервисах.

### 7.5. Какие сервисы не доступны публично

Backend application services, кроме `api-gateway`, не публикуют host ports в основном `docker-compose.yml` и доступны внутри Docker network. Публично через host доступны gateway и frontend-приложения.

Отдельные infrastructure ports опубликованы для разработки и observability:

- RabbitMQ management UI: `15672`;
- AI search PostgreSQL: `1836`;
- AI search Redis: `6380`;
- Prometheus: `9090`;
- Tempo: `3200`;
- Loki: `3100`;
- Grafana: `3000`.

## 8. Data architecture

### 8.1. Database-per-service

Архитектура использует database-per-service. Каждый доменный сервис владеет своей базой и не должен напрямую читать или писать БД другого сервиса. Межсервисное взаимодействие выполняется через HTTP API, internal API key endpoints и RabbitMQ events.

По коду и compose-файлу не обнаружено нормального бизнес-сценария, где один сервис напрямую подключается к БД другого сервиса. Это хорошая формулировка для диплома: strict database ownership with service-level integration.

### 8.2. Базы данных и главные сущности

| Service | Database/storage | Главные сущности |
|---|---|---|
| `identity-service` | `identity-db` PostgreSQL | `users`, `roles`, `permissions`, `user_roles`, `role_permissions`, `role_inheritance`, `refresh_tokens`, `activation_tokens`, `subject_types`, `actor_types`, `user_provision_requests`. |
| `chat-service` | `chat-db` MongoDB | `conversations`, `messages`. |
| `car-service` | `car-db` PostgreSQL | `brands`, `models`, `car_models`, `partner_cars`, `car_model_images`, `partner_car_images`, `features`, `car_features`, `car_comments`. |
| `ai-search-service` | `ai-search-db` PostgreSQL/pgvector, Redis | `ai_car_documents`, `ai_chat_histories`, `brand_model_aliases`, `ai_recommendation_clicks`, `user_embeddings`. |
| `booking-service` | `booking-db` PostgreSQL | `bookings`, `payment_sync_outbox_messages`, `subscription_plans`, `subscriptions`. |
| `client-service` | `client-db` PostgreSQL | `clients` with profile fields, document file names, related user id, booking block fields. |
| `partner-service` | `partner-db` PostgreSQL | `partners` with company/owner data, contract and identity documents, related user id, active/deactivation state. |
| `ticket-service` | `ticket-db` PostgreSQL | `tickets`, `ticket_workflow_outbox_messages`, `complaints`, `complaint_attachments`, `complaint_booking_access_requests`, `complaint_reopen_requests`, `complaint_action_logs`. |
| `payment-service` | `payment-db` PostgreSQL | `partner_wallets`, `customer_payments`, `partner_payouts`, `partner_ledger_entries`, `mock_payment_attempts`, `booking_charges`, `processed_integration_events`. |
| `file-service` | local volume or Google Cloud Storage | Uploaded documents and generated temporary links. |
| `image-service` | local volume or Google Cloud Storage | Uploaded images and image variants. |
| `email-service` | No DB | Stateless email consumer/sender. |
| `api-gateway` | No DB | Stateless edge proxy. |
| `car-market-value-service` | No DB | Stateless market value estimation. |
| `ai-damage-eval-service` | No DB | Stateless damage evaluation inference. |

### 8.3. Migrations

PostgreSQL-backed services use Flyway migration containers. SQL migrations are stored under service-level `src/Migrations` directories. Examples:

- `identity-flyway` for `identity-db`;
- `car-flyway` for `car-db`;
- `booking-flyway` for `booking-db`;
- `client-flyway` for `client-db`;
- `partner-flyway` for `partner-db`;
- `payment-flyway` for `payment-db`;
- `ticket-flyway` for `ticket-db`;
- `ai-search-flyway` for `ai-search-db`.

.NET services also contain EF Core entity mappings, but the compose setup relies on Flyway SQL migrations for database initialization.

## 9. Event-driven architecture

### 9.1. RabbitMQ topology

Основной exchange:

- `autorent.events`.

Основные очереди:

- `email-service.notifications`;
- `payment-service.booking-payments`;
- `car-service.partner-car-provision`;
- `ai-search-service.indexing`;
- `booking-service.user-deleted`.

### 9.2. Реальные события и routing keys

| Routing key / event | Publisher | Consumer | Назначение |
|---|---|---|---|
| `ticket.email.client-approved` | `ticket-service` | `email-service` | Email после approve client ticket. |
| `ticket.email.client-rejected` | `ticket-service` | `email-service` | Email после reject client ticket. |
| `ticket.email.partner-approved` | `ticket-service` | `email-service` | Email после approve partner ticket. |
| `ticket.email.partner-rejected` | `ticket-service` | `email-service` | Email после reject partner ticket. |
| `ticket.email.partner-car-approved` | `ticket-service` | `email-service` | Email после approve partner car. |
| `ticket.email.partner-car-rejected` | `ticket-service` | `email-service` | Email после reject partner car. |
| `ticket.partner-car.provision-requested` | `ticket-service` | `car-service` | Создание/обновление partner car после approval. |
| `booking.payment.confirmed` | `booking-service` | `payment-service` | Синхронизация оплаты после подтверждения booking. |
| `booking.payment.canceled` | `booking-service` | `payment-service` | Синхронизация отмены booking/payment. |
| `booking.payment.completed` | `booking-service` | `payment-service` | Финализация booking и partner ledger. |
| `car.search.partner-car-upserted` | `car-service` | `ai-search-service` | Обновление AI search index. |
| `car.search.partner-car-deleted` | `car-service` | `ai-search-service` | Удаление машины из AI search index. |
| `user.deleted` | `identity-service` | `booking-service` | Реакция booking-service на удаление user. |
| `chat.email.new-message` | `chat-service` | `email-service` | Email notification for new chat message. |

### 9.3. Где используется outbox

Outbox реально используется в двух сервисах:

| Service | Table | Назначение |
|---|---|---|
| `ticket-service` | `ticket_workflow_outbox_messages` | Надежная отправка событий после ticket workflow: email events, partner car provisioning, booking completion/cancellation side effects. |
| `booking-service` | `payment_sync_outbox_messages` | Надежная отправка payment sync events после booking confirmed/canceled/completed. |

Outbox-сообщения содержат payload JSON, attempt count, last error, next attempt time, processed time and lock fields. Dispatcher выполняет retries с backoff. Это снижает риск потери события после изменения локальной БД.

### 9.4. Retry, dead-letter queue and status tracking

Реализовано:

- retry/backoff в outbox dispatchers;
- attempt count and last error в outbox tables;
- manual ack/nack у RabbitMQ consumers;
- persistent idempotency в `payment-service` через `processed_integration_events`;
- in-memory deduplication в `email-service`.

Ограничения:

- явной dead-letter queue topology в проекте сейчас нет;
- часть consumers при ошибке делает requeue, но dedicated DLQ не настроена;
- `ai-search-service` при ошибке может reject без requeue;
- RabbitMQ queue metrics не подключены к Prometheus из коробки.

Для диплома корректная формулировка: "Система уже использует outbox и retry/status tracking for selected critical workflows, но полноценный DLQ layer является дальнейшим улучшением."

### 9.5. Event-driven workflows

Точно event-driven:

- ticket approval/rejection -> email notification;
- partner car approval -> car-service provisioning -> AI search indexing;
- booking confirmed/canceled/completed -> payment-service ledger/wallet synchronization;
- user deleted -> booking-service cancels active bookings;
- chat new message -> email notification;
- partner car changed/deleted -> AI search index update/delete.

## 10. Security, JWT and permissions

### 10.1. JWT creation

JWT создаётся в `identity-service`. Токен подписывается RSA SHA256. Refresh tokens хранятся отдельно, в хешированном виде.

JWT claims:

- `sub`: user id;
- `username`;
- `subject_type`, например `user`, `service`, `api_key`, `system`;
- `actor_type`, например `client`, `partner`, `admin`, `internal`;
- multiple `permissions` claims.

Email сейчас не является надежным JWT claim. Frontend может использовать fallback из localStorage/login flow, но для диплома не стоит утверждать, что email всегда находится внутри JWT.

Identity-service также публикует JWKS endpoint:

- `/.well-known/jwks.json`.

### 10.2. Кто проверяет JWT

JWT проверяют сами target services, а не gateway:

- .NET services используют JWT Bearer validation по RSA public key;
- `file-service` и `image-service` имеют Node.js JWT/permission middleware;
- `chat-service` проверяет JWT для user APIs и SignalR;
- frontend использует claims для UI routing, но backend enforcement остается в сервисах.

Gateway отвечает за edge concerns: proxying, CORS, rate limit, security headers, request id, tracing. Он не является источником бизнес-авторизации.

### 10.3. Internal API key

В проекте есть internal API key для межсервисных внутренних endpoint-ов. Он передаётся через header:

- `X-Internal-Api-Key`.

Internal API key используется, например, в identity, booking, client, partner, ticket, payment, file/chat internal APIs. Это нужно для service-to-service вызовов, которые не должны быть публичными пользовательскими endpoint-ами.

### 10.4. Список permissions

Реальные permissions из identity migrations:

| Domain | Permissions |
|---|---|
| Roles/permissions | `Role.Create`, `Role.AssignPermission`, `Role.View`, `Permission.Create`, `Permission.View` |
| Users | `User.AssignRole`, `User.RemoveRole`, `User.Create`, `User.View`, `User.Update`, `User.Deactivate`, `User.Activate`, `User.Delete` |
| Tickets | `Ticket.View`, `Ticket.Approve`, `Ticket.Reject`, `Ticket.ViewAll` |
| Files/images | `File.Create`, `File.Read`, `File.Delete`, `Image.Create`, `Image.Delete` |
| Clients | `Client.View`, `Client.Create`, `Client.Update`, `Client.Delete`, `Client.Block` |
| Partners | `Partner.View`, `Partner.Create`, `Partner.Update`, `Partner.Delete`, `Partner.Deactivate` |
| Bookings | `Booking.Create`, `Booking.View`, `Booking.Update`, `Booking.Delete` |
| Car models | `CarModel.Create`, `CarModel.Update`, `CarModel.Delete` |
| Partner cars | `PartnerCar.Create`, `PartnerCar.Update`, `PartnerCar.Delete`, `PartnerCar.View`, `PartnerCar.ViewOwn` |
| Car comments | `CarComment.Create`, `CarComment.Update`, `CarComment.Delete` |
| Car images | `CarImage.Create`, `CarImage.Update`, `CarImage.Delete` |
| Complaints | `Complaint.View`, `Complaint.Review`, `Complaint.Resolve` |
| Complaint actions | `Complaint.Action.CancelBooking`, `Complaint.Action.WaiveCharge`, `Complaint.Action.Escalate`, `Complaint.Action.RefundCharge` |
| Access/payment | `AccessRequest.Review`, `Payment.View`, `Payment.Update` |
| Legacy car permissions | `Car.Create`, `Car.Update`, `Car.Delete`, `Car.Image.Create` |

Legacy car permissions существуют в ранних migrations, но актуальная car domain model использует `CarModel.*`, `PartnerCar.*`, `CarComment.*`, `CarImage.*`.

### 10.5. Permissions по ролям

| Role | Permissions summary |
|---|---|
| `user` | `Booking.Create`; partner-car own workflows such as `PartnerCar.Create`, `PartnerCar.Update`, `PartnerCar.Delete`, `PartnerCar.ViewOwn`; comments/images create/update/delete; image create/delete. Реальное разделение client/partner дополнительно зависит от `actor_type`. |
| `manager` | `Ticket.View`, `Ticket.Approve`, `Ticket.Reject`, `Client.Block`, complaint view/review/resolve and complaint action permissions. |
| `data-manager` | `Client.View`, `Client.Update`, `Client.Delete`, `PartnerCar.View`, `PartnerCar.Update`, `PartnerCar.Delete`, `Booking.View`, `Booking.Update`, `Booking.Delete`, `Client.Block`, `Complaint.View`. |
| `supermanager` | Inherits manager and data-manager permissions; дополнительно `Ticket.ViewAll`, `User.View`, `AccessRequest.Review`, `Payment.View`, `Payment.Update`, `Partner.Deactivate`. |
| `admin` | Administrative/domain management permissions: users/roles/permissions, tickets, client and partner management, files/images, car model/partner car/comment/image permissions, complaints, access requests, payments. |
| `superadmin` | All permissions. |

### 10.6. Protected endpoint groups

| Service | Protection model |
|---|---|
| `identity-service` | Auth endpoints are public. Users/roles/permissions endpoints require `User.*`, `Role.*`, `Permission.*`. Internal user provisioning endpoints use `X-Internal-Api-Key`. |
| `car-service` | Public catalog reads. Car model writes require `CarModel.*`. Partner car writes require `PartnerCar.*`. Own partner car list requires `PartnerCar.ViewOwn`. Internal partner car APIs use internal API key. |
| `booking-service` | User booking flows require JWT and ownership checks. Creating booking requires `Booking.Create`. Internal/all management endpoints use `Booking.View`/`Booking.Update` or internal API key depending endpoint. |
| `client-service` | Management endpoints use `Client.View/Create/Update/Delete/Block`. Profile endpoints use JWT ownership. Internal endpoints use internal API key. |
| `partner-service` | Management endpoints use `Partner.View/Create/Update/Delete/Deactivate`. Partner self endpoints use JWT/actor context. Internal endpoints use internal API key. |
| `ticket-service` | Ticket queues and details require `Ticket.View`; approve/reject use `Ticket.Approve`/`Ticket.Reject`; all-ticket/supermanager views use `Ticket.ViewAll`; complaint/access workflows use `Complaint.*` and `AccessRequest.Review`. |
| `payment-service` | View endpoints require `Payment.View`; update/operations use `Payment.Update` or internal API key depending workflow. |
| `file-service` | Normal routes use `File.Create`, `File.Read`, `File.Delete`; internal routes use internal API key. |
| `image-service` | Upload/delete routes use `Image.Create`/`Image.Delete`; serving images is public/internal depending route. |
| `chat-service` | User chat and SignalR require JWT; internal conversation/message APIs use internal API key. |

## 11. Observability

### 11.1. Metrics

Prometheus currently scrapes:

- `api-gateway:8080/metrics`;
- `ticket-service:8080/metrics`;
- `identity-service:8080/metrics`;
- Prometheus self-metrics.

То есть реальные `/metrics` сейчас имеют:

- `api-gateway`;
- `ticket-service`;
- `identity-service`.

Gateway metrics include:

- HTTP requests in flight;
- HTTP request total/count;
- HTTP request duration histogram.

Ticket-service and identity-service expose HTTP request metrics and observability metrics related to request processing. Ticket-service also exposes upstream HTTP metrics for internal calls.

### 11.2. Tracing

Tracing реализован через OpenTelemetry:

- `api-gateway` uses Node OpenTelemetry instrumentation;
- `identity-service` uses ASP.NET Core OpenTelemetry instrumentation;
- `ticket-service` uses ASP.NET Core and HTTP client instrumentation;
- traces are exported to OpenTelemetry Collector and Tempo.

`traceparent` is propagated across gateway and instrumented services.

### 11.3. X-Request-Id

`X-Request-Id` есть и реально используется:

- gateway генерирует или принимает incoming `X-Request-Id`;
- gateway возвращает его в response и прокидывает upstream;
- `identity-service` и `ticket-service` имеют request observability middleware, который принимает/создает request id and includes it in logs/responses;
- ticket-service outgoing HTTP calls propagate request context.

### 11.4. Logs and Promtail

Promtail собирает JSON logs из файлов:

- `api-gateway`;
- `ticket-service`;
- `identity-service`;
- `car-service`;
- `booking-service`;
- `email-service`;
- `ai-search-service`.

Логи маркируются labels such as:

- `service`;
- `event`;
- `method`;
- `route`;
- `statusCode`;
- `target` / `outcome` for ticket upstream calls;
- `routingKey` / `transport` for email events.

### 11.5. Grafana dashboards

Готовый dashboard есть:

- `ops/observability/grafana/dashboards/autorent-observability.json`;
- title: `AutoRent Observability`.

Dashboard показывает:

- Gateway Request Rate;
- Gateway Avg Duration;
- Ticket Service Request Rate;
- Ticket Service Upstream Rate;
- Ticket Service Avg Upstream Duration;
- Identity Service Request Rate;
- Identity Service Avg Duration;
- Application Logs.

### 11.6. Метрики, которые стоит показать в дипломе

Уже можно показывать:

- request count/rate by service, route and status;
- average/request duration;
- error rate by status code;
- in-flight requests;
- upstream request rate/duration for ticket-service;
- application logs correlated by service and request id;
- traces across gateway, identity-service and ticket-service.

Как future improvement можно предложить:

- RabbitMQ queue length, ready messages and unacked messages;
- DLQ message count if DLQ topology is added;
- outbox backlog and retry count;
- booking-payment sync lag;
- AI search indexing failures;
- Redis cache hit rate for AI search;
- email delivery success/failure rate.

## 12. Почему микросервисы, а не монолит

Для этого проекта микросервисная архитектура подходит лучше монолита по нескольким причинам.

Во-первых, домены системы имеют естественные границы: identity, car catalog, bookings, partners, clients, tickets, payments, files/images, chat and AI search. Эти области имеют разные модели данных, разные правила доступа и разные lifecycle workflows. Разделение на сервисы позволяет явно закрепить ownership за каждым доменом.

Во-вторых, система обслуживает разные роли и frontend-приложения. External frontend, internal frontend и superadmin frontend используют разные сценарии, но заходят через единый gateway. Микросервисы позволяют развивать клиентские, партнёрские, менеджерские и административные функции независимо.

В-третьих, часть функций является асинхронной по природе: ticket approval, partner car provisioning, email notifications, booking-payment synchronization, AI search indexing. RabbitMQ и outbox pattern лучше выражают такие workflow, чем синхронный монолитный вызов внутри одного приложения.

В-четвертых, AI/search/pricing/damage evaluation имеют особые зависимости: pgvector, Redis, Ollama, Python/FastAPI, YOLO/OpenCV, external market scraping. В монолите эти зависимости усложнили бы основной backend. В микросервисах AI-related services можно изолировать, масштабировать и развивать отдельно.

В-пятых, payment, booking и ticket workflows требуют надежности и idempotency. Разделение сервисов позволяет локализовать риски: сбой email или AI indexing не должен останавливать core booking flow.

Важно признать, что на раннем MVP монолит был бы проще. Но для дипломного проекта с несколькими ролями, approval workflow, AI features, платежной логикой, observability и database-per-service подходом микросервисы лучше демонстрируют масштабируемую архитектуру и реальные distributed systems trade-offs.

## 13. Business requirements that shaped the architecture

Главные business requirements, которые повлияли на архитектуру:

- много ролей и actor types: customer/client, partner, manager, supermanager, admin, superadmin;
- отдельные user interfaces для external, internal и superadmin сценариев;
- partner approval и partner car approval через ticket workflow;
- customer booking lifecycle with payment and completion review;
- complaints and operational review flows;
- partner finance: wallet, ledger, payouts;
- AI search/recommendations as a separate product feature;
- future dynamic pricing and market value estimation;
- image/file handling for documents, car photos and damage/completion review;
- observability for distributed request tracing and debugging;
- security boundary between public APIs and internal service-to-service APIs;
- potential expansion to premium rental markets.

Важное уточнение: в коде сейчас нет отдельной Dubai-specific интеграции. Market-value service использует external car market scraping, а не Dubai-specific API. Поэтому Dubai market лучше описывать как business context or future target market, not as already implemented feature.

## 14. Trade-offs and limitations

Реалистичные trade-offs, которые стоит признать в дипломе:

- микросервисы сложнее запускать и деплоить, чем монолит;
- появляется больше infrastructure: Docker Compose, RabbitMQ, many databases, observability stack;
- network calls can fail, so services need retries, timeouts and idempotency;
- event-driven workflows сложнее debugging without logs/traces;
- database-per-service исключает простые cross-database joins and transactions;
- consistency between services is eventual, not immediate;
- gateway становится важной edge dependency;
- permissions and JWT validation must stay consistent across services;
- outbox реализован только в критичных сервисах, не во всех publishers;
- полноценные dead-letter queues пока не настроены;
- RabbitMQ metrics are not yet scraped by Prometheus;
- часть shared logic повторяется между сервисами because there is no universal cross-language auth library.

## 15. Diploma-safe summary

Короткая формулировка для диплома:

> AutoRent uses a microservice architecture with a custom Node.js API Gateway, Vue-based role-specific frontends, .NET/Node.js/Python backend services, database-per-service persistence, RabbitMQ-based integration events, selected outbox patterns for reliable workflows, JWT/RBAC authorization and an observability stack based on Prometheus, Grafana, Loki, Tempo and OpenTelemetry.

Что можно заявлять как реализованное:

- 15 backend application services including gateway;
- 3 frontend applications;
- PostgreSQL database-per-service for core services;
- MongoDB chat storage;
- Redis and pgvector for AI search;
- RabbitMQ event-driven workflows;
- outbox in ticket-service and booking-service;
- JWT with permissions claims;
- roles `user`, `manager`, `supermanager`, `admin`, `data-manager`, `superadmin`;
- actor types `client` and `partner`;
- gateway rate limiting, security headers, TLS option, `X-Request-Id`, tracing and metrics;
- metrics for gateway, identity-service and ticket-service;
- Grafana dashboard and Promtail log collection.

Что лучше описывать как future improvement:

- separate RBAC roles named `customer` and `partner`;
- `super-manager` with hyphen;
- complete DLQ strategy;
- RabbitMQ metrics in Prometheus;
- full metrics coverage for all services;
- Dubai-specific market integration;
- advanced dynamic pricing beyond existing market-value/AI support;
- universal shared auth library across .NET and Node.js services.
