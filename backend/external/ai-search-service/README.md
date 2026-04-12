# AI Search Service

Сервис AI-поиска по машинам AutoRent.

Он принимает свободный текст пользователя, извлекает из него структурированные фильтры, выполняет гибридный поиск по заранее проиндексированным машинам и возвращает:

- текст ответа ассистента;
- применённые фильтры;
- список наиболее подходящих машин;
- историю диалога пользователя, если фронт использует chat mode.

## Что делает сервис

Сервис решает две отдельные задачи:

1. Online-поиск и подбор машин по free-text запросу пользователя.
2. Offline/async индексация карточек машин в локальную поисковую таблицу PostgreSQL + pgvector.

Основная идея: не ходить в `car-service` с тяжёлыми текстовыми запросами каждый раз, а держать собственный поисковый индекс `ai_car_documents`, в котором уже собраны текст, теги и embedding для каждой доступной машины.

## Основные части сервиса

- `src/index.ts`
  HTTP entrypoint, маршруты, health/metrics, периодический reindex, graceful shutdown.
- `src/ai/queryParser.ts`
  Главный оркестратор разбора пользовательского запроса.
- `src/ai/heuristicQueryParser.ts`
  Rule-based parser для бюджета, мест, коробки, рейтинга, года, бренда и стиля.
- `src/ai/localLlmQueryParser.ts`
  Извлечение structured filters через локальную LLM.
- `src/ai/openAiQueryParser.ts`
  Извлечение structured filters через OpenAI-compatible API.
- `src/search/searchService.ts`
  Hybrid retrieval, фильтрация, rerank и формирование причин рекомендации.
- `src/indexing/searchIndexer.ts`
  Полная и точечная переиндексация документов.
- `src/messaging/indexingConsumer.ts`
  RabbitMQ consumer для событий изменения машин.
- `src/chat/chatHistoryRepository.ts`
  Чтение и сохранение истории диалога.
- `src/embeddings/*`
  Провайдеры embedding: local deterministic, local LLM, OpenAI-compatible.

## API

- `GET /healthz`
- `GET /metrics`
- `POST /recommendations`
- `GET /history`
- `PUT /history`
- `POST /internal/reindex`
- `POST /internal/reindex/partner-cars/:partnerCarId`

## Как работает запрос `/recommendations`

### 1. Входной payload

Сервис ожидает:

- `prompt: string`
- `messages?: AiChatMessage[]`

`prompt` обязателен. Если его нет или он пустой, сервис вернёт `400`.

`messages` не обязателен, но важен для follow-up запросов вроде:

- `подешевле`
- `теперь автомат`
- `не спорт`
- `а можно с рейтингом выше`

Перед использованием история нормализуется и чистится.

### 2. Разбор запроса через `queryParser`

Файл `src/ai/queryParser.ts` это центральный оркестратор разбора. Он не просто парсит текст, а собирает итоговый `ParsedRecommendationQuery`.

Структура `ParsedRecommendationQuery`:

- `prompt`
- `maxBudgetPerHour`
- `passengers`
- `transmission`
- `minRating`
- `preferredStyles`
- `excludedStyles`
- `preferredBrands`
- `minYear`
- `startTime`
- `endTime`
- `requiresAvailableOnDates`

### 3. Первый слой: heuristic parser

Сначала всегда вызывается `parseQueryHeuristically(prompt)`.

Эвристический parser умеет извлекать:

- бюджет в час:
  - `до 10000`
  - `до 15 тыс`
- количество мест:
  - `6 мест`
  - `4 человека`
- коробку:
  - `автомат`
  - `механика`
- рейтинг:
  - `рейтинг от 4.5`
  - `rating 4+`
- минимальный год:
  - `от 2020`
  - `2021+`
- предпочтительные стили:
  - `sport`
  - `business`
  - `family`
  - `city`
  - `luxury`
- исключённые стили:
  - `не спорт`
  - `без luxury`
  - `кроме family`
- бренды из словаря:
  - `toyota`, `bmw`, `audi` и т.д.

Это базовый слой надёжности. Даже если LLM недоступна, сервис всё равно умеет работать.

### 4. Второй слой: LLM parser

После эвристики сервис пытается улучшить разбор:

- если задан `LOCAL_LLM_BASE_URL`, используется `localLlmQueryParser`;
- иначе, если задан `OPENAI_API_KEY`, используется `openAiQueryParser`;
- если оба варианта недоступны или LLM вызов упал, сервис остаётся на heuristic parser.

