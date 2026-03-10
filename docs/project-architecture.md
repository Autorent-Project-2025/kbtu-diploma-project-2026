# AutoRent Project Architecture

Диаграмма ниже показывает runtime-контур проекта: 3 frontend-приложения, 11 backend-сервисов, их основные service-to-service вызовы, базы данных и внешние провайдеры. Миграционные контейнеры `*-flyway` намеренно опущены, чтобы не перегружать схему.

```mermaid
flowchart LR
  classDef actor fill:#FFFFFF,stroke:#5F6368,color:#202124;
  classDef frontend fill:#E8F0FE,stroke:#1A73E8,color:#0B1F33;
  classDef edge fill:#FFF4D6,stroke:#C47F00,color:#4A2E00;
  classDef shared fill:#E6F4EA,stroke:#1E8E5A,color:#123524;
  classDef external fill:#FDEBD2,stroke:#D97706,color:#4D2E00;
  classDef internal fill:#FCE8E6,stroke:#C53929,color:#4A1C16;
  classDef data fill:#F8F9FA,stroke:#5F6368,color:#202124;
  classDef storage fill:#EEF3F7,stroke:#607D8B,color:#23343B;
  classDef provider fill:#FFF7E0,stroke:#B26A00,color:#4A2E00;

  subgraph Actors["Actors"]
    direction TB
    CUSTOMER["Customer / Partner"]:::actor
    MANAGER["Manager"]:::actor
    SUPERADMIN["Superadmin"]:::actor
  end

  subgraph Frontend["Frontend Apps"]
    direction TB
    FE_EXT["External Frontend<br/>Vue 3 + Vite<br/>:5173"]:::frontend
    FE_INT["Internal Frontend<br/>Vue 3 + Vite<br/>:5174"]:::frontend
    FE_SUPER["Superadmin Frontend<br/>Vue 3 + Vite<br/>:5175"]:::frontend
  end

  subgraph Edge["Edge"]
    GATEWAY["API Gateway<br/>Node.js + Express<br/>:9186"]:::edge
  end

  subgraph Backend["Backend Services"]
    direction LR

    subgraph Shared["Shared"]
      direction TB
      IDENTITY["Identity Service<br/>auth, users, roles<br/>:1244"]:::shared
      EMAIL["Email Service<br/>SMTP notifications<br/>:9182"]:::shared
      IMAGE["Image Service<br/>image storage<br/>:9181"]:::shared
    end

    subgraph External["External"]
      direction TB
      CAR["Car Service<br/>catalog + matching<br/>:1298"]:::external
      BOOKING["Booking Service<br/>bookings + payment flow<br/>:1821"]:::external
      CLIENT["Client Service<br/>client profiles<br/>:1831"]:::external
    end

    subgraph Internal["Internal"]
      direction TB
      PARTNER["Partner Service<br/>partner cabinet facade<br/>:1832"]:::internal
      TICKET["Ticket Service<br/>registration + approvals<br/>:1248"]:::internal
      PAYMENT["Payment Service<br/>wallet, ledger, payouts<br/>:1834"]:::internal
      FILE["File Service<br/>private documents<br/>:9183"]:::internal
    end
  end

  subgraph Data["Data Stores / Providers"]
    direction TB
    ID_DB[("identity-db<br/>PostgreSQL")]:::data
    CAR_DB[("car-db<br/>PostgreSQL")]:::data
    BOOKING_DB[("booking-db<br/>PostgreSQL")]:::data
    CLIENT_DB[("client-db<br/>PostgreSQL")]:::data
    PARTNER_DB[("partner-db<br/>PostgreSQL")]:::data
    TICKET_DB[("ticket-db<br/>PostgreSQL")]:::data
    PAYMENT_DB[("payment-db<br/>PostgreSQL")]:::data
    FILE_STORE[("file_uploads / Google Cloud Storage")]:::storage
    IMAGE_STORE[("image_uploads / Google Cloud Storage")]:::storage
    SMTP["SMTP provider"]:::provider
  end

  CUSTOMER --> FE_EXT
  MANAGER --> FE_INT
  SUPERADMIN --> FE_SUPER

  FE_EXT -->|REST| GATEWAY
  FE_INT -->|REST| GATEWAY
  FE_SUPER -->|REST| GATEWAY

  GATEWAY -->|/identity/*| IDENTITY
  GATEWAY -->|/cars/*| CAR
  GATEWAY -->|/bookings/*| BOOKING
  GATEWAY -->|/clients/*| CLIENT
  GATEWAY -->|/partners/*| PARTNER
  GATEWAY -->|/tickets/*| TICKET
  GATEWAY -->|/files/*| FILE
  GATEWAY -->|/internal/*| IMAGE

  TICKET -->|user provisioning| IDENTITY
  TICKET -->|client provisioning| CLIENT
  TICKET -->|partner provisioning<br/>and /me for PartnerCar| PARTNER
  TICKET -->|document upload<br/>and temp links| FILE
  TICKET -->|PartnerCar images| IMAGE
  TICKET -->|approved PartnerCar provisioning| CAR
  TICKET -->|approve / reject emails| EMAIL

  CAR -->|partner context for /my| PARTNER
  CAR -->|counts, linked bookings,<br/>availability checks| BOOKING
  CAR -->|car model / partner car images| IMAGE

  BOOKING -->|partner car snapshot| CAR
  BOOKING -->|mock payments<br/>and booking sync outbox| PAYMENT

  PARTNER -->|temporary file links| FILE
  PARTNER -->|wallet, ledger, payouts| PAYMENT
  PARTNER -->|partner bookings| BOOKING

  IDENTITY -.->|JWT / JWKS for auth| CAR
  IDENTITY -.->|JWT / JWKS for auth| BOOKING
  IDENTITY -.->|JWT / JWKS for auth| CLIENT
  IDENTITY -.->|JWT / JWKS for auth| PARTNER
  IDENTITY -.->|JWT / JWKS for auth| TICKET
  IDENTITY -.->|JWT / JWKS for auth| FILE
  IDENTITY -.->|JWT / JWKS for auth| IMAGE

  IDENTITY --> ID_DB
  CAR --> CAR_DB
  BOOKING --> BOOKING_DB
  CLIENT --> CLIENT_DB
  PARTNER --> PARTNER_DB
  TICKET --> TICKET_DB
  PAYMENT --> PAYMENT_DB
  FILE --> FILE_STORE
  IMAGE --> IMAGE_STORE
  EMAIL --> SMTP
```

