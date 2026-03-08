CREATE TABLE IF NOT EXISTS public.mock_payment_attempts (
    id BIGSERIAL PRIMARY KEY,
    booking_id INT NOT NULL,
    user_id UUID NOT NULL,
    session_key VARCHAR(64) NOT NULL,
    amount NUMERIC(18, 2) NOT NULL,
    currency VARCHAR(3) NOT NULL,
    status VARCHAR(32) NOT NULL,
    card_holder VARCHAR(128) NULL,
    card_last4 VARCHAR(4) NULL,
    failure_reason TEXT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    completed_at TIMESTAMPTZ NULL,
    expires_at TIMESTAMPTZ NOT NULL,
    CONSTRAINT chk_mock_payment_attempts_amount CHECK (amount > 0),
    CONSTRAINT ux_mock_payment_attempts_session_key UNIQUE (session_key)
);

CREATE INDEX IF NOT EXISTS idx_mock_payment_attempts_booking_user_created_at
    ON public.mock_payment_attempts (booking_id, user_id, created_at DESC);
