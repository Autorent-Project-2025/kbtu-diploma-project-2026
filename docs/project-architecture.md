# AutoRent Project Architecture

Диаграмма ниже показывает актуальный runtime-контур проекта: 3 frontend-приложения, 15 backend/application сервисов включая `api-gateway`, их синхронные и асинхронные взаимодействия, `RabbitMQ`, базы данных, AI runtime, объектные хранилища и observability-стек. Миграционные контейнеры `*-flyway` намеренно опущены, чтобы не перегружать схему.

```mermaid
flowchart TB
  classDef actor fill:#FFFFFF,stroke:#5F6368,color:#202124;
  classDef frontend fill:#E8F0FE,stroke:#1A73E8,color:#0B1F33;
  classDef edge fill:#FFF4D6,stroke:#C47F00,color:#4A2E00;
  classDef shared fill:#E6F4EA,stroke:#1E8E5A,color:#123524;
  classDef external fill:#FDEBD2,stroke:#D97706,color:#4D2E00;
  classDef internal fill:#FCE8E6,stroke:#C53929,color:#4A1C16;
  classDef ai fill:#EEE7FF,stroke:#7C3AED,color:#2E1065;
  classDef data fill:#F8F9FA,stroke:#5F6368,color:#202124;
  classDef storage fill:#EEF3F7,stroke:#607D8B,color:#23343B;
  classDef provider fill:#FFF7E0,stroke:#B26A00,color:#4A2E00;
  classDef messaging fill:#FFF1D6,stroke:#B26A00,color:#4A2E00;
  classDef ops fill:#E8F5F2,stroke:#0F766E,color:#12332E;

  subgraph Actors["Actors"]
    direction LR
    CUSTOMER["Customer / Partner"]:::actor
    MANAGER["Manager"]:::actor
    SUPERADMIN["Superadmin"]:::actor
  end

  subgraph Frontend["Frontend Apps"]
    direction LR
    FE_EXT["External Frontend<br/>Vue 3 + Vite<br/>:5173"]:::frontend
    FE_INT["Internal Frontend<br/>Vue 3 + Vite<br/>:5174"]:::frontend
    FE_SUPER["Superadmin Frontend<br/>Vue 3 + Vite<br/>:5175"]:::frontend
  end

  subgraph Edge["Edge"]
    GATEWAY["API Gateway<br/>Node.js + Express<br/>HTTP :9186 / HTTPS :9443"]:::edge
  end

  subgraph Backend["Backend Services"]
    direction LR

    subgraph Shared["Shared"]
      direction TB
      IDENTITY["Identity Service<br/>auth, users, roles, JWKS<br/>:1244"]:::shared
      CHAT["Chat Service<br/>conversations, SignalR, attachments<br/>internal :8080"]:::shared
      EMAIL["Email Service<br/>SMTP + RabbitMQ consumer<br/>:9182"]:::shared
      IMAGE["Image Service<br/>public image storage<br/>:9181"]:::shared
    end

    subgraph External["External"]
      direction TB
      CAR["Car Service<br/>catalog, /match, partner cars<br/>:1298"]:::external
      AI_SEARCH["AI Search Service<br/>LLM recommendations + pgvector index<br/>internal :8080"]:::ai
      BOOKING["Booking Service<br/>bookings + mock payment UI<br/>:1821"]:::external
      CLIENT["Client Service<br/>client profiles<br/>:1831"]:::external
    end

    subgraph Internal["Internal"]
      direction TB
      PARTNER["Partner Service<br/>partner cabinet facade<br/>:1832"]:::internal
      TICKET["Ticket Service<br/>registration + approvals<br/>:1248"]:::internal
      PAYMENT["Payment Service<br/>wallet, ledger, payouts<br/>:1834"]:::internal
      FILE["File Service<br/>private documents<br/>:9183"]:::internal
      MARKET["Car Market Value Service<br/>market value from kolesa.kz<br/>internal :8080"]:::internal
      DAMAGE["AI Damage Eval Service<br/>completion photos advisory check<br/>internal :8000"]:::ai
    end
  end

  subgraph RuntimeData["Runtime Infra / Data Stores / Providers"]
    direction LR
    RABBIT["RabbitMQ<br/>events exchange + queues<br/>:5672 / :15672"]:::messaging
    ID_DB[("identity-db<br/>PostgreSQL")]:::data
    CHAT_DB[("chat-db<br/>MongoDB")]:::data
    CAR_DB[("car-db<br/>PostgreSQL")]:::data
    AI_DB[("ai-search-db<br/>PostgreSQL + pgvector<br/>host :1836")]:::data
    AI_REDIS[("ai-search-redis<br/>Redis cache<br/>host :6380")]:::data
    BOOKING_DB[("booking-db<br/>PostgreSQL")]:::data
    CLIENT_DB[("client-db<br/>PostgreSQL")]:::data
    PARTNER_DB[("partner-db<br/>PostgreSQL")]:::data
    TICKET_DB[("ticket-db<br/>PostgreSQL")]:::data
    PAYMENT_DB[("payment-db<br/>PostgreSQL")]:::data
    FILE_STORE[("file_uploads / Google Cloud Storage")]:::storage
    IMAGE_STORE[("image_uploads / Google Cloud Storage")]:::storage
    OLLAMA["Ollama<br/>qwen2.5 + bge-m3<br/>:11434"]:::provider
    KOLESA["kolesa.kz"]:::provider
    SMTP["SMTP provider"]:::provider
  end

  subgraph Observability["Observability"]
    direction LR
    OTEL["OpenTelemetry Collector<br/>:4318"]:::ops
    PROM["Prometheus<br/>:9090"]:::ops
    PROMTAIL["Promtail"]:::ops
    TEMPO["Tempo<br/>:3200"]:::ops
    LOKI["Loki<br/>:3100"]:::ops
    GRAFANA["Grafana<br/>:3000"]:::ops
  end

  CUSTOMER --> FE_EXT
  MANAGER --> FE_INT
  SUPERADMIN --> FE_SUPER

  FE_EXT -->|REST / HTTPS| GATEWAY
  FE_INT -->|REST / HTTPS| GATEWAY
  FE_SUPER -->|REST / HTTPS| GATEWAY

  GATEWAY -->|/identity/*| IDENTITY
  GATEWAY -->|/cars/*| CAR
  GATEWAY -->|/ai/*| AI_SEARCH
  GATEWAY -->|/bookings/*| BOOKING
  GATEWAY -->|/clients/*| CLIENT
  GATEWAY -->|/partners/*| PARTNER
  GATEWAY -->|/tickets/*| TICKET
  GATEWAY -->|/files/*| FILE
  GATEWAY -->|/chat/* + WebSocket| CHAT
  GATEWAY -->|/payments/*| PAYMENT
  GATEWAY -->|/internal/* public images| IMAGE

  TICKET -->|user provisioning| IDENTITY
  TICKET -->|client provisioning| CLIENT
  TICKET -->|partner provisioning<br/>and /me for PartnerCar| PARTNER
  TICKET -->|document upload<br/>and temp links| FILE
  TICKET -->|PartnerCar images| IMAGE
  TICKET ==>|ticket workflow outbox| RABBIT

  CAR -->|partner context for /my| PARTNER
  CAR -->|availability checks,<br/>counts, linked bookings| BOOKING
  CAR -->|car model / partner car images| IMAGE
  CAR -->|market value estimate| MARKET
  CAR ==>|partner-car search index events| RABBIT
  MARKET -->|market listings| KOLESA

  BOOKING -->|partner car snapshot| CAR
  BOOKING -->|mock payment sessions| PAYMENT
  BOOKING -->|completion photo inspection<br/>fail-open advisory| DAMAGE
  BOOKING -->|review / complaint tickets| TICKET
  BOOKING ==>|payment sync outbox| RABBIT

  PARTNER -->|temporary file links| FILE
  PARTNER -->|wallet, ledger, payouts| PAYMENT
  PARTNER -->|partner bookings| BOOKING

  CHAT -->|attachment upload<br/>and temp links| FILE
  CHAT ==>|new message email events| RABBIT

  AI_SEARCH -->|catalog snapshots| CAR
  AI_SEARCH -->|partner metadata| PARTNER
  AI_SEARCH -->|availability checks| BOOKING
  AI_SEARCH -->|chat model + embeddings| OLLAMA

  RABBIT ==>|ticket email events| EMAIL
  RABBIT ==>|chat email events| EMAIL
  RABBIT ==>|partner-car provisioning| CAR
  RABBIT ==>|search index refresh| AI_SEARCH
  RABBIT ==>|booking payment events| PAYMENT

  IDENTITY -.->|JWT / JWKS for auth| CAR
  IDENTITY -.->|JWT / JWKS for auth| AI_SEARCH
  IDENTITY -.->|JWT / JWKS for auth| BOOKING
  IDENTITY -.->|JWT / JWKS for auth| CLIENT
  IDENTITY -.->|JWT / JWKS for auth| PARTNER
  IDENTITY -.->|JWT / JWKS for auth| TICKET
  IDENTITY -.->|JWT / JWKS for auth| FILE
  IDENTITY -.->|JWT / JWKS for auth| IMAGE
  IDENTITY -.->|JWT / JWKS for auth| CHAT

  IDENTITY --> ID_DB
  CHAT --> CHAT_DB
  CAR --> CAR_DB
  AI_SEARCH --> AI_DB
  AI_SEARCH --> AI_REDIS
  BOOKING --> BOOKING_DB
  CLIENT --> CLIENT_DB
  PARTNER --> PARTNER_DB
  TICKET --> TICKET_DB
  PAYMENT --> PAYMENT_DB
  FILE --> FILE_STORE
  IMAGE --> IMAGE_STORE
  EMAIL --> SMTP

  GATEWAY -. traces .-> OTEL
  TICKET -. traces .-> OTEL
  IDENTITY -. traces .-> OTEL
  OTEL --> TEMPO

  GATEWAY -. metrics .-> PROM
  TICKET -. metrics .-> PROM
  IDENTITY -. metrics .-> PROM

  GATEWAY -. logs .-> PROMTAIL
  TICKET -. logs .-> PROMTAIL
  IDENTITY -. logs .-> PROMTAIL
  CAR -. logs .-> PROMTAIL
  BOOKING -. logs .-> PROMTAIL
  AI_SEARCH -. logs .-> PROMTAIL
  EMAIL -. logs .-> PROMTAIL
  PROMTAIL --> LOKI

  GRAFANA --> PROM
  GRAFANA --> LOKI
  GRAFANA --> TEMPO
```