LLM parser должен вернуть только JSON по фиксированной схеме.

Он особенно полезен для:

- более свободных формулировок;
- извлечения дат;
- более мягкого понимания style/brand intent;
- случаев, где rule-based parser ничего не нашёл.

### 5. reconcile: как сервис объединяет heuristic и LLM

Сервис не доверяет LLM безусловно.

После ответа модели применяется `reconcileWithHeuristics(...)`, где:

- бюджет берётся из heuristic parser, если он его нашёл;
- пассажиры берутся из heuristic parser, если он их нашёл;
- коробка берётся из heuristic parser;
- рейтинг берётся из heuristic parser;
- год учитывается только если в prompt есть явный year intent;
- стили и бренды модель может расширить, но не в follow-up запросах, где это рискованно.

Идея простая: rule-based логика отвечает за точные поля, а модель помогает там, где нужна семаника.

### 6. Наследование контекста из истории

Если текущий prompt короткий или похож на продолжение предыдущего запроса, `queryParser` может унаследовать прошлые фильтры из истории чата.

Сервис ищет последнее сообщение ассистента, у которого есть `appliedFilters`, и использует его как предыдущий контекст.

Это срабатывает для запросов вида:

- `не спорт`
- `подешевле`
- `дороже`
- `теперь автомат`
- `от 2022`
- `6 мест`

Контекст не наследуется для явного reset/greeting, например:

- `привет`
- `сначала`
- `новый запрос`
- `reset`

При merge:

- отсутствующие поля берутся из прошлого запроса;
- новые `excludedStyles` объединяются с предыдущими;
- `preferredStyles` очищаются от того, что пользователь исключил;
- даты и флаг availability тоже могут наследоваться.

### 7. Clarification или полноценный поиск

После парсинга сервис решает, достаточно ли критериев для реального поиска.

Если filters почти пустые, а текст слишком общий, сервис не запускает поиск, а возвращает уточняющий ответ.

Примеры запросов, где сервис может попросить уточнение:

- `привет`
- `что посоветуешь`
- `машину`

Если в запросе уже есть достаточные search signals, запускается поиск.

## Как работает `searchCars`

Файл: `src/search/searchService.ts`

### 1. Формирование retrieval prompt

Для retrieval сервис не ограничивается исходным `prompt`.
Он строит расширенный `retrievalPrompt`, в который добавляет:

- исходный текст;
- preferred styles;
- preferred brands;
- transmission;
- rating threshold;
- seats;
- budget;
- excluded styles в виде `not ...`.

Это нужно, чтобы и lexical search, и embedding search лучше отражали извлечённые фильтры.

### 2. Создание embedding

Для `retrievalPrompt` считается embedding через `createEmbedding(...)`.

Порядок провайдеров такой:

1. Если есть `LOCAL_LLM_BASE_URL`, сервис пытается получить embedding у локальной модели.
2. Если локальный embedding не удался, используется deterministic local fallback.
3. Если локальной модели нет, но есть `OPENAI_API_KEY`, можно использовать OpenAI-compatible embeddings.
4. Если и это недоступно или упало, остаётся deterministic local embedding.

Все embeddings нормализуются к размерности `128`.

### 3. Hybrid retrieval в PostgreSQL

Поиск идёт по таблице `ai_car_documents`.

В SQL одновременно считаются:

- `vectorDistance` через `pgvector`;
- `lexicalScore` через `ts_rank_cd(...)` и `websearch_to_tsquery(...)`.

Одновременно в SQL применяются жёсткие фильтры:

- `price_hour <= budget * 1.25`
- `seats >= passengers`
- `year >= minYear`
- `transmission == requested transmission`
- `rating >= minRating`
- `brand IN preferredBrands`

Лимит на этом этапе: `24` кандидата.

Важно: для бюджета в SQL есть мягкий коридор `125%`, чтобы не потерять близкие варианты, если точного совпадения нет.

### 4. Post-processing после SQL

После получения кандидатов сервис применяет ещё несколько шагов в коде:

- availability filter по датам через `booking-service`;
- preferred style filter;
- excluded style filter;
- точный budget filter.

Логика для preferred styles мягкая:

- если после style filter остались совпадения, остаются только они;
- если style filter всё вырезал, сервис откатывается к исходному набору.

То есть стиль здесь не превращается в жёсткий стоп-фильтр, который случайно убьёт все результаты.

