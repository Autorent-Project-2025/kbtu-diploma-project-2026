ALTER TABLE public.car_comments
    ADD COLUMN IF NOT EXISTS booking_id INT NULL;

CREATE UNIQUE INDEX IF NOT EXISTS ux_car_comments_booking_id
    ON public.car_comments (booking_id)
    WHERE booking_id IS NOT NULL;
