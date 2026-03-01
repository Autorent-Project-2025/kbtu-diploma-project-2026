# Image Service

## Описание
Сервис для загрузки и удаления изображений.

Основные функции:
- Принимает изображение через `multipart/form-data`.
- Валидирует тип файла (`jpeg`, `png`, `webp`) и ограничение размера.
- Конвертирует изображение в `webp` (с авто-поворотом по EXIF).
- Сохраняет файл либо локально (`uploads`), либо в Google Cloud Storage.
- Возвращает `imageId` и `imageUrl` для дальнейшего использования в других сервисах.

## Технологический стек
- `Node.js` + `TypeScript` - реализация сервиса и типизация.
- `Express` - HTTP API.
- `Multer` - обработка файловых загрузок.
- `Sharp` - обработка и конвертация изображений в `webp`.
- `@google-cloud/storage` - работа с Google Cloud Storage при `USE_WEB_STORAGE=true`.
- `Docker` + `docker-compose` - контейнеризация и локальный запуск.
- `GitHub Actions` - деплой в Heroku (`.github/workflows/deploy.yaml`).

## Запуск сервиса
### Локально (без Docker)
1. Скопировать окружение:
```bash
cp .env.example .env
```
2. Установить зависимости и запустить:
```bash
cd src
npm ci
npm run dev
```

### Через Docker Compose
1. Скопировать окружение:
```bash
cp .env.example .env
```
2. Запустить:
```bash
docker compose up --build
```

Сервис будет доступен на порту `EXTERNAL_PORT` из `.env`.

## Переменные окружения
- `EXTERNAL_PORT` - внешний порт контейнера.
- `USE_WEB_STORAGE` - режим хранения:
  - `false` - локальное хранение в `uploads`.
  - `true` - хранение в Google Cloud Storage.
- `MAX_FILE_SIZE_MB` - максимальный размер входного файла (в MB).
- `GCLOUD_CLIENT_EMAIL` - email сервисного аккаунта GCP.
- `GCLOUD_PRIVATE_KEY` - private key сервисного аккаунта.
- `GCLOUD_PROJECT_ID` - ID проекта GCP.
- `GCLOUD_BUCKET` - имя бакета GCS (обязателен при `USE_WEB_STORAGE=true`).

## Взаимодействие с сервисом
### `POST /api/images`
Загрузка изображения.

Запрос:
- `Content-Type: multipart/form-data`
- Поле файла: `imageFile`

Успешный ответ: `201 Created`
```json
{
  "imageId": "2fd720d7-cc09-4e10-9f49-3ba6df15bb0d.webp",
  "imageUrl": "/public/2fd720d7-cc09-4e10-9f49-3ba6df15bb0d.webp"
}
```

Ошибки:
- `400` - файл не передан, неподдерживаемый тип, превышен размер.
- `500` - внутренняя ошибка сервиса.

### `DELETE /api/images/:imageId`
Удаление изображения по идентификатору.

Успешный ответ: `200 OK`
```json
{
  "message": "Image deleted",
  "imageId": "2fd720d7-cc09-4e10-9f49-3ba6df15bb0d.webp"
}
```

Ошибки:
- `400` - некорректный `imageId`.
- `404` - изображение не найдено.
- `500` - внутренняя ошибка сервиса.

### `GET /public/:fileName`
Отдача файла из локального хранилища `uploads` (актуально для `USE_WEB_STORAGE=false`).

При `USE_WEB_STORAGE=true` в `imageUrl` возвращается публичный URL Google Cloud Storage.

## Структура проекта
> Актуализируется по мере развития проекта

```text
./.github/workflows/deploy.yaml   // CI/CD деплой в Heroku
./docker-compose.yml              // запуск контейнера
./.env.example                    // пример переменных окружения
./src                             // исходный код сервиса
```
