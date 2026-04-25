# Backend Architecture

Дата анализа: 25 апреля 2026.

Документ описывает актуальный backend-состав проекта по `docker-compose.yml`, структуре `backend/external`, `backend/internal`, `backend/shared`, README и frontend API clients.

## 1. Актуальный состав backend

В текущей версии проекта есть 15 backend application services, включая `api-gateway`.

| Service из вопроса | Реальный статус | Комментарий |
|---|---:|---|
| `api-gateway` | Да | Docker service называется `api-gateway`, исходники лежат в `backend/external/reverse-proxy-service`. |
| `identity-service` | Да | Shared service для auth, JWT, users, roles, permissions. |
| `car-service` | Да | External domain service для каталога, моделей, partner cars, comments, images. |
| `booking-service` | Да | External domain service для booking lifecycle, payment flow and completion review. |
| `client-service` | Да | External domain service для client profiles. |
| `partner-service` | Да | Internal domain service, но используется и external partner cabinet. |
| `ticket-service` | Да | Internal domain service для moderation, approvals, complaints. |
| `payment-service` | Да | Internal domain service для mock payments, wallets, ledger, payouts, charges. |
| `file-service` | Да | Internal/shared document storage service. |
| `image-service` | Да | Shared image storage/processing service. |
| `chat-service` | Да | Shared service для conversations, messages and SignalR. |
| `email-service` | Да | Shared notification service, consumes email events. |
| `ai-search-service` | Да | External AI/search service. |
| `car-market-value-service` | Да | Internal support service для market value estimation. |
| `ai-car-damage-eval-service` | Частично как имя | Исходная директория называется `backend/internal/ai-car-damage-eval-service`, но Docker service в compose называется `ai-damage-eval-service`. В runtime и diagrams лучше писать `ai-damage-eval-service`. |

### 1.1. Полный runtime backend list из compose

1. `api-gateway`
2. `identity-service`
3. `car-service`
4. `booking-service`
5. `client-service`
6. `partner-service`
7. `ticket-service`
8. `payment-service`
9. `file-service`
10. `image-service`
11. `chat-service`
12. `email-service`
13. `ai-search-service`
14. `car-market-value-service`
15. `ai-damage-eval-service`

### 1.2. Optional/demo status

В root `docker-compose.yml` эти 15 backend-сервисов входят в default demo startup. Для них не используются отдельные Compose profiles.

Есть несколько важных нюансов:

- `ai-damage-eval-service` запускается по умолчанию, но в бизнес-логике он advisory-only. Если AI damage evaluation недоступен, booking completion review должен деградировать в ручную проверку менеджером.
- `ai-search-service` тоже входит в default compose, но зависит от `ollama`, `ollama-pull`, `ai-search-db` and Redis. Первый запуск может быть тяжелым, потому что Ollama скачивает модели.
- `car-market-value-service` запускается по умолчанию, и `car-service` зависит от его healthcheck. Это не optional в compose.
- `email-service` запускается по умолчанию, но фактическая доставка email зависит от SMTP/environment configuration. Для demo можно показывать workflow even if real email transport is not production-grade.
- Observability services are not backend application services. Prometheus/Grafana/Loki/Tempo/Promtail useful for demo, but business flows can be explained separately from observability.

### 1.3. Переименования и несовпадения имён

| Runtime name | Source folder / old name | Что писать в дипломе |
|---|---|---|
| `api-gateway` | `backend/external/reverse-proxy-service` | Писать `api-gateway`. Можно уточнить, что это custom Node.js reverse proxy. |
| `ai-damage-eval-service` | `backend/internal/ai-car-damage-eval-service` | Для runtime/compose писать `ai-damage-eval-service`; для пути к коду можно указать folder name. |

Объединённых сервисов среди списка сейчас нет. `file-service` и `image-service` остаются отдельными сервисами. `chat-service` и `email-service` тоже отдельные.

## 2. Группировка backend-сервисов

Для главы 3.2 лучше использовать вариант B:

- External services;
- Internal services;
- Shared services;
- Infrastructure/support services.

Эта группировка совпадает со структурой репозитория и общей логикой проекта: `backend/external`, `backend/internal`, `backend/shared`.

### 2.1. External services

