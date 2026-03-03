# File Service

## Назначение
Сервис хранения приватных файлов.

Базовая логика:
- принимает файл;
- сохраняет его в Google Cloud Storage;
- возвращает имя сохраненного файла;
- выдает временную ссылку по имени файла;
- удаляет файл по имени.

По умолчанию используется Google Cloud Storage (`USE_WEB_STORAGE=true`).
Для локальной отладки можно переключить `USE_WEB_STORAGE=false`.

## Стек
- Node.js + TypeScript
- Express
- `@google-cloud/storage`

## API
Base path: `/api`.

Маршруты:
- `POST /api/files` - загрузка файла (raw body), возвращает `fileName`
- `POST /api/files/temporary-link` - выдача временной ссылки по `fileName`
- `DELETE /api/files/{fileName}` - удаление файла
- `POST /api/internal/files/upload` - внутренняя загрузка по `X-Internal-Api-Key`
- `POST /api/internal/files/temporary-link` - внутренняя выдача ссылки по `X-Internal-Api-Key`

### Upload запрос
`POST /api/files` принимает:
- Header `Content-Type: application/octet-stream`
- Header `x-file-name: <original-name.ext>`
- Raw binary body

Успешный ответ:

```json
{
  "fileName": "9c8f69fb-7db2-4307-8f1f-cf2f825d46ea.pdf"
}
```

### Получение временной ссылки
`POST /api/files/temporary-link` принимает JSON:

```json
{
  "fileName": "9c8f69fb-7db2-4307-8f1f-cf2f825d46ea.pdf"
}
```

Опционально можно передать `ttlSeconds`.

Успешный ответ:

```json
{
  "fileName": "9c8f69fb-7db2-4307-8f1f-cf2f825d46ea.pdf",
  "url": "https://storage.googleapis.com/...",
  "expiresAtUtc": "2026-03-03T13:45:00.000Z"
}
```

## Переменные окружения
См. `./.env.example`:
- `EXTERNAL_PORT`
- `USE_WEB_STORAGE`
- `MAX_FILE_SIZE_MB`
- `SIGNED_URL_TTL_SECONDS`
- `JWT_PUBLIC_KEY` (или `Jwt__PublicKey`)
- `JWT_ISSUER` (опционально)
- `JWT_AUDIENCE` (опционально)
- `INTERNAL_API_KEY` (для внутренних S2S вызовов)
- `GCLOUD_CLIENT_EMAIL`
- `GCLOUD_PRIVATE_KEY`
- `GCLOUD_PROJECT_ID`
- `GCLOUD_BUCKET`

## Запуск
### Локально
Из `backend/internal/file-service/src`:

```bash
npm ci
npm run dev
```

### Через compose сервиса
Из `backend/internal/file-service`:

```bash
docker compose up --build
```

### В составе всего проекта
Из корня репозитория:

```bash
docker compose up --build file-service
```

## Необходимые права
Права проверяются по claim `permissions` в JWT.

Требуются:
- `File.Create` - загрузка файла (`POST /api/files`)
- `File.Read` - получение временной ссылки (`POST /api/files/temporary-link`)
- `File.Delete` - удаление файла (`DELETE /api/files/{fileName}`)

Внутренние маршруты `/api/internal/files/*` не требуют JWT, но требуют заголовок `X-Internal-Api-Key`.
