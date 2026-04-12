ALTER TABLE public.bookings
    ADD COLUMN IF NOT EXISTS partner_cancellation_ticket_id UUID NULL,
    ADD COLUMN IF NOT EXISTS partner_cancellation_requested_at TIMESTAMPTZ NULL;
