# Email Service

## Назначение
Сервис отправки email через SMTP:
- `approved` письма (одобрение заявки);
- `rejected` письма (отклонение заявки);
- `custom` письма (произвольная тема/тело).

## Быстрый запуск
1. Скопировать окружение:
```bash
cp .env.example .env
```
2. Запустить локально:
```bash
cd src
npm ci
npm run start
```
3. Или через Docker Compose:
```bash
docker compose up --build
```

## Переменные окружения
- `EXTERNAL_PORT` - внешний порт контейнера.
- `PORT` - внутренний порт HTTP-сервиса (по умолчанию `8080`).
- `SMTP_VERIFY_ON_STARTUP` - проверять SMTP при старте (`true/false`).
- `SMTP_HOST` - SMTP хост.
- `SMTP_PORT` - SMTP порт.
- `SMTP_SECURE` - `true` для SMTPS (обычно `465`), `false` для STARTTLS (`587`).
- `SMTP_USER` - SMTP логин.
- `SMTP_PASS` - SMTP пароль / app password.
- `SMTP_FROM` - отправитель, пример: `"Autorent <no-reply@autorent.kz>"`.

## API
### `GET /health`
Проверка доступности сервиса.

### `POST /emails/approved`
```json
{
  "to": "user@example.com",
  "fullName": "Arlan",
  "loginEmail": "arlan@example.com",
  "setPasswordUrl": "https://site/set-password?token=..."
}
```

### `POST /emails/rejected`
```json
{
  "to": "user@example.com",
  "fullName": "Arlan",
  "reason": "Фото документа не читается"
}
```

### `POST /emails/custom`
```json
{
  "to": "user@example.com",
  "subject": "Заголовок",
  "text": "Текст письма",
  "html": "<p>HTML письмо</p>",
  "replyTo": "support@example.com"
}
```
