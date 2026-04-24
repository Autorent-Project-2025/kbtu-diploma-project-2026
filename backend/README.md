# Backend: взаимодействие сервисов

## Назначение
Этот документ описывает, как backend-сервисы в AutoRent взаимодействуют друг с другом в общем `docker-compose`.

## Состав backend
- `libraries/messaging-dotnet` - общая .NET-библиотека для RabbitMQ topology, publisher и контрактов интеграционных событий.
- `shared/identity-service` - аутентификация, роли, permissions, выдача JWT, lookup-справочники `subject_type`/`actor_type`, внутренний provisioning пользователя.
- `shared/chat-service` - conversations, SignalR, вложения через `file-service`, email-события о новых сообщениях.
- `shared/email-service` - отправка email-уведомлений.
- `shared/image-service` - загрузка/удаление изображений.
- `external/reverse-proxy-service` - API Gateway (входная точка для фронтендов).
- `external/car-service` - каталог автомобилей.
- `external/ai-search-service` - AI-подбор автомобилей, pgvector-индекс, Redis-кэш и интеграция с Ollama/OpenAI-compatible API.
- `external/booking-service` - бронирования.
- `external/client-service` - профили клиентов.
- `internal/partner-service` - профили партнеров.
- `internal/payment-service` - wallet, ledger, payouts, mock payment attempts и штрафы.
- `internal/ticket-service` - заявки и оркестрация внутренних интеграций.
- `internal/file-service` - приватные документы и временные ссылки.
- `internal/car-market-value-service` - оценка рыночной стоимости автомобиля по `kolesa.kz`.
- `internal/ai-car-damage-eval-service` - advisory AI-проверка фото при завершении бронирования.

## Вход в backend через gateway
Фронтенды ходят в `api-gateway`, а он делает route rewrite и проксирует запросы в нужный сервис:

- `/identity/*` -> `identity-service`
- `/cars/*` -> `car-service`
- `/ai/*` -> `ai-search-service`
- `/bookings/*` -> `booking-service`
- `/clients/*` -> `client-service`
- `/partners/*` -> `partner-service`
- `/tickets/*` -> `ticket-service`
- `/files/*` -> `file-service`
- `/chat/*` -> `chat-service`
- `/payments/*` -> `payment-service`
- `/internal/*` -> `image-service`

Пример:
- внешний вызов `POST /identity/auth/login`
- внутри gateway -> `POST {IDENTITY_SERVICE_URL}/auth/login`

В корневом `docker-compose.yml` наружу опубликован только gateway. Остальные backend-сервисы и БД находятся во внутренних Docker networks.

## Наблюдаемость backend-цепочек
- Gateway проставляет и пробрасывает `X-Request-Id` и `traceparent`.
- `ticket-service` принимает эти заголовки, пишет их в логи, экспортирует входящие HTTP spans и прокидывает контекст дальше в исходящие `HttpClient` вызовы.
- `identity-service` принимает тот же контекст из gateway/`ticket-service`, пишет структурированные request-логи и экспортирует входящие HTTP spans.
- Для `ticket-service` доступны метрики входящих запросов и исходящих S2S вызовов на `GET /metrics`.
- Для `identity-service` доступны метрики входящих запросов на `GET /metrics`.
- Для `api-gateway` доступны метрики edge-трафика на `GET /metrics`.
- `Promtail` собирает JSON-логи `api-gateway`, `ticket-service`, `identity-service`, `car-service`, `booking-service`, `email-service` и `ai-search-service`.
- В обычном `docker compose up --build` поднимаются `Prometheus`, `Grafana`, `Loki`, `Tempo`, `Promtail` и `OpenTelemetry Collector`.

Это покрывает основной синхронный сценарий `gateway -> ticket-service -> internal services` и позволяет видеть:
- rate/error ratio по входящим endpoint-ам;
- среднюю длительность запросов;
- rate/error ratio по upstream-вызовам `ticket-service`;
- distributed traces между сервисами;
- корреляцию `log -> trace` и `requestId -> traceId`.

## Главные service-to-service взаимодействия
Основная внутренняя оркестрация сосредоточена в `ticket-service`.

### 1) Создание тикета
Путь:
1. Клиент вызывает `POST /tickets` через gateway.
2. `ticket-service` отправляет файлы в `file-service`: `POST /api/internal/files/upload` с `X-Internal-Api-Key`.
3. `file-service` сохраняет документы и возвращает имена файлов.
4. `ticket-service` сохраняет в `ticket-db` только имена файлов.

