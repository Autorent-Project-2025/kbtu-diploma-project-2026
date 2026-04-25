# Payment Service

## Назначение
Внутренний сервис финансового учета партнера. Отвечает за:
- кошелек партнера (`partner_wallets`);
- бухгалтерский журнал движений (`partner_ledger_entries`);
- клиентские оплаты по бронированиям (`customer_payments`);
- выплаты партнеру (`partner_payouts`).

Сейчас сервис используется `booking-service` для синхронизации статусов бронирования:
- `Confirmed` -> деньги партнера попадают в `pending`;
- `Canceled` -> pending-зачисление сторнируется;
- `Completed` -> сумма переводится из `pending` в `available`.
- damage/fine charge -> создается начисление, которое может быть оплачено или списано.

## API
Нативный base path сервиса: `/`.

### Internal API
Внутренние маршруты используют `X-Internal-Api-Key`.

- `POST /internal/payments/bookings/confirm`
- `POST /internal/payments/bookings/cancel`
- `POST /internal/payments/bookings/complete`
- `POST /internal/mock-payments/start`
- `GET /internal/mock-payments/by-booking/{bookingId}`
- `POST /internal/mock-payments/{bookingId}/submit`
- `POST /internal/payments/booking-charges`
- `POST /internal/payments/booking-charges/{chargeId}/paid`
- `POST /internal/payments/booking-charges/{chargeId}/cancel`
- `POST /internal/payments/booking-charges/{chargeId}/refund`
- `GET /internal/payments/bookings/{bookingId}/charges`
- `GET /internal/payments/users/{userId}/booking-charges`
- `GET /internal/payments/wallets/{partnerUserId}`
- `GET /internal/payments/ledger/{partnerUserId}?take=50`
- `POST /internal/payments/payouts/request`
- `POST /internal/payments/payouts/{payoutId}/processing|paid|failed|cancel`
- `GET /internal/payments/payouts/{payoutId}`
- `GET /internal/payments/payouts/by-partner/{partnerUserId}`

### View API
Read-only маршруты доступны через gateway как `/payments/view/*` и требуют JWT permission `Payment.View`.

- `GET /view/bookings/{bookingId}/charges`

Во frontend это используется как:

```text
GET /payments/view/bookings/{bookingId}/charges
```

## ERM Диаграмма

```mermaid
erDiagram
  PARTNER_WALLETS {
    bigint id PK
    uuid partner_user_id UK
    string currency
    decimal pending_amount
    decimal available_amount
    decimal reserved_amount
    timestamptz created_at
    timestamptz updated_at
  }

  CUSTOMER_PAYMENTS {
    bigint id PK
    int booking_id UK
    uuid user_id
    uuid partner_user_id
    int partner_car_id
    decimal price_hour
    decimal gross_amount
    decimal platform_commission_rate
    decimal platform_commission_amount
    decimal partner_amount
    string currency
    string status
    timestamptz created_at
    timestamptz updated_at
    timestamptz confirmed_at
    timestamptz available_at
    timestamptz canceled_at
  }

  PARTNER_PAYOUTS {
    bigint id PK
    uuid partner_user_id
    string request_key UK
    decimal amount
    string currency
    string status
    timestamptz requested_at
    timestamptz processed_at
    string failure_reason
  }

  PARTNER_LEDGER_ENTRIES {
    bigint id PK
    bigint partner_wallet_id FK
    int booking_id
    bigint customer_payment_id FK
    bigint partner_payout_id FK
    string entry_type
    string bucket
    decimal amount_delta
    string currency
    string description
    timestamptz created_at
  }

  MOCK_PAYMENT_ATTEMPTS {
    bigint id PK
    int booking_id
    uuid user_id
    string session_key UK
    decimal amount
    string currency
    string status
    string card_holder
    string card_last4
    string failure_reason
    timestamptz created_at
    timestamptz updated_at
    timestamptz completed_at
    timestamptz expires_at
  }

  BOOKING_CHARGES {
    bigint id PK
    int booking_id
    uuid user_id
    uuid partner_user_id
    string charge_type
    decimal amount
    decimal partner_share_amount
    string currency
    string status
    string description
    timestamptz created_at
    timestamptz updated_at
    timestamptz paid_at
    timestamptz canceled_at
    timestamptz refunded_at
  }

  PROCESSED_INTEGRATION_EVENTS {
    bigint id PK
    string event_id UK
    string routing_key
    timestamptz processed_at
  }

  BOOKINGS {
    int id PK
  }

  IDENTITY_USERS {
    uuid id PK
  }

  PARTNER_WALLETS ||--o{ PARTNER_LEDGER_ENTRIES : records
  CUSTOMER_PAYMENTS |o--o{ PARTNER_LEDGER_ENTRIES : source
  PARTNER_PAYOUTS |o--o{ PARTNER_LEDGER_ENTRIES : source
  BOOKINGS ||--o| CUSTOMER_PAYMENTS : payment
  BOOKINGS ||--o{ MOCK_PAYMENT_ATTEMPTS : attempts
  BOOKINGS ||--o{ BOOKING_CHARGES : charges
  IDENTITY_USERS ||--o{ PARTNER_WALLETS : partner
  IDENTITY_USERS ||--o{ CUSTOMER_PAYMENTS : customer
  IDENTITY_USERS ||--o{ CUSTOMER_PAYMENTS : partner
  IDENTITY_USERS ||--o{ BOOKING_CHARGES : customer
  IDENTITY_USERS ||--o{ BOOKING_CHARGES : partner
  IDENTITY_USERS ||--o{ PARTNER_PAYOUTS : partner
```
