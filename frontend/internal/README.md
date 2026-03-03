# Internal Frontend

## Назначение
Внутренний интерфейс менеджера для обработки заявок (`tickets`).

## Стек
- Vue 3 + TypeScript
- Vite
- Vue Router
- Axios

## Основные маршруты UI
- `/login`
- `/tickets`

Маршрут `/tickets` требует:
- валидный JWT;
- permission `Ticket.View`.

## Интеграция с API Gateway
Базовый URL берется из `VITE_API_URL`.

Основные вызовы:
- `POST /identity/auth/login`
- `GET /tickets/pending`
- `GET /tickets/{id}`
- `POST /tickets/{id}/approve`
- `POST /tickets/{id}/reject`

## Переменные окружения
См. `./.env.example`:
- `VITE_API_URL`
- `VITE_APP_NAME`
- `VITE_TOKEN_EXPIRY_HOURS`

## Запуск
### Локально
Из `frontend/internal`:

```bash
npm ci
npm run dev
```

### Через compose сервиса
Из `frontend/internal`:

```bash
docker compose up --build
```

### В составе всего проекта
Из корня репозитория:

```bash
docker compose up --build internal-frontend
```

Порт по умолчанию: `5174`.
