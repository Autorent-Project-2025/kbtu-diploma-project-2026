# Image Service

## Назначение
Сервис загрузки и удаления изображений для остальных сервисов.
Поддерживает 2 режима хранения:
- локальный диск (`USE_WEB_STORAGE=false`);
- Google Cloud Storage (`USE_WEB_STORAGE=true`).

## Стек
- Node.js + TypeScript
- Express
- Sharp (валидатор/конвертер изображений)
- `@google-cloud/storage` (опционально)

## API
Base path: `/`.
Через gateway сервис доступен по префиксу `/internal`.

Маршруты:
- `POST /api/images`
- `DELETE /api/images/{imageId}`
- `GET /public/{fileName}` (только для локального хранения)

Важно: `POST /api/images` принимает **raw body** с заголовком:
- `Content-Type: application/octet-stream`

Допустимые форматы входного файла: `jpeg`, `png`, `webp`.

Успешный ответ на upload:

```json
{
  "imageId": "uuid.webp",
  "imageUrl": "/public/uuid.webp"
}
```

## Переменные окружения
См. `./.env.example`:
- `EXTERNAL_PORT`
- `USE_WEB_STORAGE`
- `MAX_FILE_SIZE_MB`
- `GCLOUD_CLIENT_EMAIL`
- `GCLOUD_PRIVATE_KEY`
- `GCLOUD_PROJECT_ID`
- `GCLOUD_BUCKET`

## Запуск
### Локально
Из `backend/shared/image-service/src`:

```bash
npm ci
npm run dev
```

### Через compose сервиса
Из `backend/shared/image-service`:

```bash
docker compose up --build
```

### В составе всего проекта
Из корня репозитория:

```bash
docker compose up --build image-service
```

Сервис будет доступен на порту `EXTERNAL_PORT` (по умолчанию `9181`).
