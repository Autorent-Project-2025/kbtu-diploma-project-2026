# Backend: взаимодействие сервисов

## Назначение
Этот документ описывает, как backend-сервисы в AutoRent взаимодействуют друг с другом в общем `docker-compose`.

## Состав backend
- `shared/identity-service` - аутентификация, роли, permissions, выдача JWT, внутренний provisioning пользователя.
- `shared/email-service` - отправка email-уведомлений.
- `shared/image-service` - загрузка/удаление изображений.
- `external/reverse-proxy-service` - API Gateway (входная точка для фронтендов).
- `external/car-service` - каталог автомобилей.
- `external/booking-service` - бронирования.
- `external/client-service` - профили клиентов.
- `internal/partner-service` - профили партнеров.
- `internal/ticket-service` - заявки и оркестрация внутренних интеграций.
- `internal/file-service` - приватные документы и временные ссылки.

## Вход в backend через gateway
Фронтенды ходят в `api-gateway`, а он делает route rewrite и проксирует запросы в нужный сервис:

- `/identity/*` -> `identity-service`
- `/cars/*` -> `car-service`
- `/bookings/*` -> `booking-service`
- `/clients/*` -> `client-service`
- `/partners/*` -> `partner-service`
- `/tickets/*` -> `ticket-service`
- `/files/*` -> `file-service`
- `/internal/*` -> `image-service`

Пример:
- внешний вызов `POST /identity/auth/login`
- внутри gateway -> `POST {IDENTITY_SERVICE_URL}/auth/login`

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
2. `ticket-service` -> `identity-service`: `POST /internal/users/provision` (`X-Internal-Api-Key`).
3. `ticket-service` -> `client-service`: `POST /internal/clients/provision` (`X-Internal-Api-Key`).
4. `ticket-service` -> `email-service`: `POST /emails/approved`.

### 4) Approve партнерского тикета
Путь:
1. Менеджер вызывает `POST /tickets/{id}/approve`.
2. `ticket-service` -> `identity-service`: `POST /internal/users/provision` (`X-Internal-Api-Key`).
3. `ticket-service` -> `partner-service`: `POST /internal/partners/provision` (`X-Internal-Api-Key`).
4. `ticket-service` -> `email-service`: `POST /emails/partners/approved`.

### 5) Reject тикета
Путь:
1. Менеджер вызывает `POST /tickets/{id}/reject`.
2. `ticket-service` отправляет email:
3. для client: `POST /emails/rejected`
4. для partner: `POST /emails/partners/rejected`
5. для partner-car: `POST /emails/partners/cars/rejected`

### 6) Approve тикета типа PartnerCar
Путь:
1. Партнер создает тикет `PartnerCar` через `POST /tickets`.
2. `ticket-service`:
   - получает контекст текущего партнера через `partner-service /me`;
   - загружает ownership PDF в `file-service`;
   - загружает фото машины в `image-service` (`POST /api/images`).
3. Менеджер вызывает `POST /tickets/{id}/approve`.
4. `ticket-service` -> `car-service`: `POST /internal/partner-cars/provision` (`X-Internal-Api-Key`).
5. `ticket-service` -> `email-service`: `POST /emails/partners/cars/approved`.

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

## Авторизация между сервисами
Используются два механизма.

### JWT (пользовательские и менеджерские API)
- `identity-service` выдает JWT.
- Остальные сервисы валидируют JWT по публичному RSA-ключу (`Jwt__PublicKey` или `JWT_PUBLIC_KEY`).
- Доступ к бизнес-операциям контролируется claim `permissions`.

### X-Internal-Api-Key (внутренние S2S endpoint)
- Для внутренних endpoint используется заголовок `X-Internal-Api-Key`.
Проверка выполняется на принимающей стороне:
- `identity-service`: `/internal/users/provision`
- `client-service`: `/internal/clients/provision`
- `partner-service`: `/internal/partners/provision`
- `file-service`: `/api/internal/files/*`
- `booking-service`: `/internal/bookings/*`
- `car-service`: `/internal/partner-cars/provision`

В локальной конфигурации общего compose сейчас используется одинаковое значение:
- `local-internal-api-key`

Но это не жесткое ограничение архитектуры: ключи можно разделять по сервисам.

## Границы данных
- `identity-service` -> `identity-db`
- `car-service` -> `car-db`
- `booking-service` -> `booking-db`
- `client-service` -> `client-db`
- `partner-service` -> `partner-db`
- `ticket-service` -> `ticket-db`
- `image-service` и `file-service` -> Docker volume или Google Cloud Storage (в зависимости от `USE_WEB_STORAGE`)

Каждый сервис владеет своей БД и не пишет напрямую в БД другого сервиса.

## Прямые S2S интеграции вне ticket-service
- `car-service` <-> `booking-service`:
  - чтение связанных бронирований (`/internal/bookings/by-partner-car/{id}`);
  - агрегаты количества бронирований (`/internal/bookings/counts`);
  - массовая проверка доступности (`/internal/bookings/check-availability`) для `/cars/match`.

Остальные external/internal сервисы преимущественно обслуживают запросы через gateway и работают со своей БД.

## Где смотреть детали
- Общая оркестрация: `../docker-compose.yml`
- Gateway routes: `external/reverse-proxy-service/src/index.ts`
- Ticket S2S интеграции: `internal/ticket-service/src/TicketService.Infrastructure/Integrations`
- Валидация `X-Internal-Api-Key` в file-service: `internal/file-service/src/api/middlewares/internalApiKeyMiddleware.ts`
