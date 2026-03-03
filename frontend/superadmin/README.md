# Superadmin Frontend

## Назначение
Интерфейс супер-админа для управления доступами и пользователями:
- `Role Management`: создание ролей, назначение/снятие permissions, настройка role inheritance.
- `User Management`: CRUD пользователя, активация/деактивация, назначение/снятие ролей.

## Стек
- Vue 3 + TypeScript
- Vite
- Vue Router
- Axios

## Маршруты UI
- `/login`
- `/users`

Маршрут `/users` требует:
- валидный JWT;
- permission `User.View`.

## Интеграция с API Gateway
Базовый URL берется из `VITE_API_URL`.

### Auth
- `POST /identity/auth/login`

### Users
- `GET /identity/users`
- `POST /identity/users`
- `GET /identity/users/{id}`
- `PUT /identity/users/{id}`
- `PATCH /identity/users/{id}/activate`
- `PATCH /identity/users/{id}/deactivate`
- `DELETE /identity/users/{id}`
- `POST /identity/users/{id}/roles`
- `DELETE /identity/users/{id}/roles/{roleId}`

### Roles & Permissions
- `GET /identity/roles`
- `POST /identity/roles`
- `POST /identity/roles/{id}/permissions`
- `DELETE /identity/roles/{id}/permissions/{permissionId}`
- `POST /identity/roles/{id}/parents`
- `DELETE /identity/roles/{id}/parents/{parentRoleId}`
- `GET /identity/permissions`

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
- `User.View` - вход в `/users`, просмотр списка пользователей
- `User.Create` - создание пользователя
- `User.Update` - редактирование пользователя
- `User.Activate` - активация пользователя
- `User.Deactivate` - деактивация пользователя
- `User.Delete` - удаление пользователя
- `User.AssignRole` - назначение роли пользователю
- `User.RemoveRole` - снятие роли у пользователя
- `Role.View` - просмотр ролей
- `Role.Create` - создание роли
- `Role.AssignPermission` - назначение/снятие permission роли и управление inheritance
- `Permission.View` - загрузка справочника permissions
