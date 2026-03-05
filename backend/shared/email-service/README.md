# Email Service

## Назначение
Сервис отправки email через SMTP. Поддерживает шаблоны:
- `approved`;
- `rejected`;
- `partner approved`;
- `partner rejected`;
- `partner car approved`;
- `partner car rejected`;
- `custom`.

## Стек
- Node.js
- TypeScript (runtime: `node --experimental-strip-types`)
- Nodemailer

## API
Base path: `/`.

Маршруты:
- `GET /health`
- `POST /emails/approved`
- `POST /emails/rejected`
- `POST /emails/partners/approved`
- `POST /emails/partners/rejected`
- `POST /emails/partners/cars/approved`
- `POST /emails/partners/cars/rejected`
- `POST /emails/custom`

Пример `POST /emails/approved`:

```json
{
  "to": "user@example.com",
  "fullName": "Arlan",
  "loginEmail": "arlan@example.com",
  "setPasswordUrl": "https://app.example.com/activate?token=..."
}
```

## Переменные окружения
См. `./.env.example`:
- `EXTERNAL_PORT`
- `PORT`
- `SMTP_VERIFY_ON_STARTUP`
- `SMTP_HOST`
- `SMTP_PORT`
- `SMTP_SECURE`
- `SMTP_USER`
- `SMTP_PASS`
- `SMTP_FROM`

## Запуск
### Локально
Из `backend/shared/email-service/src`:

```bash
npm ci
npm run dev
```

### Через compose сервиса
Из `backend/shared/email-service`:

```bash
docker compose up --build
```

### В составе всего проекта
Из корня репозитория:

```bash
docker compose up --build email-service
```

Сервис будет доступен на порту `EXTERNAL_PORT` (по умолчанию `9182`).

## Необходимые права
В текущей реализации сервис не требует JWT, API-key или permission claim.

Доступ к маршрутам:
- `GET /health`
- `POST /emails/approved`
- `POST /emails/rejected`
- `POST /emails/partners/approved`
- `POST /emails/partners/rejected`
- `POST /emails/partners/cars/approved`
- `POST /emails/partners/cars/rejected`
- `POST /emails/custom`

без проверки ролей/прав.