| Service | Почему здесь |
|---|---|
| `api-gateway` | Внешняя точка входа для frontend and clients. В коде лежит в `backend/external/reverse-proxy-service`. |
| `car-service` | User-facing catalog, car models, partner cars. |
| `booking-service` | User-facing booking lifecycle. |
| `client-service` | Client profile and customer-facing profile endpoints. |
| `ai-search-service` | User-facing AI search/recommendation endpoints. |

### 2.2. Internal services

| Service | Почему здесь |
|---|---|
| `partner-service` | Partner profile, partner operational data, wallet/ledger/payout reads. Используется и внешним partner cabinet, но в репозитории относится к internal domain. |
| `ticket-service` | Internal moderation, approvals, complaints, access requests. |
| `payment-service` | Internal financial state, ledger, wallets, charges and payouts. |
| `file-service` | Internal document/attachment storage and temporary links. |
| `car-market-value-service` | Internal pricing support service для `car-service`. |
| `ai-damage-eval-service` | Internal AI support service для booking completion review. |

### 2.3. Shared services

| Service | Почему здесь |
|---|---|
| `identity-service` | Shared auth/RBAC/JWT service for all frontends and backend services. |
| `chat-service` | Shared conversation service used by external and internal users. |
| `email-service` | Shared notification consumer/sender. |
| `image-service` | Shared image upload/storage service. |

### 2.4. Infrastructure/support services

Это не application backend services, но они нужны для runtime:

- PostgreSQL databases;
- MongoDB for chat;
- Redis for AI search cache;
- RabbitMQ;
- Ollama and `ollama-pull`;
- Flyway migration containers;
- Prometheus, Grafana, Loki, Tempo, OpenTelemetry Collector, Promtail.

## 3. Реальная ответственность каждого сервиса

### 3.1. `api-gateway`

What it implements: единая HTTP/HTTPS точка входа, reverse proxy, route rewriting, CORS, rate limiting, security headers, request id, metrics and tracing.

Who uses it: все frontend-приложения, browser clients, external API consumers.

Most important features:

- routes `/identity`, `/cars`, `/bookings`, `/clients`, `/partners`, `/tickets`, `/payments`, `/chat`, `/ai`, `/files`, `/internal`;
- `X-Request-Id` propagation;
- Prometheus `/metrics`;
- optional TLS;
- WebSocket proxying for chat.

### 3.2. `identity-service`

What it implements: authentication, JWT issuing, refresh tokens, users, roles, permissions, role inheritance, JWKS.

Who uses it: external frontend, internal frontend, superadmin frontend, backend services for JWT validation/provisioning.

Most important features:

- login/refresh flow;
- RSA-signed JWT;
- permissions claims;
- users/roles/permissions management;
- activation and provisioning flows;
- `/.well-known/jwks.json`.

### 3.3. `car-service`

What it implements: car catalog, brands/models, partner cars, car images, comments, car matching, price estimate integration.

Who uses it: external frontend, internal frontend, booking-service, ai-search-service, ticket-service through partner car provisioning events.

Most important features:

- catalog and model details;
- partner car creation/update/delete;
- public partner car details;
- car comments;
- partner car image handling through image-service;
- market value estimate through `car-market-value-service`;
- publishes search indexing events for AI search.

### 3.4. `booking-service`

What it implements: booking creation, availability validation, payment flow, status transitions, customer/partner booking actions, completion review.

Who uses it: external frontend, internal frontend, payment-service through events, ticket-service, car-service, partner-service, client-service.

Most important features:

- booking statuses and lifecycle;
- price preview and availability;
- mock payment start/status/submit;
- cancel/confirm/start/complete flows;
- completion review with uploaded photos;
- advisory damage evaluation integration;
- payment sync outbox events.

### 3.5. `client-service`

What it implements: client profile, personal data, document metadata, related identity user link, booking access/blocking state.

Who uses it: external frontend, internal frontend, ticket-service during client approval, booking-service for client-related checks.

Most important features:

- `/clients/profile` for customer profile;
- client management for internal users;
- booking access block/unblock;
- identity user relation;
- document metadata for onboarding.

### 3.6. `partner-service`

What it implements: partner profile, partner documents, partner activation/deactivation, wallet/ledger/payout read APIs for partner-facing screens.

Who uses it: external partner frontend, internal frontend, ticket-service during partner approval, booking-service, car-service.

Most important features:

- partner self-profile;
- partner management for internal users;
- partner wallet, ledger and payout views;
- partner booking views;
- temporary links for partner files;
- activation/deactivation.

