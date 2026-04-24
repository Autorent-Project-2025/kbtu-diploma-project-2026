# External Frontend

## Назначение
Публичный клиентский интерфейс AutoRent (пользовательская часть):
- просмотр машин;
- витрина доступных моделей с карточками (изображение, детали, кнопки `Подробнее`/`Забронировать`);
- автоподбор машины по модели и интервалу дат из модального окна бронирования;
- AI-подбор по свободному тексту через `/ai`;
- регистрация/активация;
- авторизация;
- создание, оплата, просмотр и завершение своих бронирований;
- completion review с загрузкой 5 фотографий после поездки;
- жалобы и чат по контексту жалобы/бронирования;
- создание тикета на регистрацию;
- партнерский раздел с машинами и тикетом на добавление новой машины.

## Стек
- Vue 3 + TypeScript
- Vite
- Vue Router
- Axios
- Tailwind CSS

## Основные маршруты UI
- `/`
- `/login`
- `/apply` (`/register` -> redirect)
- `/activate`
- `/cars`
- `/ai` (`/car-recommendations` -> redirect)
- `/cars/:id`
- `/cars/partner-cars/:id`
- `/bookings`
- `/bookings/:id`
- `/bookings/:id/payment`
- `/bookings/:id/complete`
- `/complaints`
- `/complaints/:id`
- `/profile`
- `/profile/user`
- `/profile/partner`
- `/partner/me` (`redirect -> /profile`, legacy compatibility route)
- `/partner/cars`
- `/partner/bookings`
- `/partner/cars/:id`

## Разделение client / partner
- После логина и при переходе на `/profile` приложение читает `actor_type` из JWT.
- `actor_type=partner` направляет пользователя в partner UI (`/profile/partner`, `/partner/*`).
- Любой другой `actor_type` направляет пользователя в клиентский профиль (`/profile/user`).
- Для определения типа пользователя frontend больше не использует пробный вызов `GET /partners/me`.
- `partner-service` остается источником данных партнерского кабинета, но не источником факта "этот пользователь партнер".

## Интеграция с API Gateway
Приложение использует `VITE_API_URL` (обычно `http://localhost:9186`).

Основные вызовы:
- `POST /identity/auth/login`
- `POST /identity/auth/activate`
- `GET /cars`
- `GET /cars/{id}`
- `GET /cars/available-models`
- `GET /cars/models/{id}`
- `GET /cars/partner-cars?carModelId=...`
- `GET /cars/comments/partner-cars/{partnerCarId}`
- `POST /cars/match`
- `POST /ai/recommendations`
- `GET /ai/history`
- `PUT /ai/history`
- `POST /bookings`
- `GET /bookings/my`
- `GET /bookings/{id}`
- `POST /bookings/{id}/payment/start`
- `GET /bookings/{id}/payment/status`
- `POST /bookings/{id}/payment/submit`
- `POST /bookings/{id}/complete-review`
- `POST /bookings/{id}/cancel`
- `GET /bookings/available`
- `GET /bookings/price-preview`
- `POST /tickets`
- `GET /tickets/complaints/my`
- `GET /tickets/complaints/my/{id}`
- `POST /tickets/complaints`
- `POST /tickets/complaints/my/{id}/respond`
- `GET /chat/conversations/by-context/{contextType}/{contextId}`
- `POST /chat/conversations/{conversationId}/messages`
- `GET /cars/my`
- `GET /cars/my/{id}`

Дополнительно для партнерского flow:
- JWT должен содержать `actor_type=partner`, иначе router не пустит пользователя в `/profile/partner` и `/partner/*`.
- После actor-based route guard партнерские страницы загружают данные из partner API (`/partners/me` и связанные partner endpoints).
- `POST /tickets` с `ticketType=PartnerCar` (multipart, ownership + фото машины).
- На странице `/partner/cars` партнер может вручную ввести `марку` и `модель` (с подсказками из текущего каталога), а не только выбирать готовую модель.

## Переменные окружения
См. `./.env.example`:
- `VITE_API_URL`
- `VITE_APP_NAME`
- `VITE_DEFAULT_CAR_IMAGE`
- `VITE_DEFAULT_BOOKING_HOURS`
- `VITE_TOKEN_EXPIRY_HOURS`

## Запуск
### Локально
Из `frontend/external`:

```bash
npm ci
npm run dev
```

### Через compose сервиса
Из `frontend/external`:

```bash
docker compose up --build
```

### В составе всего проекта
Из корня репозитория:

```bash
docker compose up --build frontend
```

Порт по умолчанию: `5173`.

## Необходимые права
UI не проверяет permissions явно для большинства страниц, но backend проверяет их на API-уровне.

Фактические требования:
- без прав: `/`, `/cars`, `/cars/:id`, `/apply`, `/activate`, создание тикета `POST /tickets`
- валидный JWT: раздел `/bookings`, оплата, completion review, жалобы, чат, автоподбор/бронирование, партнерский кабинет (`/partner/*`)
- валидный JWT + `actor_type=partner`: `/profile/partner`, `/partner/*`
- `Booking.Create`: создание брони (`POST /bookings`)

