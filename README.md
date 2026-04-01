# AutoRent

<div align="center">
  <p><strong>Микросервисная платформа каршеринга</strong></p>
  <p>11 backend-сервисов, 3 frontend-приложения, единый API Gateway, event-driven workflows через RabbitMQ и observability-стек из Prometheus, Grafana, Loki и Tempo.</p>
</div>

<table>
  <tr>
    <td align="center" width="25%"><strong>11</strong><br/>backend-сервисов</td>
    <td align="center" width="25%"><strong>3</strong><br/>frontend-приложения</td>
    <td align="center" width="25%"><strong>1</strong><br/>root <code>docker-compose.yml</code></td>
    <td align="center" width="25%"><strong>1</strong><br/>внешняя точка входа</td>
  </tr>
</table>

## Навигация

- [Обзор](#overview)
- [Ключевые сценарии](#scenarios)
- [Архитектура](#architecture)
- [Быстрый старт](#quick-start)
- [Публичные точки входа](#entry-points)
- [Карта репозитория](#repo-map)
- [Справка](#reference)

<a id="overview"></a>
## Обзор

AutoRent объединяет внешний клиентский контур, внутренний менеджерский контур и отдельный superadmin-интерфейс вокруг единого `api-gateway`. Доменные сервисы изолированы по базам данных, внутренние workflow вынесены в `RabbitMQ` и outbox-паттерн, а наблюдаемость подключена сразу на уровне root compose.

| Слой | Компоненты | Назначение |
|---|---|---|
| Frontend | `frontend/external`, `frontend/internal`, `frontend/superadmin` | Клиентский UI, UI менеджера и UI супер-админа |
| Edge | `api-gateway` | Единственная внешняя точка входа, route rewrite, TLS, rate limiting, security headers |
| Shared backend | `identity-service`, `email-service`, `image-service` | Аутентификация, уведомления, хранение изображений |
| Domain backend | `car-service`, `booking-service`, `client-service`, `partner-service`, `ticket-service`, `payment-service`, `file-service` | Каталог, бронирования, onboarding, partner cabinet, финансы и документы |
| Runtime infrastructure | PostgreSQL, `RabbitMQ`, `Prometheus`, `Grafana`, `Loki`, `Tempo`, `OpenTelemetry Collector`, `Promtail` | Хранение данных, messaging и observability |

<a id="scenarios"></a>
## Ключевые сценарии

| Сценарий | Что происходит | Основные компоненты |
|---|---|---|
| Клиентский контур | Каталог машин, автоподбор по модели, создание и управление бронированием | `frontend/external`, `api-gateway`, `car-service`, `booking-service` |
| Регистрация и approve/reject | Создание тикетов `Client`, `Partner`, `PartnerCar`, просмотр очереди менеджером, согласование документов | `frontend/internal`, `ticket-service`, `identity-service`, `client-service`, `partner-service`, `file-service`, `image-service` |
| Approve машины партнера | После review `ticket-service` публикует событие, `car-service` создает `partner_car`, `email-service` отправляет уведомление | `ticket-service`, `RabbitMQ`, `car-service`, `email-service` |
| Финансовый контур | Mock payment flow, ledger/wallet/payouts партнера, синхронизация статусов бронирования | `booking-service`, `payment-service`, `partner-service` |
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
- JWT и JWKS централизованы в `identity-service`.

<a id="quick-start"></a>
## Быстрый старт

Требования:

- Docker
- Docker Compose

Запуск из корня репозитория:

```bash
docker compose up --build
```

Перед первым запуском заполните нужные `.env` файлы по `.env.example`.

Что поднимется сразу:

- все frontend-приложения;
- `api-gateway`;
- backend-сервисы;
- PostgreSQL-базы;
- `RabbitMQ`;
- observability-стек (`Prometheus`, `Grafana`, `Loki`, `Tempo`, `Promtail`, `OpenTelemetry Collector`).

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
| External Frontend | [frontend/external/README.md](./frontend/external/README.md) | Клиентский UI, каталог, бронирование, кабинет партнера |
| Internal Frontend | [frontend/internal/README.md](./frontend/internal/README.md) | UI менеджера для тикетов |
| Superadmin Frontend | [frontend/superadmin/README.md](./frontend/superadmin/README.md) | Управление пользователями, ролями и permissions |

### Shared backend

| Сервис | Документация | Назначение |
|---|---|---|
| Identity Service | [backend/shared/identity-service/README.md](./backend/shared/identity-service/README.md) | Auth, JWT, JWKS, users, roles, permissions |
| Email Service | [backend/shared/email-service/README.md](./backend/shared/email-service/README.md) | SMTP-уведомления и RabbitMQ consumer |
| Image Service | [backend/shared/image-service/README.md](./backend/shared/image-service/README.md) | Хранение и выдача изображений |

### Domain backend

| Сервис | Документация | Назначение |
|---|---|---|
| API Gateway | [backend/external/reverse-proxy-service/README.md](./backend/external/reverse-proxy-service/README.md) | Edge, route rewrite, security, metrics, tracing |
| Car Service | [backend/external/car-service/README.md](./backend/external/car-service/README.md) | Каталог, partner cars, `/cars/match` |
| Booking Service | [backend/external/booking-service/README.md](./backend/external/booking-service/README.md) | Бронирования и платежный контур |
| Client Service | [backend/external/client-service/README.md](./backend/external/client-service/README.md) | Профили клиентов |
| Partner Service | [backend/internal/partner-service/README.md](./backend/internal/partner-service/README.md) | Профили партнеров и фасад кабинета партнера |
| Ticket Service | [backend/internal/ticket-service/README.md](./backend/internal/ticket-service/README.md) | Онбординг, approve/reject и оркестрация |
| Payment Service | [backend/internal/payment-service/README.md](./backend/internal/payment-service/README.md) | Wallet, ledger, payouts и внутренние платежи |
| File Service | [backend/internal/file-service/README.md](./backend/internal/file-service/README.md) | Приватные документы и временные ссылки |

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

Базовая наблюдаемость сейчас реализована для `api-gateway`, `ticket-service` и `identity-service`, а `Promtail` также собирает JSON-логи `email-service`.

Что доступно:

- `X-Request-Id` и `traceparent` проходят через edge и backend-цепочки;
- `GET /metrics` доступен для `api-gateway`, `ticket-service` и `identity-service`;
- distributed traces экспортируются через `OpenTelemetry Collector` в `Tempo`;
- логи собираются в `Loki` и коррелируются с trace через `Grafana`.

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
| Ticket Service | `Ticket.View`, `Ticket.Approve`, `Ticket.Reject` |
| Payment Service | Пользовательские permissions не требуются, сервис доступен только по `X-Internal-Api-Key` |
| File Service | `File.Create`, `File.Read`, `File.Delete` |
| Image Service | `Image.Create`, `Image.Delete` |
| Email Service | Не требуются |
| API Gateway | Не требуются |

### Frontend-права

| Frontend | Правила доступа |
|---|---|
| External Frontend | Просмотр каталога и создание тикета без прав; бронирование и автоподбор требуют JWT, создание брони требует `Booking.Create` |
| Internal Frontend | Список заявок требует `Ticket.View`; approve/reject требуют `Ticket.Approve` / `Ticket.Reject` |
| Superadmin Frontend | Вход требует `User.View`; role management использует `Role.View` / `Role.Create` / `Role.AssignPermission`, user management использует `User.*`, справочник прав требует `Permission.View` |

</details>