### 3.7. `ticket-service`

What it implements: approval/moderation workflow for client, partner and partner car tickets; complaints; access requests; booking completion and partner cancellation review.

Who uses it: external frontend, internal frontend, identity-service, client-service, partner-service, car-service, file-service, image-service, email-service.

Most important features:

- ticket submission with files/images;
- manager queue;
- approve/reject tickets;
- complaint queue and complaint actions;
- booking access requests;
- ticket workflow outbox;
- partner car provisioning events;
- email notification events.

### 3.8. `payment-service`

What it implements: mock customer payment state, partner wallets, partner ledger entries, payouts, booking charges, processed integration events.

Who uses it: booking-service, partner-service, internal frontend, payment event consumers.

Most important features:

- booking payment synchronization;
- partner wallet balance;
- ledger entries;
- payouts;
- booking charges/fines;
- idempotent event handling with `processed_integration_events`.

### 3.9. `file-service`

What it implements: file/document upload, read/delete and temporary links for documents and attachments.

Who uses it: ticket-service, chat-service, partner-service and frontend flows indirectly through domain APIs.

Most important features:

- document storage;
- attachment storage;
- temporary download links;
- local volume or cloud storage abstraction;
- permission/internal API key protection.

### 3.10. `image-service`

What it implements: image upload, image delete, serving images and image processing.

Who uses it: external frontend for profile/image upload, car-service, ticket-service, frontend flows through gateway `/internal`.

Most important features:

- image upload;
- image delete;
- Sharp-based processing;
- local volume or cloud storage abstraction;
- public/internal image serving routes.

### 3.11. `chat-service`

What it implements: conversations, messages, chat attachments and realtime communication.

Who uses it: external frontend, internal frontend, email-service through chat notification events.

Most important features:

- conversation by context;
- message history;
- SignalR hub;
- attachment temporary links;
- chat email notification event.

### 3.12. `email-service`

What it implements: email notification sending from integration events and service calls.

Who uses it: ticket-service, chat-service, booking-related flows where configured.

Most important features:

- consumes ticket approval/rejection email events;
- consumes chat new message notification events;
- SMTP/Nodemailer integration;
- basic deduplication for events;
- stateless notification delivery.

### 3.13. `ai-search-service`

What it implements: AI recommendations, semantic car search, chat-like recommendation history, vector indexing for partner cars.

Who uses it: external frontend, car-service through indexing events, car-service/partner-service/booking-service as data sources.

Most important features:

- `/ai/recommendations`;
- AI history;
- pgvector search index;
- Redis cache;
- Ollama local LLM/embedding integration;
- auto-index and periodic refresh;
- consumes partner car search indexing events.

### 3.14. `car-market-value-service`

What it implements: market value estimation by brand/model/year using external market data.

Who uses it: car-service.

Most important features:

- `GET /market-value/estimate`;
- `POST /market-value/estimate`;
- scraping/parsing external car listings;
- median market value calculation;
- confidence based on available samples.

### 3.15. `ai-damage-eval-service`

What it implements: advisory AI damage evaluation for booking completion photos.

Who uses it: booking-service.

Most important features:

- `POST /inspect-session`;
- validates 4-5 uploaded photos;
- loads computer vision model;
- returns advisory damage findings;
- protected by `X-Internal-Api-Key`;
- fail-open integration in booking flow: manager can still review manually.

## 4. Какие backend-сервисы связаны с frontend

### 4.1. External frontend

External frontend directly calls these gateway prefixes and backend services:

| Gateway prefix | Backend service | Для чего |
|---|---|---|
| `/identity` | `identity-service` | Login, refresh, activation status, account activation. |
| `/cars` | `car-service` | Catalog, car details, partner cars, comments, price estimate. |
| `/ai` | `ai-search-service` | AI recommendations and AI history. |
| `/bookings` | `booking-service` | My bookings, create booking, payment flow, completion, cancellation. |
| `/clients` | `client-service` | Customer profile. |
| `/partners` | `partner-service` | Partner profile, wallet, ledger, partner bookings, public partner profile. |
| `/tickets` | `ticket-service` | Client/partner/partner car applications, complaints. |
| `/chat` | `chat-service` | Conversations, messages, SignalR chat. |
| `/internal` | `image-service` | Image upload/delete/display routes exposed through gateway. |

Indirect services used by external frontend:

