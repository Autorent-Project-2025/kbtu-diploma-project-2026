# AutoRent

## О проекте
AutoRent - микросервисная платформа каршеринга.

В репозитории находятся:
- 11 backend-сервисов (external/internal/shared);
- 3 frontend-приложения (external/internal/superadmin);
- общая оркестрация через `docker-compose.yml` в корне.

## Диаграмма общей структуры проекта
![Диаграмма проекта](./docs/images/project-architecture.png)

## Новое в админке
- Superadmin UI обновлен до формата control center: отдельные секции `Role Management` и `User Management`, метрики и улучшенная адаптивность.
- Добавлено полноценное управление ролями: создание роли с начальными permissions и parent-ролями.
- Добавлено наследование ролей (role inheritance): итоговые permissions вычисляются транзитивно и применяются в JWT, API и UI.

## Новые бизнес-потоки
- Добавлен поток согласования машин партнера через `ticket-service`:
  - партнер создает тикет `PartnerCar` (марка/модель, госномер, PDF собственности, фото);
  - менеджер в internal UI может отредактировать поля и approve/reject;
  - при approve: создается `partner_car` в `car-service`, отправляется email партнеру;
  - при reject: отправляется email с причиной.
- Добавлен автоподбор машины по модели и времени через `car-service`:
  - внешний фронтенд запрашивает `POST /cars/match`;
  - `car-service` выбирает доступную машину с учетом загрузки партнера, рейтинга, количества бронирований и цены;
  - если машин нет, возвращаются ближайшие доступные даты;
  - при успешном подборе фронтенд создает бронирование в `booking-service`.

## Быстрый старт
1. Заполните нужные `.env` файлы по `.env.example`.
2. Из корня проекта выполните:

```bash
docker compose up --build
```

Публичные порты по умолчанию:
- API Gateway (HTTP): `9186`
- API Gateway (HTTPS, self-signed dev cert): `9443`
- External Frontend: `5173`
- Internal Frontend: `5174`
- Superadmin Frontend: `5175`

В корневом `docker-compose.yml` backend-сервисы и БД больше не публикуются наружу. Внешняя точка входа одна: `api-gateway`.

## Наблюдаемость
Базовая наблюдаемость добавлена для `api-gateway`, `ticket-service` и `identity-service`:
- сквозные `X-Request-Id` и `traceparent`;
- JSON-логи с `requestId`/`traceId`, сбор в `Loki` через `Promtail`;
- `Prometheus`-совместимые endpoints `GET /metrics`;
- distributed traces в `Tempo` через `OpenTelemetry Collector`;
- готовый профиль compose `observability` с `Prometheus`, `Grafana`, `Loki`, `Tempo`, `Promtail` и `OpenTelemetry Collector`.

Запуск observability-стека:

```bash
docker compose --profile observability up -d --build
```

Порты по умолчанию:
- Prometheus: `9090`
- Grafana: `3000`
- Loki: `3100`
- Tempo: `3200`

Основные endpoints:
- Gateway metrics: `http://localhost:9186/metrics`
- Ticket Service metrics: `http://ticket-service:8080/metrics` внутри compose; с хоста смотреть лучше через `Prometheus`/`Grafana`
- Identity Service metrics: `http://identity-service:8080/metrics` внутри compose; с хоста смотреть лучше через `Prometheus`/`Grafana`
- Tempo ready: `http://localhost:3200/ready`
- Loki API: `http://localhost:3100/loki/api/v1/query`
- Grafana dashboard: `AutoRent Observability`

В Grafana доступны datasource-ы `Prometheus`, `Loki` и `Tempo`.
Для gateway-логов настроена корреляция `log -> trace` по полю `traceId`.

## Предсозданные пользователи (seed)
После применения миграций `identity-service` доступны следующие логины:

