DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name = 'partner_cars'
          AND column_name = 'partner_id'
    )
    AND NOT EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name = 'partner_cars'
          AND column_name = 'partner_user_id'
    ) THEN
        ALTER TABLE public.partner_cars
            RENAME COLUMN partner_id TO partner_user_id;
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS idx_partner_car_partner_user
    ON public.partner_cars (partner_user_id);