- `payment-service` через `booking-service`;
- `file-service` через ticket/chat/partner domain APIs;
- `email-service` через async events;
- `car-market-value-service` через `car-service`;
- `ai-damage-eval-service` через `booking-service`.

### 4.2. Internal frontend

Internal frontend directly calls these backend services:

| Gateway prefix | Backend service | Для чего |
|---|---|---|
| `/identity` | `identity-service` | Login, refresh, users/roles/permissions in admin area. |
| `/tickets` | `ticket-service` | Tickets, approvals, complaints, access requests. |
| `/clients` | `client-service` | Client table, client details, block/unblock booking access. |
| `/partners` | `partner-service` | Partner table, partner details, wallet, ledger, payouts, activation/deactivation. |
| `/cars` | `car-service` | Partner cars table/details, update/delete, comments/images. |
| `/bookings` | `booking-service` | All bookings, booking details, cancel booking. |
| `/payments` | `payment-service` | Booking charges and payment-related review data. |
| `/chat` | `chat-service` | Internal chat panel and SignalR conversations. |

Indirect services used by internal frontend:

- `file-service` through ticket/partner/chat temporary links;
- `image-service` through ticket/car/image flows;
- `email-service` through ticket/chat events;
- `ai-damage-eval-service` through booking completion review;
- `car-market-value-service` through car-service.

### 4.3. Superadmin frontend

Superadmin frontend directly uses:

| Gateway prefix | Backend service | Для чего |
|---|---|---|
| `/identity` | `identity-service` | Login, refresh, users, roles, permissions, role inheritance, activation/deactivation/delete. |

Superadmin frontend не вызывает domain services напрямую.

## 5. Ключевые backend-сервисы для demo

Для demo и дипломной защиты лучше подробнее описывать 7 сервисов.

### 5.1. `api-gateway`

Почему ключевой: показывает единую точку входа и скрывает внутреннюю структуру backend от frontend.

Что показать:

- все frontends ходят через gateway;
- gateway routes map to services;
- rate limiting, CORS, security headers;
- `X-Request-Id`, metrics, tracing;
- backend services are not publicly exposed.

### 5.2. `identity-service`

Почему ключевой: без него невозможны login, JWT, permissions and role-based access.

Что показать:

- login/refresh/JWT;
- permissions inside JWT;
- roles and permissions management;
- superadmin frontend uses this service;
- other services validate JWT using identity public key/JWKS.

### 5.3. `car-service`

Почему ключевой: это основной customer-facing domain service для каталога.

Что показать:

- catalog and car details;
- partner cars;
- images/comments;
- price estimate via market value service;
- events to AI search index.

### 5.4. `booking-service`

Почему ключевой: это core transactional flow проекта.

Что показать:

- booking creation;
- availability and price preview;
- payment flow;
- booking lifecycle statuses;
- completion review with photos;
- payment sync outbox.

### 5.5. `ticket-service`

Почему ключевой: он показывает manager workflow and moderation logic.

Что показать:

- client/partner/partner car applications;
- approve/reject;
- complaints and access requests;
- outbox events;
- interaction with identity/client/partner/car/email/file/image services.

### 5.6. `partner-service`

Почему ключевой: он показывает partner side of marketplace.

Что показать:

- partner profile;
- partner activation/deactivation;
- partner bookings;
- wallet/ledger/payout views;
- integration with ticket approval and booking flows.

### 5.7. `payment-service`

Почему ключевой: он показывает финансовую часть системы.

Что показать:

- mock customer payments;
- partner wallets;
- ledger entries;
- payouts;
- booking charges/fines;
- idempotent event processing.

## 6. Backend technologies and patterns

Этот раздел отвечает только по тому, что реально прослеживается в коде.

### 6.1. Backend technologies

| Area | Реально используется |
|---|---|
| .NET backend | ASP.NET Core, .NET 10, EF Core, Npgsql/PostgreSQL, JWT Bearer auth, hosted services, typed `HttpClient`. |
| Node.js backend | TypeScript, Express, Axios/fetch style HTTP calls where needed, `amqplib` for RabbitMQ in Node services, Sharp in image-service, Nodemailer in email-service. |
| Python backend | FastAPI, Uvicorn, Pydantic/Pydantic settings, requests/BeautifulSoup in market value service, OpenCV/Ultralytics-style damage evaluation stack. |
| Databases | PostgreSQL, MongoDB, Redis, pgvector. |
| Messaging | RabbitMQ topic exchange with publishers/consumers. |
| Migrations | Flyway SQL migrations for PostgreSQL services. |
| Observability | Custom JSON logs, Prometheus metrics in selected services, OpenTelemetry in gateway/identity/ticket path. |