## Ключевые контуры

- `api-gateway` - единая входная точка для всех frontend-приложений; `payment-service` остаётся только внутренним сервисом и наружу не публикуется.
- `ticket-service` - главный orchestrator процесса onboarding и согласования: создаёт пользователей/профили, складывает документы, инициирует создание `partner_car` и отправляет email-уведомления.
- `car-service` и `booking-service` образуют контур подбора и доступности машин: каталог и ранжирование живут в `car-service`, фактическая занятость и статусы бронирований - в `booking-service`.
- `partner-service` выступает как фасад кабинета партнёра: агрегирует профиль, временные ссылки на документы, wallet/ledger/payouts и список бронирований.
- `identity-service` выдаёт JWT и публикует JWKS; остальные user-facing backend-сервисы валидируют пользовательские токены по публичному ключу.

## Основные пользовательские потоки

1. `Customer / Partner -> External Frontend -> API Gateway -> backend` для каталога, бронирований, регистрации и партнёрского кабинета.
2. `Manager -> Internal Frontend -> API Gateway -> Ticket Service` для очереди заявок, просмотра документов и approve/reject.
3. `Superadmin -> Superadmin Frontend -> API Gateway -> Identity Service` для управления пользователями, ролями и permission inheritance.
4. `Booking Service -> Payment Service` для mock-платежей и финансовой синхронизации статусов бронирования.
