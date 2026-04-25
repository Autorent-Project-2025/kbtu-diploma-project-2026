# AutoRent

<div align="center">
  <p><strong>Микросервисная платформа каршеринга</strong></p>
  <p>15 backend-сервисов, 3 frontend-приложения, единый API Gateway, event-driven workflows через RabbitMQ, AI-подбор автомобилей и observability-стек из Prometheus, Grafana, Loki и Tempo.</p>
</div>

<table>
  <tr>
    <td align="center" width="25%"><strong>15</strong><br/>backend-сервисов</td>
    <td align="center" width="25%"><strong>3</strong><br/>frontend-приложения</td>
    <td align="center" width="25%"><strong>1</strong><br/>root <code>docker-compose.yml</code></td>
    <td align="center" width="25%"><strong>1</strong><br/>внешняя точка входа</td>
  </tr>
</table>

## Навигация

- [Обзор](#overview)
- [Ключевые сценарии](#scenarios)
- [Архитектура](#architecture)
- [Layered backend service structure](#backend-service-layers)
- [Быстрый старт](#quick-start)
- [Публичные точки входа](#entry-points)
- [Карта репозитория](#repo-map)
- [Справка](#reference)

<a id="overview"></a>
## Обзор

AutoRent объединяет внешний клиентский контур, внутренний менеджерский контур и отдельный superadmin-интерфейс вокруг единого `api-gateway`. Доменные сервисы изолированы по базам данных, внутренние workflow вынесены в `RabbitMQ` и outbox-паттерн, AI-функции вынесены в отдельные сервисы, а наблюдаемость подключена сразу на уровне root compose.

| Слой | Компоненты | Назначение |
|---|---|---|
| Frontend | `frontend/external`, `frontend/internal`, `frontend/superadmin` | Клиентский UI, UI менеджера и UI супер-админа |
| Edge | `api-gateway` | Единственная внешняя HTTP/HTTPS-точка входа, route rewrite, TLS, rate limiting, security headers |
| Shared backend | `identity-service`, `chat-service`, `email-service`, `image-service` | Аутентификация, чаты, уведомления, хранение изображений |
| Domain backend | `car-service`, `booking-service`, `client-service`, `partner-service`, `ticket-service`, `payment-service`, `file-service` | Каталог, бронирования, onboarding, partner cabinet, финансы и документы |
| AI/support backend | `ai-search-service`, `car-market-value-service`, `ai-damage-eval-service` | AI-подбор, рыночная стоимость, advisory-проверка повреждений |
| Runtime infrastructure | PostgreSQL/pgvector, MongoDB, Redis, Ollama, `RabbitMQ`, `Prometheus`, `Grafana`, `Loki`, `Tempo`, `OpenTelemetry Collector`, `Promtail` | Хранение данных, LLM/embedding runtime, messaging и observability |

<a id="scenarios"></a>
## Ключевые сценарии

| Сценарий | Что происходит | Основные компоненты |
|---|---|---|
| Клиентский контур | Каталог машин, AI-подбор, создание, оплата и завершение бронирования | `frontend/external`, `api-gateway`, `car-service`, `ai-search-service`, `booking-service`, `payment-service` |
| Регистрация и approve/reject | Создание тикетов `Client`, `Partner`, `PartnerCar`, просмотр очереди менеджером, согласование документов | `frontend/internal`, `ticket-service`, `identity-service`, `client-service`, `partner-service`, `file-service`, `image-service` |
| Approve машины партнера | После review `ticket-service` публикует событие, `car-service` создает `partner_car`, `email-service` отправляет уведомление | `ticket-service`, `RabbitMQ`, `car-service`, `email-service` |
| Финансовый контур | Mock payment flow, ledger/wallet/payouts партнера, штрафы и синхронизация статусов бронирования | `booking-service`, `payment-service`, `partner-service` |
| Чаты и жалобы | Диалог клиента/партнера/менеджера с вложениями и realtime-уведомлениями | `chat-service`, `file-service`, `frontend/external`, `frontend/internal` |
| Completion review | Клиент загружает 5 фото после поездки, AI-проверка дает advisory-оценку, менеджер принимает финальное решение | `booking-service`, `ai-damage-eval-service`, `ticket-service`, `frontend/internal` |
| Администрирование прав | Управление пользователями, ролями, permissions и role inheritance | `frontend/superadmin`, `identity-service` |

<a id="architecture"></a>
## Архитектура

Актуальная runtime-диаграмма проекта находится в [docs/project-architecture.md](./docs/project-architecture.md). Это основной источник истины для общей схемы взаимодействий.

Быстрые ссылки по системной документации:

- [Общая runtime-диаграмма](./docs/project-architecture.md)
- [Backend: взаимодействие сервисов](./backend/README.md)
- [Infrastructure и ops](./ops/README.md)
- [Наблюдаемость и телеметрия](./ops/observability/README.md)

Ключевые принципы текущей архитектуры:

- Внешний трафик входит только через `api-gateway`; backend-сервисы и БД наружу не публикуются.
- Каждый доменный сервис владеет своей БД и не пишет напрямую в БД другого сервиса.
- Workflow `ticket-service` и финансовая синхронизация `booking-service` используют outbox + `RabbitMQ`.
- `ai-search-service` держит собственный pgvector-индекс и обновляется через reindex API, периодический refresh и события `RabbitMQ`.
- `ai-damage-eval-service` работает как advisory-only сервис: при недоступности AI booking flow деградирует в ручную проверку менеджером.
- JWT и JWKS централизованы в `identity-service`.

<a id="backend-service-layers"></a>
## Layered backend service structure

Most .NET backend services follow the same layered project shape: `Api`, `Application`, `Domain` and `Infrastructure`. The exact files differ by service, but the responsibility split is consistent across the main .NET services.

```text
backend/
└── <area>/
    └── <service-name>/
        └── src/
            ├── <ServiceName>.Api/
            │   ├── Controllers/
            │   ├── Contracts/
            │   ├── Middleware/
            │   ├── Options/
            │   └── Program.cs
            ├── <ServiceName>.Application/
            │   ├── Commands/
            │   ├── Queries/
            │   ├── DTOs/
            │   ├── Interfaces/
            │   └── Models/
            ├── <ServiceName>.Domain/
            │   ├── Entities/
            │   ├── Enums/
            │   └── ValueObjects/
            ├── <ServiceName>.Infrastructure/
            │   ├── Persistence/
            │   ├── Integrations/
            │   ├── Services/
            │   ├── Options/
            │   └── Observability/
            └── Migrations/
                └── V*_*.sql
```

| Layer | Responsibility |
|---|---|
| `Api` | Exposes HTTP endpoints, configures ASP.NET Core, authentication, authorization policies, middleware, dependency injection and request/response contracts. |
| `Application` | Contains use-case logic boundaries: commands, queries, handlers, DTOs, service interfaces and integration abstractions used by the API layer. |
| `Domain` | Contains core business entities, enums and domain rules that should not depend on infrastructure, databases or HTTP frameworks. |
| `Infrastructure` | Implements persistence, EF Core DbContext, repositories where used, service implementations, typed HttpClient integrations, RabbitMQ publishers/consumers, background workers and observability helpers. |
| `Migrations` | Contains Flyway SQL migrations that create and evolve the service-owned PostgreSQL schema. |

Example services with this structure:

- `backend/shared/identity-service/src`
- `backend/external/car-service/src`
- `backend/external/booking-service/src`
- `backend/internal/ticket-service/src`
- `backend/internal/payment-service/src`

Non-.NET services use a lighter structure, but keep the same boundary idea: HTTP API entrypoint, domain/service logic, infrastructure clients and configuration.

<a id="quick-start"></a>
## Быстрый старт

Требования:

- Docker
- Docker Compose
- Доступ к образам Docker Hub и моделям Ollama при первом запуске AI-поиска

Запуск из корня репозитория:

```bash
docker compose up --build
```

Перед первым запуском заполните нужные `.env` файлы по `.env.example`.

Что поднимется сразу:

- все frontend-приложения;
- `api-gateway`;
- backend-сервисы;
- PostgreSQL/pgvector и MongoDB-базы;
- Redis и Ollama для AI-поиска;
- `RabbitMQ`;
- observability-стек (`Prometheus`, `Grafana`, `Loki`, `Tempo`, `Promtail`, `OpenTelemetry Collector`).

GPU для Ollama включается отдельным override-файлом:

```bash
docker compose -f docker-compose.yml -f docker-compose.gpu.yml up -d
```

<a id="entry-points"></a>
## Публичные точки входа

| Поверхность | URL / порт | Примечание |
|---|---|---|
| API Gateway HTTP | `http://localhost:9186` | Основная HTTP-точка входа |
| API Gateway HTTPS | `https://localhost:9443` | Dev TLS с self-signed сертификатом |
| External Frontend | `http://localhost:5173` | Клиентский UI |
| Internal Frontend | `http://localhost:5174` | UI менеджера |
| Superadmin Frontend | `http://localhost:5175` | UI супер-админа |
| Grafana | `http://localhost:3000` | Dashboard и correlation log -> trace |
| Prometheus | `http://localhost:9090` | Метрики |
| Loki | `http://localhost:3100` | Логи |
| Tempo | `http://localhost:3200` | Traces |
| RabbitMQ Management | `http://localhost:15672` | Management UI |

> В текущем root compose внешней backend-точкой входа остается только `api-gateway`. Остальные backend-сервисы работают во внутренних Docker networks.

Основные gateway routes:

| Route | Upstream |
|---|---|
| `/identity/*` | `identity-service` |
| `/cars/*` | `car-service` |
| `/ai/*` | `ai-search-service` |
| `/bookings/*` | `booking-service` |
| `/clients/*` | `client-service` |
| `/partners/*` | `partner-service` |
| `/tickets/*` | `ticket-service` |
| `/files/*` | `file-service` |
| `/chat/*` | `chat-service` |
| `/payments/*` | `payment-service` |
| `/internal/*` | `image-service` для публичных изображений |

<a id="repo-map"></a>
## Карта репозитория

### Системная документация

| Документ | Что внутри |
|---|---|
| [README.md](./README.md) | Вход в проект, быстрый старт и навигация |
| [docs/project-architecture.md](./docs/project-architecture.md) | Runtime-диаграмма и ключевые архитектурные потоки |
| [backend/README.md](./backend/README.md) | Межсервисные backend-интеграции |
| [ops/README.md](./ops/README.md) | Infrastructure и ops-структура |
| [ops/observability/README.md](./ops/observability/README.md) | Метрики, логи, traces и Grafana provisioning |

### Frontend-приложения

| Компонент | Документация | Назначение |
|---|---|---|
| External Frontend | [frontend/external/README.md](./frontend/external/README.md) | Клиентский UI, AI-подбор, бронирование, жалобы, кабинет партнера |
| Internal Frontend | [frontend/internal/README.md](./frontend/internal/README.md) | UI менеджера для тикетов, жалоб, бронирований, финансов и администрирования |
| Superadmin Frontend | [frontend/superadmin/README.md](./frontend/superadmin/README.md) | Управление пользователями, ролями и permissions |

### Shared backend

| Сервис | Документация | Назначение |
|---|---|---|
| Identity Service | [backend/shared/identity-service/README.md](./backend/shared/identity-service/README.md) | Auth, JWT, JWKS, users, roles, permissions |
| Chat Service | [backend/shared/chat-service/README.md](./backend/shared/chat-service/README.md) | Conversations, SignalR, вложения и внутренние system messages |
| Email Service | [backend/shared/email-service/README.md](./backend/shared/email-service/README.md) | SMTP-уведомления и RabbitMQ consumer |
| Image Service | [backend/shared/image-service/README.md](./backend/shared/image-service/README.md) | Хранение и выдача изображений |

### Domain backend

| Сервис | Документация | Назначение |
|---|---|---|
| API Gateway | [backend/external/reverse-proxy-service/README.md](./backend/external/reverse-proxy-service/README.md) | Edge, route rewrite, security, metrics, tracing |
| Car Service | [backend/external/car-service/README.md](./backend/external/car-service/README.md) | Каталог, partner cars, `/cars/match` |
| AI Search Service | [backend/external/ai-search-service/README.md](./backend/external/ai-search-service/README.md) | AI-рекомендации, pgvector-индекс, Redis-кэш, Ollama |
| Booking Service | [backend/external/booking-service/README.md](./backend/external/booking-service/README.md) | Бронирования и платежный контур |
| Client Service | [backend/external/client-service/README.md](./backend/external/client-service/README.md) | Профили клиентов |
| Partner Service | [backend/internal/partner-service/README.md](./backend/internal/partner-service/README.md) | Профили партнеров и фасад кабинета партнера |
| Ticket Service | [backend/internal/ticket-service/README.md](./backend/internal/ticket-service/README.md) | Онбординг, approve/reject и оркестрация |
| Payment Service | [backend/internal/payment-service/README.md](./backend/internal/payment-service/README.md) | Wallet, ledger, payouts и внутренние платежи |
| File Service | [backend/internal/file-service/README.md](./backend/internal/file-service/README.md) | Приватные документы и временные ссылки |
| Car Market Value Service | [backend/internal/car-market-value-service/README.md](./backend/internal/car-market-value-service/README.md) | Оценка рыночной стоимости по данным `kolesa.kz` |
| AI Damage Eval Service | [backend/internal/ai-car-damage-eval-service/README.md](./backend/internal/ai-car-damage-eval-service/README.md) | Advisory AI-проверка фото при завершении бронирования |

### Libraries

| Компонент | Документация | Назначение |
|---|---|---|
| Backend Libraries | [backend/libraries/README.md](./backend/libraries/README.md) | Общие backend-библиотеки |
| AutoRent.Messaging | [backend/libraries/messaging-dotnet/README.md](./backend/libraries/messaging-dotnet/README.md) | RabbitMQ topology, contracts, publisher |

<a id="reference"></a>
## Справка

<details>
<summary><strong>Наблюдаемость</strong></summary>

<br/>

Базовые метрики сейчас реализованы для `api-gateway`, `ticket-service` и `identity-service`. `Promtail` собирает JSON-логи `api-gateway`, `ticket-service`, `identity-service`, `car-service`, `booking-service`, `email-service` и `ai-search-service`.

Что доступно:

- `X-Request-Id` и `traceparent` проходят через edge и backend-цепочки;
- `GET /metrics` доступен для `api-gateway`, `ticket-service` и `identity-service`;
- distributed traces экспортируются через `OpenTelemetry Collector` в `Tempo`;
- логи перечисленных сервисов собираются в `Loki` и коррелируются с trace через `Grafana`.

Основные endpoints:

- Gateway metrics: `http://localhost:9186/metrics`
- Tempo ready: `http://localhost:3200/ready`
- Loki API: `http://localhost:3100/loki/api/v1/query`
- Grafana: `http://localhost:3000`
- Prometheus: `http://localhost:9090`

</details>

<details>
<summary><strong>Предсозданные пользователи (seed)</strong></summary>

<br/>

После применения миграций `identity-service` доступны следующие логины:

| Роль/назначение | Email | Пароль | Примечание |
|---|---|---|---|
| Superadmin | `superadmin@local` | `SuperAdmin123!` | Полный доступ, роль `superadmin` |
| Обычный пользователь | `user@autorent.local` | `DemoUser123!` | Роль `user`, плюс seed-профиль в `client-service` |
| Партнер (demo) | `partner@autorent.local` | `DemoPartner123!` | Роль `user`, плюс seed-профиль в `partner-service` |
| Менеджер | `manager@autorent.local` | `DemoManager123!` | Роль `manager`, доступ во внутреннюю панель |

</details>

<details>
<summary><strong>Модель прав (permissions)</strong></summary>

<br/>

Права передаются в JWT в claim `permissions`.

### Backend-права по сервисам

| Сервис | Необходимые права |
|---|---|
| Identity Service | `User.View`, `User.Create`, `User.Update`, `User.AssignRole`, `User.RemoveRole`, `User.Activate`, `User.Deactivate`, `User.Delete`, `Role.View`, `Role.Create`, `Role.AssignPermission`, `Permission.View`, `Permission.Create` |
| Car Service | `CarModel.*`, `PartnerCar.*`, `CarComment.*`, `CarImage.*` |
| Booking Service | `Booking.Create` для создания, остальные пользовательские операции требуют валидный JWT |
| Client Service | `Client.View`, `Client.Create`, `Client.Update`, `Client.Delete` |
| Partner Service | `Partner.View`, `Partner.Create`, `Partner.Update`, `Partner.Delete` |
| Ticket Service | `Ticket.View`, `Ticket.ViewAll`, `Ticket.Approve`, `Ticket.Reject`, `Complaint.View`, `Complaint.Review`, `AccessRequest.Review` |
| Payment Service | Внутренние операции требуют `X-Internal-Api-Key`; read-only view API через gateway требует `Payment.View` |
| File Service | `File.Create`, `File.Read`, `File.Delete` |
| Image Service | `Image.Create`, `Image.Delete` |
| Chat Service | Требует валидный JWT для пользовательских conversation API; внутренние conversation операции требуют `X-Internal-Api-Key` |
| Email Service | Не требуются |
| API Gateway | Не требуются |

### Frontend-права

| Frontend | Правила доступа |
|---|---|
| External Frontend | Просмотр каталога и создание тикета без прав; бронирование и автоподбор требуют JWT, создание брони требует `Booking.Create` |
| Internal Frontend | Разделы открываются по permissions: `Ticket.*`, `Client.View`, `Partner.View`, `PartnerCar.View`, `Booking.View`, `Complaint.*`, `AccessRequest.Review`, `Payment.View`, `User.View` |
| Superadmin Frontend | Вход требует `User.View`; role management использует `Role.View` / `Role.Create` / `Role.AssignPermission`, user management использует `User.*`, справочник прав требует `Permission.View` |

</details>