### 6.2. Patterns checklist

| Pattern / practice | Есть в коде? | Evidence / comments |
|---|---:|---|
| Layered architecture / Clean Architecture inside .NET services | Да | .NET services split into `Api`, `Application`, `Domain`, `Infrastructure` projects. This is visible in `identity-service`, `car-service`, `booking-service`, `client-service`, `partner-service`, `ticket-service`, `payment-service`, `chat-service`. It is closer to layered/clean architecture, but not perfectly strict in every service. |
| Repository pattern | Частично | Clearly used in `identity-service`, `ticket-service`, `chat-service`: repository interfaces in `Application` and implementations in `Infrastructure/Persistence/Repositories`. Other services often use EF `ApplicationDbContext` directly inside service classes instead of repository abstraction. |
| Service layer / Application services | Да | There are command/query handlers in `identity-service`, `ticket-service`, `chat-service`; domain/application service interfaces like `IBookingService`, `ICarModelService`, `IPartnerService`, `IPaymentLedgerService`, `IMockPaymentService`. |
| Dependency Injection | Да | ASP.NET Core DI is used heavily: `AddScoped`, `AddSingleton`, `AddTransient`, `AddHostedService`, `AddHttpClient`, infrastructure registration extension methods in some services. |
| DTO pattern | Да | API contracts and application DTOs exist under folders like `Api/Contracts`, `Application/DTOs`, `Application/Models`. Examples: booking DTOs, car DTOs, ticket DTOs, identity request contracts. |
| Options pattern | Да | Many `*Options.cs` classes are bound through `Configure<T>`, `AddOptions<T>()`, `IOptions<T>`, sometimes with `ValidateOnStart`. Examples: `JwtOptions`, `RabbitMqOptions`, `InternalAuthOptions`, `PaymentSyncOutboxOptions`, `TicketWorkflowOutboxOptions`. |
| HttpClient clients for service-to-service communication | Да | Typed clients are registered through `AddHttpClient<TInterface, TImplementation>`. Examples: booking-service clients for car/payment/client/identity/partner/ticket/damage/email; ticket-service clients for identity/client/partner/car/file/image/chat/booking/payment; car-service clients for partner/booking/client/image/market value. |
| Outbox pattern | Да, но не везде | Implemented in `ticket-service` and `booking-service`. Tables/entities: `ticket_workflow_outbox_messages` and `payment_sync_outbox_messages`. Dispatchers: `TicketWorkflowOutboxDispatcher`, `PaymentSyncOutboxDispatcher`. |
| Consumer/Publisher pattern | Да | RabbitMQ publishers use `IRabbitMqPublisher`/`RabbitMqPublisher` from `AutoRent.Messaging`. Consumers are implemented as `BackgroundService` in .NET services and `amqplib` consumers in Node services. |
| Middleware | Да | ASP.NET custom middlewares: `ApiExceptionMiddleware`, `RequestObservabilityMiddleware`. Node/Express middlewares exist for JWT permission checks, internal API key checks, file type checks and gateway request handling. |
| Validation layer | Частично, не как отдельный framework layer | No `FluentValidation`, `AbstractValidator` or central validator registration was found. Validation is implemented manually in command handlers, service methods, domain entities, EF configurations, controller checks and Python Pydantic schemas. So it is correct to say "manual validation", not "dedicated validation layer". |
| Flyway migrations | Да | `docker-compose.yml` has `flyway/flyway:12` migration containers for identity, car, ai-search, booking, client, partner, payment and ticket PostgreSQL databases. SQL migrations live in service-level `src/Migrations` directories. |

### 6.3. Practical interpretation for thesis

Safe wording:

> The .NET backend follows a layered architecture with API, Application, Domain and Infrastructure projects. It uses dependency injection, DTOs, options binding, typed HttpClient integrations, custom middleware and Flyway-managed SQL migrations. Repository pattern is used in selected services, especially identity, ticket and chat, while several domain services access EF Core DbContext directly through service classes. Outbox is implemented only in the critical ticket and booking workflows.

Avoid saying:

