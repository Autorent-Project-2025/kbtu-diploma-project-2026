CREATE EXTENSION IF NOT EXISTS btree_gist;

DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name = 'bookings'
          AND column_name = 'car_id'
    )
    AND NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name = 'bookings'
          AND column_name = 'partner_car_id'
    ) THEN
        ALTER TABLE public.bookings
            RENAME COLUMN car_id TO partner_car_id;
    END IF;
END $$;

DO $$
DECLARE
    current_type TEXT;
BEGIN
    SELECT data_type
      INTO current_type
      FROM information_schema.columns
     WHERE table_schema = 'public'
       AND table_name = 'bookings'
       AND column_name = 'user_id';

    IF current_type IS DISTINCT FROM 'uuid' THEN
        ALTER TABLE public.bookings
            ALTER COLUMN user_id TYPE UUID
            USING (
                CASE
                    WHEN user_id::text ~* '^[0-9a-f]{8}-[0-9a-f]{4}-[1-5][0-9a-f]{3}-[89ab][0-9a-f]{3}-[0-9a-f]{12}$'
                        THEN user_id::text::uuid
                    ELSE (
                        substr(md5(user_id::text), 1, 8) || '-' ||
                        substr(md5(user_id::text), 9, 4) || '-' ||
                        substr(md5(user_id::text), 13, 4) || '-' ||
                        substr(md5(user_id::text), 17, 4) || '-' ||
                        substr(md5(user_id::text), 21, 12)
                    )::uuid
                END
            );
    END IF;
END $$;

DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name = 'bookings'
          AND column_name = 'start_date'
    )
    AND NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name = 'bookings'
          AND column_name = 'start_time'
    ) THEN
        ALTER TABLE public.bookings
            RENAME COLUMN start_date TO start_time;
    END IF;

    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name = 'bookings'
          AND column_name = 'end_date'
    )
    AND NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name = 'bookings'
          AND column_name = 'end_time'
    ) THEN
        ALTER TABLE public.bookings
            RENAME COLUMN end_date TO end_time;
    END IF;
END $$;

DO $$
DECLARE
    start_type TEXT;
    end_type TEXT;
BEGIN
    SELECT data_type
      INTO start_type
      FROM information_schema.columns
     WHERE table_schema = 'public'
       AND table_name = 'bookings'
       AND column_name = 'start_time';

    SELECT data_type
      INTO end_type
      FROM information_schema.columns
     WHERE table_schema = 'public'
       AND table_name = 'bookings'
       AND column_name = 'end_time';

    IF start_type = 'timestamp without time zone' THEN
        ALTER TABLE public.bookings
            ALTER COLUMN start_time TYPE TIMESTAMPTZ
            USING start_time AT TIME ZONE 'UTC';
    END IF;

    IF end_type = 'timestamp without time zone' THEN
        ALTER TABLE public.bookings
            ALTER COLUMN end_time TYPE TIMESTAMPTZ
            USING end_time AT TIME ZONE 'UTC';
    END IF;
END $$;

ALTER TABLE public.bookings
    ADD COLUMN IF NOT EXISTS partner_id UUID;

UPDATE public.bookings
SET partner_id = '00000000-0000-0000-0000-000000000000'::uuid
WHERE partner_id IS NULL;

ALTER TABLE public.bookings
    ALTER COLUMN partner_id SET NOT NULL;

ALTER TABLE public.bookings
    ADD COLUMN IF NOT EXISTS booking_range TSTZRANGE;

UPDATE public.bookings
SET booking_range = tstzrange(start_time, end_time, '[)')
WHERE booking_range IS NULL
   OR lower(booking_range) IS DISTINCT FROM start_time
   OR upper(booking_range) IS DISTINCT FROM end_time;

ALTER TABLE public.bookings
    ALTER COLUMN booking_range SET NOT NULL;

CREATE OR REPLACE FUNCTION public.sync_booking_range_from_times()
RETURNS trigger
LANGUAGE plpgsql
AS $$
BEGIN
    NEW.booking_range := tstzrange(NEW.start_time, NEW.end_time, '[)');
    RETURN NEW;
END;
$$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_trigger
        WHERE tgname = 'trg_bookings_sync_booking_range'
          AND tgrelid = 'public.bookings'::regclass
    ) THEN
        CREATE TRIGGER trg_bookings_sync_booking_range
        BEFORE INSERT OR UPDATE OF start_time, end_time
        ON public.bookings
        FOR EACH ROW
        EXECUTE FUNCTION public.sync_booking_range_from_times();
    END IF;
END $$;

UPDATE public.bookings
SET status = lower(status)
WHERE status IS NOT NULL
  AND status <> lower(status);

DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM public.bookings b1
        JOIN public.bookings b2
          ON b1.id < b2.id
         AND b1.partner_car_id = b2.partner_car_id
         AND b1.booking_range && b2.booking_range
         AND lower(coalesce(b1.status, '')) IN ('pending', 'confirmed', 'active')
         AND lower(coalesce(b2.status, '')) IN ('pending', 'confirmed', 'active')
    ) THEN
        RAISE EXCEPTION
            'Cannot add prevent_overlapping_bookings: overlapping active bookings already exist. Resolve conflicts first.';
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conrelid = 'public.bookings'::regclass
          AND conname = 'prevent_overlapping_bookings'
    ) THEN
        ALTER TABLE public.bookings
            ADD CONSTRAINT prevent_overlapping_bookings
            EXCLUDE USING gist (
                partner_car_id WITH =,
                booking_range WITH &&
            )
            WHERE (status IN ('pending', 'confirmed', 'active'));
    END IF;
END $$;

ALTER TABLE public.bookings
    ADD COLUMN IF NOT EXISTS price_hour NUMERIC(10, 2),
    ADD COLUMN IF NOT EXISTS total_price NUMERIC(10, 2),
    ADD COLUMN IF NOT EXISTS created_at TIMESTAMPTZ NOT NULL DEFAULT NOW();

UPDATE public.bookings
SET total_price = price
WHERE total_price IS NULL
  AND price IS NOT NULL;

UPDATE public.bookings
SET created_at = start_time
WHERE created_at IS NULL;

CREATE INDEX IF NOT EXISTS idx_booking_car_time
    ON public.bookings (partner_car_id, start_time, end_time);

CREATE INDEX IF NOT EXISTS idx_booking_user
    ON public.bookings (user_id);

CREATE INDEX IF NOT EXISTS idx_booking_partner
    ON public.bookings (partner_id);

