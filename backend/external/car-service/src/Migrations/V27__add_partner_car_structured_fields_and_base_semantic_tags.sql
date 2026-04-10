ALTER TABLE public.car_models
    ADD COLUMN IF NOT EXISTS body_type VARCHAR(50),
    ADD COLUMN IF NOT EXISTS horsepower INT;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conrelid = 'public.car_models'::regclass
          AND conname = 'chk_car_models_horsepower'
    ) THEN
        ALTER TABLE public.car_models
            ADD CONSTRAINT chk_car_models_horsepower
            CHECK (horsepower IS NULL OR (horsepower > 0 AND horsepower <= 3000));
    END IF;
END $$;

INSERT INTO public.features (name)
SELECT seed.name
FROM (
    VALUES
        ('econom'),
        ('comfort'),
        ('business'),
        ('sport'),
        ('suv'),
        ('electric'),
        ('family')
) AS seed(name)
WHERE NOT EXISTS (
    SELECT 1
    FROM public.features feature
    WHERE LOWER(feature.name) = LOWER(seed.name)
);