### 5. Business rerank

Для каждого кандидата вычисляются:

- `vectorScore`
- `lexicalScore`
- `businessScore`
- `finalScore`

`businessScore` повышается за:

- попадание в бюджет;
- достаточное число мест;
- нужную коробку;
- достаточный рейтинг;
- совпадение по стилю;
- год не ниже запрошенного;
- просто высокий рейтинг машины.

Итоговая формула:

- `finalScore = vectorScore * 0.5 + lexicalScore * 0.2 + businessScore * 0.3`

После этого:

- кандидаты сортируются по `finalScore`;
- остаются top 6;
- для каждой машины формируются краткие `reasons`.

## Как формируется текст ответа

Файл: `src/ai/answerComposer.ts`

Сейчас основной recommendation text формируется детерминированно.

То есть сервис:

- не пишет итоговую рекомендацию через LLM;
- не пересказывает результаты моделью;
- не делает генеративный summary для обычного search response.

Итоговый ответ собирается шаблонно на основе:

- количества найденных машин;
- учтённого бюджета;
- коробки;
- рейтинга;
- вместимости;
- style preference.

LLM сейчас используется только для короткого clarification reply, если сервису не хватает критериев для поиска.

Если локальная LLM для clarification недоступна, используется жёстко заданный fallback text.

## Индексация

Файл: `src/indexing/searchIndexer.ts`

Индексация нужна, чтобы подготовить поисковые документы заранее.

### Полный reindex

`reindexEverything()`:

1. Запрашивает список доступных моделей из `car-service`.
2. Для каждой модели запрашивает partner cars.
3. Для каждой partner car строит документ.
4. Upsert'ит документ в `ai_car_documents`.
5. Удаляет из индекса те машины, которых больше нет среди активных.

Полный reindex вызывается:

- на старте сервиса, если `AUTO_INDEX_ON_STARTUP=true`;
- по cron-like таймеру через `AUTO_REFRESH_INTERVAL_SECONDS`;
- через `POST /internal/reindex`.

### Точечный reindex

`reindexPartnerCar(partnerCarId)`:

- обновляет один документ;
- если машина больше неактивна, удаляет её из индекса.

Вызывается:

- через `POST /internal/reindex/partner-cars/:partnerCarId`;
- через RabbitMQ события.

### Из каких сервисов тянутся данные

Через `catalogClient.ts` индексатор ходит в:

- `car-service`
  - список доступных моделей;
  - детали модели;
  - список partner cars;
  - детали partner car;
- `partner-service`
  - публичный профиль партнёра;
- `booking-service`
  - проверка availability по датам.

### Что попадает в поисковый документ

Для каждой машины сервис собирает:

- `brand`
- `model`
- `year`
- `title`
- `description`
- `color`
- `transmission`
- `fuelType`
- `engine`
- `seats`
- `priceHour`
- `priceDay`
- `rating`
- `ratingsCount`
- `imageUrl`
- `carrierName`
- `detailsUrl`
- `bookingUrl`
- `tags`
- `searchableText`
- `embedding`

`searchableText` склеивается из:

- названия модели;
- описания;
- цвета;
- двигателя;
- коробки;
- топлива;
- количества мест;
- имени партнёра;
- тегов;
- комментариев к машине.

### Теги

`buildSearchTags(...)` добавляет в документ нормализованные теги на основе:

- features;
- engine;
- transmission;
- fuelType;
- seats.

Также делаются простые нормализации вроде:

- transmission -> `automatic` / `manual`
- fuelType -> `petrol`

## Хранилище и схема БД

### `ai_car_documents`

Создаётся миграцией `V1__init_ai_search.sql`.

Таблица хранит:

- бизнес-данные по машине;
- JSON-теги;
- `searchable_text`;
- `vector_embedding vector(128)`;
- generated `tsvector` поле `lexical_document`.

Индексы:

- GIN по `lexical_document`;
- B-tree по `partner_user_id`;
- B-tree по `price_hour`;
- IVFFLAT по `vector_embedding`.

Это и есть основа hybrid retrieval.

### `ai_chat_histories`

Создаётся миграцией `V2__add_ai_chat_histories.sql`.

Хранит:

- `user_id`
- `messages jsonb`
- `updated_at`

История используется для:

- показа прошлых сообщений пользователю;
- наследования фильтров в follow-up запросах.

## История чата

Маршруты:

- `GET /history`
- `PUT /history`

Они защищены JWT middleware.

Сервис:

- вручную валидирует Bearer token;
- поддерживает только `RS256`;
- проверяет signature;
- проверяет `nbf`, `exp`;
- опционально проверяет `iss` и `aud`;
- использует `sub` как `userId`.

Перед сохранением история проходит нормализацию:

- ограничение числа сообщений;
- ограничение длины текста;
- нормализация `appliedFilters`;
- нормализация машин в сообщении;
- отбрасывание мусорных/битых значений.

## Метрики и observability

Сервис ведёт:

- `GET /healthz`
- `GET /metrics`
- structured logs через `observabilityLogger`

HTTP-метрики собираются в памяти процесса:

- общее число запросов;
- суммарная длительность по `method + route + status`.

Также логируются ключевые события:

- старт сервиса;
- ошибки startup;
- успешный/проваленный parser fallback;
- reindex completed;
- reindex failure;
- chat history load/save;
- RabbitMQ indexing failures;
- завершение HTTP request.

## RabbitMQ

Если задан `RABBITMQ_URL`, сервис поднимает consumer:

- queue: `ai-search-service.indexing`
- routing key upsert: `car.search.partner-car-upserted`
- routing key delete: `car.search.partner-car-deleted`

Поведение:

- при upsert event сервис делает `reindexPartnerCar(...)`;
- при delete event удаляет документ из `ai_car_documents`;
- при ошибке message уходит в `nack(..., requeue=false)`.

Если `RABBITMQ_URL` не задан, consumer просто не запускается.

## Fallback-стратегия

Сервис спроектирован так, чтобы деградировать мягко, а не падать целиком.

### Query parsing

- local LLM parser
- OpenAI-compatible parser
- heuristic parser

Если модель недоступна, поиск всё равно работает.

### Embeddings

- local LLM embeddings
- OpenAI-compatible embeddings
- deterministic local embeddings

Если внешние embedding-провайдеры недоступны, сервис всё равно может искать по локальному вектору и lexical search.

### Ответ ассистента

- clarification может быть сгенерирован локальной LLM;
- если это не удалось, есть deterministic fallback text;
- recommendation summary сейчас детерминированный по умолчанию.

## Ограничения текущей реализации

- heuristic parser знает только ограниченный словарь стилей и брендов;
- даты нормально извлекаются в основном через LLM parser;
- recommendation text не генерируется моделью и не делает rich summary по карточкам;
- style tagging пока довольно простой и зависит от исходных features/transmission/fuelType;
- full reindex идёт последовательно и может быть дорогим на большом каталоге;
- SQL retrieval сейчас берёт top 24 кандидата до post-filtering, это может ограничивать recall при очень большом каталоге.

## Пример жизненного цикла запроса

Запрос пользователя:

`Хочу не спорт, автомат, до 12000, рейтинг от 4.5`

Сервис делает следующее:

1. Эвристика извлекает:
   - `excludedStyles = ["sport"]`
   - `transmission = "automatic"`
   - `maxBudgetPerHour = 12000`
   - `minRating = 4.5`
2. Если доступна LLM, она может дополнительно предложить style/brand/date intent.
3. `queryParser` объединяет результат модели с эвристикой.
4. Строится retrieval prompt с этими фильтрами.
5. Считается embedding.
6. SQL выбирает кандидатов из `ai_car_documents`.
7. Сервис вырезает машины со style `sport`.
8. Считает `businessScore` и `finalScore`.
9. Возвращает top 6 машин и причины выбора.

## Environment

См. `.env.example`.

Ключевые переменные:

- `DATABASE_URL`
- `CAR_SERVICE_BASE_URL`
- `PARTNER_SERVICE_BASE_URL`
- `BOOKING_SERVICE_BASE_URL`
- `API_GATEWAY_PUBLIC_BASE_URL`
- `RABBITMQ_URL`
- `AUTO_INDEX_ON_STARTUP`
- `AUTO_REFRESH_INTERVAL_SECONDS`
- `LOCAL_LLM_BASE_URL`
- `LOCAL_LLM_CHAT_MODEL`
- `LOCAL_LLM_EMBEDDING_MODEL`
- `LOCAL_LLM_TIMEOUT_SECONDS`
- `OPENAI_API_KEY`
- `OPENAI_BASE_URL`
- `OPENAI_CHAT_MODEL`
- `OPENAI_EMBEDDING_MODEL`