### 2) Получение ссылки на документ тикета
Путь:
1. Менеджер вызывает `GET /tickets/{id}/documents/{identity|license|ownership}/temporary-link`.
2. `ticket-service` запрашивает у `file-service`: `POST /api/internal/files/temporary-link` с `X-Internal-Api-Key`.
3. `file-service` генерирует временную ссылку.
4. Возвращается временная ссылка на документ.

### 3) Approve клиентского тикета
Путь:
1. Менеджер вызывает `POST /tickets/{id}/approve`.
2. `ticket-service` -> `identity-service`: `POST /internal/users/provision` (`X-Internal-Api-Key`, c `subject_type=user`, `actor_type=client`).
3. `ticket-service` -> `client-service`: `POST /internal/clients/provision` (`X-Internal-Api-Key`).
4. `ticket-service` записывает outbox-событие, dispatcher публикует `ticket.client-approved-email-requested` в `RabbitMQ`.
5. `email-service` читает событие и отправляет письмо.

### 4) Approve партнерского тикета
Путь:
1. Менеджер вызывает `POST /tickets/{id}/approve`.
2. `ticket-service` -> `identity-service`: `POST /internal/users/provision` (`X-Internal-Api-Key`, c `subject_type=user`, `actor_type=partner`).
3. `ticket-service` -> `partner-service`: `POST /internal/partners/provision` (`X-Internal-Api-Key`).
4. `ticket-service` записывает outbox-событие, dispatcher публикует `ticket.partner-approved-email-requested` в `RabbitMQ`.
5. `email-service` читает событие и отправляет письмо.

### 5) Reject тикета
Путь:
1. Менеджер вызывает `POST /tickets/{id}/reject`.
2. `ticket-service` записывает outbox-событие отказа.
3. Dispatcher публикует в `RabbitMQ` один из routing keys:
   - client: `ticket.client-rejected-email-requested`
   - partner: `ticket.partner-rejected-email-requested`
   - partner-car: `ticket.partner-car-rejected-email-requested`
4. `email-service` читает событие и отправляет письмо.

### 6) Approve тикета типа PartnerCar
Путь:
1. Партнер создает тикет `PartnerCar` через `POST /tickets`.
2. `ticket-service`:
   - получает контекст текущего партнера через `partner-service /me`;
   - загружает ownership PDF в `file-service`;
   - загружает фото машины в `image-service` (`POST /api/images`).
3. Менеджер вызывает `POST /tickets/{id}/approve`.
4. `ticket-service` публикует outbox-событие `ticket.partner-car-provision-requested`.
5. `car-service` читает событие и создает `partner_car`.
6. `ticket-service` публикует `ticket.partner-car-approved-email-requested`, `email-service` отправляет письмо.

### 7) Автоподбор машины по модели
Путь:
1. Клиентский frontend вызывает `POST /cars/match` через gateway.
2. `car-service` выбирает кандидатов `partner_cars` по `modelId` и `status=Available`.
3. `car-service` -> `booking-service`: `POST /internal/bookings/check-availability` (`X-Internal-Api-Key`).
4. Из кандидатов исключаются занятые машины.
5. `car-service` ранжирует доступные машины по метрикам:
   - загрузка партнера;
   - рейтинг;
   - количество бронирований;
   - цена.
6. Возвращается `partnerCarId` либо ближайшие `suggestedStartTimesUtc`.

### 8) AI-подбор автомобиля по свободному тексту
Путь:
1. Клиентский frontend вызывает `POST /ai/recommendations` через gateway.
2. `ai-search-service` проверяет Redis-кэш, классифицирует intent и разбирает запрос heuristic + LLM parser.
3. Сервис ищет кандидатов в `ai-search-db` через hybrid retrieval: lexical search + pgvector embedding.
4. Для availability service-to-service проверок `ai-search-service` обращается в `booking-service`, а для snapshot данных и reindex - в `car-service`.
5. Индекс обновляется на старте, по таймеру, через internal reindex endpoints и через RabbitMQ-события `car.search.partner-car-*`.

### 9) Чаты по контексту
Путь:
1. Frontend открывает conversation через `GET /chat/conversations/by-context/{contextType}/{contextId}` и подключается к SignalR hub `/chat/hubs/conversation`.
2. Сообщения отправляются в `POST /chat/conversations/{conversationId}/messages`; вложения идут multipart и сохраняются через `file-service`.
3. `chat-service` хранит conversation state в `chat-db` (MongoDB).
4. Для offline-уведомлений `chat-service` публикует событие в `RabbitMQ`, которое обрабатывает `email-service`.
5. Внутренние сервисы могут создавать/закрывать conversation через `/chat/internal/conversations/*` с `X-Internal-Api-Key`.

