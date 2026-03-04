CREATE TABLE IF NOT EXISTS public.brands (
    id SERIAL PRIMARY KEY,
    name VARCHAR(255) NOT NULL
);

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conrelid = 'public.brands'::regclass
          AND conname = 'ux_brands_name'
    ) THEN
        ALTER TABLE public.brands
            ADD CONSTRAINT ux_brands_name UNIQUE (name);
    END IF;
END $$;

CREATE TABLE IF NOT EXISTS public.models (
    id SERIAL PRIMARY KEY,
    brand_id INT NOT NULL,
    name VARCHAR(255) NOT NULL
);

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conrelid = 'public.models'::regclass
          AND conname = 'fk_models_brand'
    ) THEN
        ALTER TABLE public.models
            ADD CONSTRAINT fk_models_brand
            FOREIGN KEY (brand_id) REFERENCES public.brands(id);
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conrelid = 'public.models'::regclass
          AND conname = 'ux_models_brand_name'
    ) THEN
        ALTER TABLE public.models
            ADD CONSTRAINT ux_models_brand_name UNIQUE (brand_id, name);
    END IF;
END $$;

ALTER TABLE public.car_models
    ADD COLUMN IF NOT EXISTS brand_id INT,
    ADD COLUMN IF NOT EXISTS model_id INT;

DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name = 'car_models'
          AND column_name = 'brand'
    ) THEN
        INSERT INTO public.brands (name)
        SELECT DISTINCT TRIM(car_model.brand)
        FROM public.car_models car_model
        WHERE car_model.brand IS NOT NULL
          AND LENGTH(TRIM(car_model.brand)) > 0
        ON CONFLICT (name) DO NOTHING;
    END IF;
END $$;

DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name = 'car_models'
          AND column_name = 'brand'
    ) AND EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name = 'car_models'
          AND column_name = 'model'
    ) THEN
        INSERT INTO public.models (brand_id, name)
        SELECT DISTINCT brand.id, TRIM(car_model.model)
        FROM public.car_models car_model
        JOIN public.brands brand
            ON LOWER(brand.name) = LOWER(TRIM(car_model.brand))
        WHERE car_model.model IS NOT NULL
          AND LENGTH(TRIM(car_model.model)) > 0
        ON CONFLICT (brand_id, name) DO NOTHING;

        UPDATE public.car_models car_model
        SET brand_id = brand.id
        FROM public.brands brand
        WHERE car_model.brand_id IS NULL
          AND LOWER(brand.name) = LOWER(TRIM(car_model.brand));

        UPDATE public.car_models car_model
        SET model_id = model_lookup.id
        FROM public.models model_lookup
        JOIN public.brands brand
            ON brand.id = model_lookup.brand_id
        WHERE car_model.model_id IS NULL
          AND LOWER(brand.name) = LOWER(TRIM(car_model.brand))
          AND LOWER(model_lookup.name) = LOWER(TRIM(car_model.model));
    END IF;
END $$;

INSERT INTO public.brands (name)
VALUES ('Unknown')
ON CONFLICT (name) DO NOTHING;

INSERT INTO public.models (brand_id, name)
SELECT brand.id, 'Unknown'
FROM public.brands brand
WHERE brand.name = 'Unknown'
ON CONFLICT (brand_id, name) DO NOTHING;

UPDATE public.car_models car_model
SET brand_id = brand.id
FROM public.brands brand
WHERE car_model.brand_id IS NULL
  AND brand.name = 'Unknown';

UPDATE public.car_models car_model
SET model_id = model_lookup.id
FROM public.models model_lookup
JOIN public.brands brand
    ON brand.id = model_lookup.brand_id
WHERE car_model.model_id IS NULL
  AND brand.name = 'Unknown'
  AND model_lookup.name = 'Unknown';

ALTER TABLE public.car_models
    ALTER COLUMN brand_id SET NOT NULL,
    ALTER COLUMN model_id SET NOT NULL;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conrelid = 'public.car_models'::regclass
          AND conname = 'fk_car_models_brand'
    ) THEN
        ALTER TABLE public.car_models
            ADD CONSTRAINT fk_car_models_brand
            FOREIGN KEY (brand_id) REFERENCES public.brands(id);
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conrelid = 'public.car_models'::regclass
          AND conname = 'fk_car_models_model'
    ) THEN
        ALTER TABLE public.car_models
            ADD CONSTRAINT fk_car_models_model
            FOREIGN KEY (model_id) REFERENCES public.models(id);
    END IF;
END $$;

DROP INDEX IF EXISTS public.ux_car_models_brand_model_year;

CREATE UNIQUE INDEX IF NOT EXISTS ux_car_models_brand_model_year
    ON public.car_models (brand_id, model_id, year);

CREATE INDEX IF NOT EXISTS idx_car_models_brand_id
    ON public.car_models (brand_id);

CREATE INDEX IF NOT EXISTS idx_car_models_model_id
    ON public.car_models (model_id);

DO $$
DECLARE
    cars_relkind "char";
BEGIN
    IF to_regclass('public.cars') IS NOT NULL THEN
        SELECT pg_class.relkind
        INTO cars_relkind
        FROM pg_class
        WHERE pg_class.oid = 'public.cars'::regclass;

        IF cars_relkind = 'v' THEN
            EXECUTE 'DROP VIEW public.cars';
        END IF;
    END IF;
END $$;

ALTER TABLE public.car_models
    DROP COLUMN IF EXISTS brand,
    DROP COLUMN IF EXISTS model;

DO $$
BEGIN
    IF to_regclass('public.cars') IS NULL
       AND to_regclass('public.car_models') IS NOT NULL THEN
        EXECUTE 'CREATE VIEW public.cars AS SELECT * FROM public.car_models';
    END IF;
END $$;