- all services use strict Clean Architecture;
- every service uses Repository pattern;
- the project has a centralized validation framework;
- every event publisher uses outbox;
- there is one universal backend middleware/auth/logging package.

## 7. Shared libraries

### 7.1. What exists

There is one real shared backend library directory:

- `backend/libraries/messaging-dotnet`;
- project: `backend/libraries/messaging-dotnet/src/AutoRent.Messaging/AutoRent.Messaging.csproj`.

This is not a service. It has no container, no database and no independent runtime. It is compiled into .NET services that reference it.

### 7.2. RabbitMQ contracts library

Yes, there is a shared .NET library for RabbitMQ contracts and messaging infrastructure.

`AutoRent.Messaging` contains:

- `RabbitMqOptions`;
- `RabbitMqTopology`;
- `IntegrationMessage`;
- `IRabbitMqPublisher`;
- `RabbitMqPublisher`;
- `RabbitMqConnectionFactoryBuilder`;
- `RabbitMqJson`;
- event contracts:
  - `BookingPaymentEvents.cs`;
  - `CarSearchEvents.cs`;
  - `PartnerCarEvents.cs`;
  - `TicketEmailEvents.cs`;
  - `UserEvents.cs`.

The current topology in code includes:

- exchange: `autorent.events`;
- queues:
  - `email-service.notifications`;
  - `payment-service.booking-payments`;
  - `car-service.partner-car-provision`;
  - `ai-search-service.indexing`;
  - `booking-service.user-deleted`;
- routing keys for ticket email events, partner car provisioning, booking payment events, car search indexing and user deletion.

The library is referenced by several .NET services, including:

- `booking-service`;
- `car-service`;
- `identity-service`;
- `ticket-service`;
- `payment-service`;
- `chat-service`.

Node.js services such as `email-service` and `ai-search-service` do not consume the .NET library directly. They use their own TypeScript/RabbitMQ code, but participate in the same RabbitMQ topology.

### 7.3. Common DTO/contracts

Partially.

What is shared:

- RabbitMQ integration event contracts in `AutoRent.Messaging.Contracts`.

What is not shared:

- HTTP request/response DTOs are not centralized in a common library.
- Each service defines its own API contracts and DTOs locally, for example under `Api/Contracts`, `Application/DTOs` or `Application/Models`.
- There is no cross-language shared contracts package for Node.js/TypeScript/Python services.

Safe wording:

> The project has shared integration event contracts for RabbitMQ, but HTTP API DTOs remain service-local.

### 7.4. Common auth library

No common auth library was found.

Current state:

- `identity-service` owns JWT issuing, refresh tokens, users, roles and permissions.
- .NET services configure JWT validation locally.
- Node.js services such as `file-service` and `image-service` have local JWT permission middleware.
- Gateway does not perform business authorization; it proxies requests.

Safe wording:

> Authentication is centralized in identity-service, but JWT validation code/configuration is implemented per service. There is no universal common auth library.

### 7.5. Common exception handling / logging

No single shared exception-handling or logging library was found.

What exists:

- multiple .NET services have local `ApiExceptionMiddleware`;
- several services have local `RequestObservabilityMiddleware`;
- .NET services use `ILogger<T>`;
- selected services write JSON observability logs through service-local log writers;
- Node services have their own logging helpers;
- Promtail collects logs from selected services.

Safe wording:

> Exception handling and observability follow similar patterns across services, but the implementation is duplicated per service rather than extracted into a shared library.

### 7.6. Summary

| Shared concern | Current status |
|---|---|
| RabbitMQ contracts/topology/publisher | Yes, `backend/libraries/messaging-dotnet`. |
| Shared HTTP DTOs | No, DTOs are service-local. |
| Shared cross-language contracts | No. |
| Common auth library | No. |
| Common exception handling library | No. |
| Common logging library | No. |
| Shared observability pattern | Partially, but implemented per service. |

## 8. Short summary for thesis

Backend текущей версии состоит из 15 application services. Для главы 3.2 лучше использовать grouping variant B: External services, Internal services, Shared services and Infrastructure/support services. Все application services входят в root compose demo, но AI-related services имеют operational nuances: `ai-search-service` depends on Ollama model downloads, а `ai-damage-eval-service` is advisory-only and can fail open into manual manager review. Runtime name for damage evaluation is `ai-damage-eval-service`, while source folder is `ai-car-damage-eval-service`.
