CREATE TABLE IF NOT EXISTS public.booking_charges (
    id BIGSERIAL PRIMARY KEY,
    booking_id INT NOT NULL,
    user_id UUID NOT NULL,
    partner_user_id UUID NOT NULL,
    charge_type VARCHAR(32) NOT NULL,
    amount NUMERIC(18, 2) NOT NULL,
    partner_share_amount NUMERIC(18, 2) NOT NULL,
    currency VARCHAR(3) NOT NULL,
    status VARCHAR(32) NOT NULL,
    description VARCHAR(255) NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    updated_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    paid_at TIMESTAMPTZ NULL,
    canceled_at TIMESTAMPTZ NULL,
    CONSTRAINT chk_booking_charges_amount CHECK (amount >= 0),
    CONSTRAINT chk_booking_charges_partner_share_amount CHECK (partner_share_amount >= 0)
);

CREATE INDEX IF NOT EXISTS idx_booking_charges_booking_id
    ON public.booking_charges (booking_id);

CREATE INDEX IF NOT EXISTS idx_booking_charges_user_id
    ON public.booking_charges (user_id);

CREATE INDEX IF NOT EXISTS idx_booking_charges_partner_user_id
    ON public.booking_charges (partner_user_id);

CREATE INDEX IF NOT EXISTS idx_booking_charges_status
    ON public.booking_charges (status);
