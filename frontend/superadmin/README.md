# Superadmin Frontend

## Назначение
Интерфейс супер-админа для управления пользователями и ролями.

## Стек
- Vue 3 + TypeScript
- Vite
- Vue Router
- Axios

## Основные маршруты UI
- `/login`
- `/users`

Маршрут `/users` требует:
- валидный JWT;
- permission `User.View`.

## Интеграция с API Gateway
Базовый URL берется из `VITE_API_URL`.

Основные вызовы:
- `POST /identity/auth/login`
- `GET /identity/users`
- `POST /identity/users`
- `GET /identity/users/{id}`
- `PUT /identity/users/{id}`
- `PATCH /identity/users/{id}/activate`
- `PATCH /identity/users/{id}/deactivate`
- `DELETE /identity/users/{id}`
- `POST /identity/users/{id}/roles`
- `DELETE /identity/users/{id}/roles/{roleId}`
- `GET /identity/roles`

## Переменные окружения
См. `./.env.example`:
- `VITE_API_URL`
- `VITE_APP_NAME`
- `VITE_TOKEN_EXPIRY_HOURS`

## Запуск
### Локально
Из `frontend/superadmin`:

```bash
npm ci
npm run dev
```

### Через compose сервиса
Из `frontend/superadmin`:

```bash
docker compose up --build
```

### В составе всего проекта
Из корня репозитория:

```bash
docker compose up --build superadmin-frontend
```

Порт по умолчанию: `5175`.

## Необходимые права
Требования для работы интерфейса супер-админа:
- `User.View` - вход в `/users`, просмотр списка пользователей
- `User.Create` - создание пользователя
- `User.Update` - редактирование пользователя
- `User.Activate` - активация пользователя
- `User.Deactivate` - деактивация пользователя
- `User.Delete` - удаление пользователя
- `User.AssignRole` - назначение роли пользователю
- `User.RemoveRole` - снятие роли у пользователя
- `Role.View` - загрузка справочника ролей

