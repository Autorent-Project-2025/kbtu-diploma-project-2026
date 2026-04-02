ALTER TABLE public.bookings
    ADD COLUMN IF NOT EXISTS trip_started_at TIMESTAMPTZ NULL,
    ADD COLUMN IF NOT EXISTS trip_completed_at TIMESTAMPTZ NULL,
    ADD COLUMN IF NOT EXISTS completion_review_ticket_id UUID NULL;
