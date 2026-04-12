ALTER TABLE public.bookings
    ADD COLUMN IF NOT EXISTS car_comment_id INT NULL,
    ADD COLUMN IF NOT EXISTS car_comment_submitted_at TIMESTAMPTZ NULL;
