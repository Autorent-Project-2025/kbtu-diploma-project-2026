DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name = 'bookings'
          AND column_name = 'partner_id'
    )
    AND NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name = 'bookings'
          AND column_name = 'partner_user_id'
    ) THEN
        ALTER TABLE public.bookings
            RENAME COLUMN partner_id TO partner_user_id;
    END IF;
END $$;

DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM pg_indexes
        WHERE schemaname = 'public'
          AND tablename = 'bookings'
          AND indexname = 'idx_booking_partner'
    )
    AND NOT EXISTS (
        SELECT 1
        FROM pg_indexes
        WHERE schemaname = 'public'
          AND tablename = 'bookings'
          AND indexname = 'idx_booking_partner_user'
    ) THEN
        ALTER INDEX public.idx_booking_partner
            RENAME TO idx_booking_partner_user;
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS idx_booking_partner_user
    ON public.bookings (partner_user_id);
