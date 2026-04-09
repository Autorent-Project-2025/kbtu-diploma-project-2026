# AI Search Service

Сервис AI-поиска по машинам AutoRent.

## Что делает
- принимает свободный текстовый запрос пользователя;
- извлекает структурированные фильтры из prompt;
- выполняет hybrid retrieval:
  - lexical full-text поиск;
  - vector similarity поиск через `pgvector`;
  - business rerank по бюджету, рейтингу и совпадению сценария;
- возвращает ответ ассистента и карточки подходящих машин;
- асинхронно переиндексирует документы по RabbitMQ событиям;
- пишет structured logs в jsonl для `promtail -> Loki`.

## Текущий срез
- embeddings:
  - если задан `LOCAL_LLM_BASE_URL`, сервис использует локальные embeddings через Ollama;
  - если локальная модель недоступна, сервис падает на deterministic local embedding fallback;
  - если задан `OPENAI_API_KEY`, сервис всё ещё умеет использовать OpenAI-compatible embeddings, но compose по умолчанию их не включает;
  - иначе использует локальный deterministic embedding fallback;
- query parsing:
  - если задан `LOCAL_LLM_BASE_URL`, сервис пробует вытащить structured query через локальную LLM в Ollama;
  - если локальная модель недоступна, сервис переключается на heuristic parser;
  - если задан `OPENAI_API_KEY`, сервис всё ещё умеет использовать OpenAI-compatible parser, но compose по умолчанию их не включает;
  - иначе использует rule-based parser;
- индексатор тянет данные по HTTP из `car-service`, `partner-service`, `booking-service`.

## API
- `GET /healthz`
- `GET /metrics`
- `POST /recommendations`
- `POST /internal/reindex`
- `POST /internal/reindex/partner-cars/:partnerCarId`

## Environment
См. `.env.example`.
