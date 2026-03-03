# External Frontend

## Назначение
Публичный клиентский интерфейс AutoRent (пользовательская часть):
- просмотр машин;
- регистрация/активация;
- авторизация;
- создание и просмотр своих бронирований;
- создание тикета на регистрацию.

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

## Интеграция с API Gateway
Приложение использует `VITE_API_URL` (обычно `http://localhost:9186`).

Основные вызовы:
- `POST /identity/auth/login`
- `POST /identity/auth/activate`
- `GET /cars`
- `GET /cars/{id}`
- `POST /bookings`
- `GET /bookings/my`
- `POST /bookings/{id}/cancel`
- `GET /bookings/available`
- `POST /tickets`

Примечание: в коде есть клиентские методы для `/cars/comment*`, но текущий `car-service` таких маршрутов не публикует.

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
- валидный JWT: раздел `/bookings`, `GET /bookings/my`, операции со своими бронями
- `Booking.Create`: создание брони (`POST /bookings`)