| Роль/назначение | Email | Пароль | Примечание |
|---|---|---|---|
| Superadmin | `superadmin@local` | `SuperAdmin123!` | Полный доступ (роль `superadmin`) |
| Обычный пользователь | `user@autorent.local` | `DemoUser123!` | Роль `user`, плюс seed-профиль в `client-service` |
| Партнер (demo) | `partner@autorent.local` | `DemoPartner123!` | Роль `user`, плюс seed-профиль в `partner-service` |
| Менеджер | `manager@autorent.local` | `DemoManager123!` | Роль `manager`, доступ в internal panel |

## Сервисы
| Сервис | Путь | Назначение |
|---|---|---|
| Identity Service | `backend/shared/identity-service` | Auth, users, roles, permissions, JWKS |
| Car Service | `backend/external/car-service` | Каталог, партнерские машины, автоподбор `/cars/match` |
| Booking Service | `backend/external/booking-service` | Бронирования и internal-проверка доступности машин |
| Ticket Service | `backend/internal/ticket-service` | Тикеты `Client/Partner/PartnerCar`, approve/reject и оркестрация |
| Payment Service | `backend/internal/payment-service` | Внутренние mock-платежи, wallet, ledger и payouts партнеров |
| Image Service | `backend/shared/image-service` | Загрузка/удаление изображений |
| File Service | `backend/internal/file-service` | Хранение приватных файлов и выдача временных ссылок |
| Email Service | `backend/shared/email-service` | SMTP-уведомления (client/partner/partner-car) |
| API Gateway | `backend/external/reverse-proxy-service` | Единственная внешняя точка входа: route rewrite, security headers, rate limiting, health checks, HTTP/HTTPS edge |
| Client Service | `backend/external/client-service` | Профили клиентов (CRUD + `/me`) |
| Partner Service | `backend/internal/partner-service` | Профили партнеров (CRUD + `/me`) |
| External Frontend | `frontend/external` | Пользовательский UI + автоподбор и бронирование по модели |
| Internal Frontend | `frontend/internal` | UI менеджера |
| Superadmin Frontend | `frontend/superadmin` | UI супер-админа |

## Модель прав (permissions)
Права передаются в JWT в claim `permissions`.

### Backend-права по сервисам
| Сервис | Необходимые права |
|---|---|
| Identity Service | `User.View`, `User.Create`, `User.Update`, `User.AssignRole`, `User.RemoveRole`, `User.Activate`, `User.Deactivate`, `User.Delete`, `Role.View`, `Role.Create`, `Role.AssignPermission`, `Permission.View`, `Permission.Create` |
| Car Service | `CarModel.*`, `PartnerCar.*`, `CarComment.*`, `CarImage.*` (по endpoint-ам сервиса) |
| Booking Service | `Booking.Create` (для создания), остальные пользовательские операции требуют валидный JWT |
| Client Service | `Client.View`, `Client.Create`, `Client.Update`, `Client.Delete` |
| Partner Service | `Partner.View`, `Partner.Create`, `Partner.Update`, `Partner.Delete` |
| Ticket Service | `Ticket.View`, `Ticket.Approve`, `Ticket.Reject` |
| Payment Service | Пользовательские permissions не требуются; сервис доступен только по внутреннему `X-Internal-Api-Key` |
| File Service | `File.Create`, `File.Read`, `File.Delete` |
| Image Service | `Image.Create`, `Image.Delete` |
| Email Service | Не требуются (в текущей реализации) |
| API Gateway | Не требуются (в текущей реализации) |

### Frontend-права
- External Frontend: просмотр каталога и создание тикета без прав; бронирование и автоподбор требуют JWT, создание брони - `Booking.Create`.
- Internal Frontend: доступ к списку заявок - `Ticket.View`; approve/reject - `Ticket.Approve`/`Ticket.Reject`.
- Superadmin Frontend: вход в panel требует `User.View`; role management использует `Role.View`/`Role.Create`/`Role.AssignPermission`, user management использует `User.*`, загрузка справочника прав - `Permission.View`.

## Документация по сервисам
Подробная документация находится в `README.md` каждого сервиса.