### 10) Completion review и AI damage assessment
Путь:
1. Клиент завершает бронирование через `POST /bookings/{id}/complete-review` и загружает 5 slot-labelled фото.
2. `booking-service` получает snapshot машины из `car-service` и вызывает `ai-damage-eval-service /inspect-session`.
3. Если AI вернул `OK` или `DAMAGES_FOUND`, `booking-service` создает review ticket для менеджера; если AI недоступен или истек timeout, ticket всё равно создается с ручной проверкой.
4. Если AI вернул `INVALID_SESSION`, клиент получает `400` с деталями по проблемным фото.
5. Менеджер в internal frontend approve/reject review; штрафы и выплаты синхронизируются с `payment-service`.

## Авторизация между сервисами
Используются два механизма.

### JWT (пользовательские и менеджерские API)
- `identity-service` выдает JWT.
- Остальные сервисы валидируют JWT по публичному RSA-ключу (`Jwt__PublicKey` или `JWT_PUBLIC_KEY`).
- JWT содержит не только `permissions`, но и `subject_type`/`actor_type`.
- Доступ к бизнес-операциям контролируется claim `permissions`.
- `actor_type` используется там, где нужно разделять доменные сценарии одного и того же субъекта. Например, внешний frontend определяет partner/client UI по `actor_type`, а не через пробный вызов `partner-service`.

### X-Internal-Api-Key (внутренние S2S endpoint)
- Для внутренних endpoint используется заголовок `X-Internal-Api-Key`.
Проверка выполняется на принимающей стороне:
- `identity-service`: `/internal/users/provision`
- `client-service`: `/internal/clients/provision`
- `partner-service`: `/internal/partners/provision`
- `file-service`: `/api/internal/files/*`
- `booking-service`: `/internal/bookings/*`
- `car-service`: `/internal/partner-cars/provision`
- `chat-service`: `/internal/conversations/*`
- `payment-service`: внутренние платежные операции

В общем compose ключи разведены по целевым сервисам:
- `local-identity-service-key`
- `local-client-service-key`
- `local-partner-service-key`
- `local-car-service-key`
- `local-booking-service-key`
- `local-payment-service-key`
- `local-file-service-key`
- `local-chat-service-key`
- `local-ai-damage-eval-service-key`

Это уменьшает blast radius по сравнению с одним общим `X-Internal-Api-Key`.

## Границы данных
- `identity-service` -> `identity-db`
- `chat-service` -> `chat-db` (MongoDB)
- `car-service` -> `car-db`
- `ai-search-service` -> `ai-search-db` (PostgreSQL + pgvector) и `ai-search-redis`
- `booking-service` -> `booking-db`
- `client-service` -> `client-db`
- `partner-service` -> `partner-db`
- `payment-service` -> `payment-db`
- `ticket-service` -> `ticket-db`
- `image-service` и `file-service` -> Docker volume или Google Cloud Storage (в зависимости от `USE_WEB_STORAGE`)

Каждый сервис владеет своей БД и не пишет напрямую в БД другого сервиса.

## Прямые S2S интеграции вне ticket-service
- `car-service` <-> `booking-service`:
  - чтение связанных бронирований (`/internal/bookings/by-partner-car/{id}`);
  - агрегаты количества бронирований (`/internal/bookings/counts`);
  - массовая проверка доступности (`/internal/bookings/check-availability`) для `/cars/match`.
- `car-service` -> `car-market-value-service`:
  - оценка рыночной стоимости для price-preview и расчета аренды.
- `ai-search-service` -> `car-service` / `partner-service` / `booking-service`:
  - сбор snapshot-данных для индекса;
  - проверка доступности и partner metadata при выдаче рекомендаций.
- `booking-service` -> `payment-service`:
  - mock payment session;
  - ledger/charge/fine/payout операции.
- `payment-service` -> gateway:
  - read-only `/payments/view/*` API для internal frontend с permission `Payment.View`.
- `booking-service` -> `ai-damage-eval-service`:
  - advisory проверка completion-фото с коротким timeout и fail-open поведением.
- `chat-service` -> `file-service`:
  - загрузка вложений и выдача temporary links.

Остальные external/internal сервисы преимущественно обслуживают запросы через gateway и работают со своей БД.

## Где смотреть детали
- Общая оркестрация: `../docker-compose.yml`
- Gateway routes: `external/reverse-proxy-service/src/index.ts`
- Ticket S2S интеграции: `internal/ticket-service/src/TicketService.Infrastructure/Integrations`
- Валидация `X-Internal-Api-Key` в file-service: `internal/file-service/src/api/middlewares/internalApiKeyMiddleware.ts`
- AI Search детали: `external/ai-search-service/README.md`
- Chat детали: `shared/chat-service/README.md`
- Damage evaluation contract: `internal/ai-car-damage-eval-service/README.md`
