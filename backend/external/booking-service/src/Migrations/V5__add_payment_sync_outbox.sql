CREATE TABLE IF NOT EXISTS public.payment_sync_outbox_messages (
    id BIGSERIAL PRIMARY KEY,
    booking_id INT NOT NULL,
    event_key VARCHAR(200) NOT NULL,
    event_type VARCHAR(100) NOT NULL,
    payload JSONB NOT NULL,
    attempt_count INT NOT NULL DEFAULT 0,
    last_error TEXT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    next_attempt_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    processed_at TIMESTAMPTZ NULL,
    locked_until TIMESTAMPTZ NULL,
    CONSTRAINT ux_payment_sync_outbox_messages_event_key UNIQUE (event_key),
    CONSTRAINT chk_payment_sync_outbox_messages_attempt_count CHECK (attempt_count >= 0)
);

CREATE INDEX IF NOT EXISTS idx_payment_sync_outbox_dispatch
    ON public.payment_sync_outbox_messages (processed_at, next_attempt_at, id);

CREATE INDEX IF NOT EXISTS idx_payment_sync_outbox_booking_id
    ON public.payment_sync_outbox_messages (booking_id);
