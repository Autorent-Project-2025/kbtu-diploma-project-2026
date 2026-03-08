CREATE TABLE IF NOT EXISTS public.partner_wallets (
    id BIGSERIAL PRIMARY KEY,
    partner_user_id UUID NOT NULL,
    currency VARCHAR(3) NOT NULL,
    pending_amount NUMERIC(18, 2) NOT NULL DEFAULT 0,
    available_amount NUMERIC(18, 2) NOT NULL DEFAULT 0,
    reserved_amount NUMERIC(18, 2) NOT NULL DEFAULT 0,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT ux_partner_wallets_partner_user_id UNIQUE (partner_user_id),
    CONSTRAINT chk_partner_wallets_pending_amount CHECK (pending_amount >= 0),
    CONSTRAINT chk_partner_wallets_available_amount CHECK (available_amount >= 0),
    CONSTRAINT chk_partner_wallets_reserved_amount CHECK (reserved_amount >= 0)
);

CREATE TABLE IF NOT EXISTS public.customer_payments (
    id BIGSERIAL PRIMARY KEY,
    booking_id INT NOT NULL,
    user_id UUID NOT NULL,
    partner_user_id UUID NOT NULL,
    partner_car_id INT NOT NULL,
    price_hour NUMERIC(18, 2),
    gross_amount NUMERIC(18, 2) NOT NULL,
    platform_commission_rate NUMERIC(5, 4) NOT NULL,
    platform_commission_amount NUMERIC(18, 2) NOT NULL,
    partner_amount NUMERIC(18, 2) NOT NULL,
    currency VARCHAR(3) NOT NULL,
    status VARCHAR(32) NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    confirmed_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    available_at TIMESTAMPTZ NULL,
    canceled_at TIMESTAMPTZ NULL,
    CONSTRAINT ux_customer_payments_booking_id UNIQUE (booking_id),
    CONSTRAINT chk_customer_payments_gross_amount CHECK (gross_amount >= 0),
    CONSTRAINT chk_customer_payments_platform_commission_rate CHECK (platform_commission_rate >= 0 AND platform_commission_rate <= 1),
    CONSTRAINT chk_customer_payments_platform_commission_amount CHECK (platform_commission_amount >= 0),
    CONSTRAINT chk_customer_payments_partner_amount CHECK (partner_amount >= 0)
);

CREATE TABLE IF NOT EXISTS public.partner_payouts (
    id BIGSERIAL PRIMARY KEY,
    partner_user_id UUID NOT NULL,
    amount NUMERIC(18, 2) NOT NULL,
    currency VARCHAR(3) NOT NULL,
    status VARCHAR(32) NOT NULL,
    requested_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    processed_at TIMESTAMPTZ NULL,
    failure_reason TEXT NULL,
    CONSTRAINT chk_partner_payouts_amount CHECK (amount >= 0)
);

CREATE TABLE IF NOT EXISTS public.partner_ledger_entries (
    id BIGSERIAL PRIMARY KEY,
    partner_wallet_id BIGINT NOT NULL,
    booking_id INT NULL,
    customer_payment_id BIGINT NULL,
    partner_payout_id BIGINT NULL,
    entry_type VARCHAR(64) NOT NULL,
    bucket VARCHAR(32) NOT NULL,
    amount_delta NUMERIC(18, 2) NOT NULL,
    currency VARCHAR(3) NOT NULL,
    description VARCHAR(255) NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    CONSTRAINT fk_partner_ledger_entries_wallet
        FOREIGN KEY (partner_wallet_id) REFERENCES public.partner_wallets(id) ON DELETE CASCADE,
    CONSTRAINT fk_partner_ledger_entries_customer_payment
        FOREIGN KEY (customer_payment_id) REFERENCES public.customer_payments(id) ON DELETE SET NULL,
    CONSTRAINT fk_partner_ledger_entries_partner_payout
        FOREIGN KEY (partner_payout_id) REFERENCES public.partner_payouts(id) ON DELETE SET NULL
);

CREATE INDEX IF NOT EXISTS idx_partner_wallets_currency
    ON public.partner_wallets (currency);

CREATE INDEX IF NOT EXISTS idx_customer_payments_partner_user_id
    ON public.customer_payments (partner_user_id);

CREATE INDEX IF NOT EXISTS idx_customer_payments_status
    ON public.customer_payments (status);

CREATE INDEX IF NOT EXISTS idx_partner_payouts_partner_user_id
    ON public.partner_payouts (partner_user_id);

CREATE INDEX IF NOT EXISTS idx_partner_payouts_status
    ON public.partner_payouts (status);

CREATE INDEX IF NOT EXISTS idx_partner_ledger_entries_wallet_id
    ON public.partner_ledger_entries (partner_wallet_id, created_at DESC);

CREATE INDEX IF NOT EXISTS idx_partner_ledger_entries_booking_id
    ON public.partner_ledger_entries (booking_id);