## Ключевые контуры

- `api-gateway` - единственная внешняя HTTP/HTTPS-точка входа для всех frontend-приложений; backend-сервисы, `RabbitMQ` и базы данных наружу не публикуются. Gateway проксирует `/ai/*`, `/chat/*` и `/payments/*` так же, как остальные доменные маршруты, а публичный `/internal/*` используется только как прокси к `image-service`.
- `ticket-service` - синхронный orchestrator onboarding-потоков: создаёт пользователей/профили, складывает документы и изображения, а затем через outbox публикует workflow-события в `RabbitMQ` для email-уведомлений и provisioning партнерских машин.
- `car-service`, `ai-search-service` и `booking-service` образуют контур подбора и доступности машин: каталог и partner-car snapshot живут в `car-service`, AI retrieval и pgvector-индекс - в `ai-search-service`, фактическая занятость и статусы бронирований - в `booking-service`.
- `booking-service`, `payment-service` и `ai-damage-eval-service` связаны completion/payment flow: mock payment идёт по внутреннему HTTP, финансовая синхронизация статусов `Confirmed / Canceled / Completed` идёт через outbox и `RabbitMQ`, а проверка пяти completion-фото вызывается как advisory-only AI-интеграция.
- `partner-service` выступает как фасад кабинета партнёра: агрегирует профиль, временные ссылки на документы, wallet/ledger/payouts и список бронирований.
- `chat-service` хранит conversation state в MongoDB, отдаёт SignalR-hub через gateway, использует `file-service` для вложений и публикует email-события о новых сообщениях в `RabbitMQ`.
- observability-стек (`Prometheus`, `Grafana`, `Loki`, `Tempo`, `OpenTelemetry Collector`, `Promtail`) подключён вместе с основным compose; сейчас edge metrics/traces дают `api-gateway`, backend metrics/traces реализованы в `ticket-service` и `identity-service`, а `Promtail` собирает JSON-логи `api-gateway`, `ticket-service`, `identity-service`, `car-service`, `booking-service`, `email-service` и `ai-search-service`.
- `identity-service` выдаёт JWT и публикует JWKS; остальные user-facing backend-сервисы валидируют пользовательские токены по публичному ключу.

## Основные пользовательские потоки

1. `Customer / Partner -> External Frontend -> API Gateway -> backend` для каталога, AI-подбора, бронирований, регистрации, жалоб, чатов и партнёрского кабинета.
2. `Manager -> Internal Frontend -> API Gateway -> Ticket Service` для очереди заявок, просмотра документов и approve/reject; после синхронных provisioning-вызовов `ticket-service` публикует события в `RabbitMQ`, которые подхватывают `email-service` и `car-service`.
3. `Superadmin -> Superadmin Frontend -> API Gateway -> Identity Service` для управления пользователями, ролями и permission inheritance.
4. `Customer -> External Frontend -> Booking Service -> Payment Service` для mock payment session (`start/submit`), после чего `booking-service` синхронизирует подтверждение, отмену, завершение брони, штрафы и payout-события в `payment-service`.
5. `Customer -> Booking Service -> AI Damage Eval Service -> Ticket Service -> Internal Frontend` для completion review: AI возвращает advisory-оценку, а менеджер принимает финальное решение.
6. `API Gateway / Ticket Service / Identity Service -> OpenTelemetry Collector / Prometheus / Promtail -> Tempo / Loki / Grafana` для трассировки, метрик и корреляции логов.
