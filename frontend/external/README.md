# External Frontend

## Назначение
Публичный клиентский интерфейс AutoRent (пользовательская часть):
- просмотр машин;
- витрина доступных моделей с карточками (изображение, детали, кнопки `Подробнее`/`Забронировать`);
- автоподбор машины по модели и интервалу дат из модального окна бронирования;
- регистрация/активация;
- авторизация;
- создание и просмотр своих бронирований;
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
- `/cars/:id`
- `/bookings`
- `/partner/me`
- `/partner/cars`
- `/partner/cars/:id`

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
- `POST /bookings`
- `GET /bookings/my`
- `POST /bookings/{id}/cancel`
- `GET /bookings/available`
- `POST /tickets`
- `GET /cars/my`
- `GET /cars/my/{id}`

Дополнительно для партнерского flow:
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
- валидный JWT: раздел `/bookings`, автоподбор/бронирование, партнерский кабинет (`/partner/*`)
- `Booking.Create`: создание брони (`POST /bookings`)

