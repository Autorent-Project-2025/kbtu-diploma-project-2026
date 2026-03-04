DO $$
BEGIN
    IF to_regclass('public.car_models') IS NULL
       AND to_regclass('public.cars') IS NOT NULL THEN
        ALTER TABLE public.cars RENAME TO car_models;
    END IF;
END $$;

CREATE TABLE IF NOT EXISTS public.car_models (
    id SERIAL PRIMARY KEY,
    brand VARCHAR(255) NOT NULL,
    model VARCHAR(255) NOT NULL,
    year INT NOT NULL,
    engine VARCHAR(100),
    transmission VARCHAR(100),
    seats INT CHECK (seats > 0 AND seats <= 20),
    fuel_type VARCHAR(50),
    doors INT CHECK (doors > 0 AND doors <= 6),
    description VARCHAR(585),
    rating NUMERIC(2, 1) NULL CHECK (rating >= 1 AND rating <= 5),
    ratings_count INT NOT NULL DEFAULT 0
);

ALTER TABLE public.car_models
    ADD COLUMN IF NOT EXISTS brand VARCHAR(255),
    ADD COLUMN IF NOT EXISTS model VARCHAR(255),
    ADD COLUMN IF NOT EXISTS year INT,
    ADD COLUMN IF NOT EXISTS engine VARCHAR(100),
    ADD COLUMN IF NOT EXISTS transmission VARCHAR(100),
    ADD COLUMN IF NOT EXISTS seats INT,
    ADD COLUMN IF NOT EXISTS fuel_type VARCHAR(50),
    ADD COLUMN IF NOT EXISTS doors INT,
    ADD COLUMN IF NOT EXISTS description VARCHAR(585),
    ADD COLUMN IF NOT EXISTS rating NUMERIC(2, 1),
    ADD COLUMN IF NOT EXISTS ratings_count INT NOT NULL DEFAULT 0;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conrelid = 'public.car_models'::regclass
          AND conname = 'chk_car_models_rating'
    ) THEN
        ALTER TABLE public.car_models
            ADD CONSTRAINT chk_car_models_rating
            CHECK (rating IS NULL OR (rating >= 1 AND rating <= 5));
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conrelid = 'public.car_models'::regclass
          AND conname = 'chk_car_models_seats'
    ) THEN
        ALTER TABLE public.car_models
            ADD CONSTRAINT chk_car_models_seats
            CHECK (seats IS NULL OR (seats > 0 AND seats <= 20));
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conrelid = 'public.car_models'::regclass
          AND conname = 'chk_car_models_doors'
    ) THEN
        ALTER TABLE public.car_models
            ADD CONSTRAINT chk_car_models_doors
            CHECK (doors IS NULL OR (doors > 0 AND doors <= 6));
    END IF;
END $$;

ALTER TABLE public.car_models
    ALTER COLUMN brand SET NOT NULL,
    ALTER COLUMN model SET NOT NULL,
    ALTER COLUMN year SET NOT NULL,
    ALTER COLUMN ratings_count SET NOT NULL;

CREATE UNIQUE INDEX IF NOT EXISTS ux_car_models_brand_model_year
    ON public.car_models (brand, model, year);

DO $$
BEGIN
    IF to_regclass('public.cars') IS NULL
       AND to_regclass('public.car_models') IS NOT NULL THEN
        EXECUTE 'CREATE VIEW public.cars AS SELECT * FROM public.car_models';
    END IF;
END $$;

CREATE TABLE IF NOT EXISTS public.partner_cars (
    id SERIAL PRIMARY KEY,
    partner_id UUID NOT NULL,
    car_model_id INT NOT NULL,
    license_plate VARCHAR(20) NOT NULL,
    color VARCHAR(50),
    price_hour NUMERIC(10, 2),
    price_day NUMERIC(10, 2),
    status INT NOT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    rating NUMERIC(2, 1) NULL,
    ratings_count INT NOT NULL DEFAULT 0
);

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conrelid = 'public.partner_cars'::regclass
          AND conname = 'fk_partner_cars_car_model'
    ) THEN
        ALTER TABLE public.partner_cars
            ADD CONSTRAINT fk_partner_cars_car_model
            FOREIGN KEY (car_model_id) REFERENCES public.car_models(id);
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conrelid = 'public.partner_cars'::regclass
          AND conname = 'ux_partner_cars_license_plate'
    ) THEN
        ALTER TABLE public.partner_cars
            ADD CONSTRAINT ux_partner_cars_license_plate
            UNIQUE (license_plate);
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conrelid = 'public.partner_cars'::regclass
          AND conname = 'chk_partner_cars_status'
    ) THEN
        ALTER TABLE public.partner_cars
            ADD CONSTRAINT chk_partner_cars_status
            CHECK (status IN (0, 1, 2, 3));
    END IF;

    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conrelid = 'public.partner_cars'::regclass
          AND conname = 'chk_partner_cars_rating'
    ) THEN
        ALTER TABLE public.partner_cars
            ADD CONSTRAINT chk_partner_cars_rating
            CHECK (rating IS NULL OR (rating >= 1 AND rating <= 5));
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS idx_partner_car_model
    ON public.partner_cars (car_model_id);

CREATE INDEX IF NOT EXISTS idx_partner_car_status
    ON public.partner_cars (status);

CREATE INDEX IF NOT EXISTS idx_partner_car_model_status
    ON public.partner_cars (car_model_id, status);

CREATE TABLE IF NOT EXISTS public.car_model_images (
    id SERIAL PRIMARY KEY,
    model_id INT NOT NULL,
    image_url TEXT NOT NULL,
    image_type INT NOT NULL,
    display_order INT NOT NULL,
    CONSTRAINT fk_car_model_images_model
        FOREIGN KEY (model_id) REFERENCES public.car_models(id) ON DELETE CASCADE
);

CREATE TABLE IF NOT EXISTS public.partner_car_images (
    id SERIAL PRIMARY KEY,
    car_id INT NOT NULL,
    image_url TEXT NOT NULL,
    image_type INT NOT NULL,
    display_order INT NOT NULL,
    CONSTRAINT fk_partner_car_images_car
        FOREIGN KEY (car_id) REFERENCES public.partner_cars(id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_car_model_images_model
    ON public.car_model_images (model_id);

CREATE INDEX IF NOT EXISTS idx_partner_car_images_car
    ON public.partner_car_images (car_id);

INSERT INTO public.car_model_images (model_id, image_url, image_type, display_order)
SELECT ci.car_id, ci.image_url, ci.image_type, ci.display_order
FROM public.car_images ci
LEFT JOIN public.car_model_images cmi
  ON cmi.model_id = ci.car_id
 AND cmi.image_url = ci.image_url
 AND cmi.image_type = ci.image_type
 AND cmi.display_order = ci.display_order
WHERE cmi.id IS NULL;

ALTER TABLE public.car_comments
    ADD COLUMN IF NOT EXISTS partner_car_id INT;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM pg_constraint
        WHERE conrelid = 'public.car_comments'::regclass
          AND conname = 'fk_car_comments_partner_car'
    ) THEN
        ALTER TABLE public.car_comments
            ADD CONSTRAINT fk_car_comments_partner_car
            FOREIGN KEY (partner_car_id) REFERENCES public.partner_cars(id) ON DELETE CASCADE;
    END IF;
END $$;

CREATE INDEX IF NOT EXISTS idx_car_comments_partner_car_id
    ON public.car_comments (partner_car_id);
