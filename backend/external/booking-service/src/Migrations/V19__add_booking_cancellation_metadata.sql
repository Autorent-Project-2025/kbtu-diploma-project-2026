ALTER TABLE public.bookings
    ADD COLUMN IF NOT EXISTS cancellation_actor VARCHAR(32) NULL,
    ADD COLUMN IF NOT EXISTS cancellation_reason VARCHAR(2000) NULL;
