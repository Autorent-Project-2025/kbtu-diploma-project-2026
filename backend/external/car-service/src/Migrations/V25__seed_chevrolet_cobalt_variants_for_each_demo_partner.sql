CREATE TEMP TABLE tmp_cobalt_partner_car_seed (
    partner_user_id UUID NOT NULL,
    variant_code VARCHAR(10) NOT NULL,
    brand_name VARCHAR(255) NOT NULL,
    model_name VARCHAR(255) NOT NULL,
    model_year INT NOT NULL,
    engine VARCHAR(100),
    transmission VARCHAR(100),
    seats INT,
    fuel_type VARCHAR(50),
    doors INT,
    description VARCHAR(585),
    model_rating NUMERIC(2, 1),
    model_ratings_count INT,
    market_value_kzt NUMERIC(14, 2),
    license_plate VARCHAR(20) NOT NULL,
    color VARCHAR(50),
    ownership_file_name VARCHAR(255),
    partner_car_rating NUMERIC(2, 1),
    partner_car_ratings_count INT
) ON COMMIT DROP;

INSERT INTO tmp_cobalt_partner_car_seed (
    partner_user_id,
    variant_code,
    brand_name,
    model_name,
    model_year,
    engine,
    transmission,
    seats,
    fuel_type,
    doors,
    description,
    model_rating,
    model_ratings_count,
    market_value_kzt,
    license_plate,
    color,
    ownership_file_name,
    partner_car_rating,
    partner_car_ratings_count
)
VALUES
    (
        '22222222-2222-2222-2222-222222222222'::uuid,
        '1',
        'Chevrolet',
        'Cobalt',
        2020,
        '1.5L',
        'Manual',
        5,
        'Petrol',
        4,
        'Affordable city sedan with simple maintenance, roomy cabin, and a practical everyday setup.',
        4.2,
        3,
        5600000.00,
        'KZP22211',
        'White',
        NULL,
        4.3,
        2
    ),
    (
        '77777777-7777-7777-7777-777777777777'::uuid,
        '2',
        'Chevrolet',
        'Cobalt',
        2020,
        '1.5L',
        'Manual',
        5,
        'Petrol',
        4,
        'Affordable city sedan with simple maintenance, roomy cabin, and a practical everyday setup.',
        4.2,
        3,
        5600000.00,
        'KZP77721',
        'White',
        NULL,
        4.2,
        2
    ),
    (
        '88888888-8888-8888-8888-888888888888'::uuid,
        '3',
        'Chevrolet',
        'Cobalt',
        2020,
        '1.5L',
        'Manual',
        5,
        'Petrol',
        4,
        'Affordable city sedan with simple maintenance, roomy cabin, and a practical everyday setup.',
        4.2,
        3,
        5600000.00,
        'KZP88821',
        'White',
        NULL,
        4.2,
        2
    );

CREATE TEMP TABLE tmp_cobalt_model_image_seed (
    brand_name VARCHAR(255) NOT NULL,
    model_name VARCHAR(255) NOT NULL,
    model_year INT NOT NULL,
    image_id VARCHAR(255) NOT NULL,
    image_url TEXT NOT NULL,
    image_type INT NOT NULL,
    display_order INT NOT NULL
) ON COMMIT DROP;

INSERT INTO tmp_cobalt_model_image_seed (
    brand_name,
    model_name,
    model_year,
    image_id,
    image_url,
    image_type,
    display_order
)
VALUES
    ('Chevrolet', 'Cobalt', 2020, 'cars/Chevrolet Cobalt 2020/1/front.png', '/internal/public/cars/Chevrolet Cobalt 2020/1/front.png', 0, 1),
    ('Chevrolet', 'Cobalt', 2020, 'cars/Chevrolet Cobalt 2020/2/side.png', '/internal/public/cars/Chevrolet Cobalt 2020/2/side.png', 1, 2),
    ('Chevrolet', 'Cobalt', 2020, 'cars/Chevrolet Cobalt 2020/2/inside.png', '/internal/public/cars/Chevrolet Cobalt 2020/2/inside.png', 2, 3),
    ('Chevrolet', 'Cobalt', 2020, 'cars/Chevrolet Cobalt 2020/1/back.png', '/internal/public/cars/Chevrolet Cobalt 2020/1/back.png', 3, 4);

CREATE TEMP TABLE tmp_cobalt_partner_car_image_seed (
    license_plate VARCHAR(20) NOT NULL,
    image_id VARCHAR(255) NOT NULL,
    image_url TEXT NOT NULL,
    image_type INT NOT NULL,
    display_order INT NOT NULL
) ON COMMIT DROP;

INSERT INTO tmp_cobalt_partner_car_image_seed (
    license_plate,
    image_id,
    image_url,
    image_type,
    display_order
)
VALUES
    ('KZP22211', 'cars/Chevrolet Cobalt 2020/1/front.png', '/internal/public/cars/Chevrolet Cobalt 2020/1/front.png', 0, 1),
    ('KZP22211', 'cars/Chevrolet Cobalt 2020/1/back.png', '/internal/public/cars/Chevrolet Cobalt 2020/1/back.png', 3, 2),
    ('KZP77721', 'cars/Chevrolet Cobalt 2020/2/side.png', '/internal/public/cars/Chevrolet Cobalt 2020/2/side.png', 1, 1),
    ('KZP77721', 'cars/Chevrolet Cobalt 2020/2/inside.png', '/internal/public/cars/Chevrolet Cobalt 2020/2/inside.png', 2, 2),
    ('KZP77721', 'cars/Chevrolet Cobalt 2020/2/back.png', '/internal/public/cars/Chevrolet Cobalt 2020/2/back.png', 3, 3),
    ('KZP88821', 'cars/Chevrolet Cobalt 2020/3/front.png', '/internal/public/cars/Chevrolet Cobalt 2020/3/front.png', 0, 1),
    ('KZP88821', 'cars/Chevrolet Cobalt 2020/3/back.png', '/internal/public/cars/Chevrolet Cobalt 2020/3/back.png', 3, 2);

CREATE TEMP TABLE tmp_cobalt_car_model_semantic_feature_seed (
    brand_name VARCHAR(255) NOT NULL,
    model_name VARCHAR(255) NOT NULL,
    model_year INT NOT NULL,
    feature_name TEXT NOT NULL
) ON COMMIT DROP;

INSERT INTO tmp_cobalt_car_model_semantic_feature_seed (
    brand_name,
    model_name,
    model_year,
    feature_name
)
VALUES
    ('Chevrolet', 'Cobalt', 2020, 'city'),
    ('Chevrolet', 'Cobalt', 2020, 'family'),
    ('Chevrolet', 'Cobalt', 2020, 'sedan');

INSERT INTO public.brands (name)
SELECT DISTINCT seed.brand_name
FROM tmp_cobalt_partner_car_seed seed
WHERE NOT EXISTS (
    SELECT 1
    FROM public.brands existing_brand
    WHERE LOWER(existing_brand.name) = LOWER(seed.brand_name)
);

INSERT INTO public.models (brand_id, name)
SELECT DISTINCT brand.id, seed.model_name
FROM tmp_cobalt_partner_car_seed seed
JOIN public.brands brand
    ON LOWER(brand.name) = LOWER(seed.brand_name)
WHERE NOT EXISTS (
    SELECT 1
    FROM public.models existing_model
    WHERE existing_model.brand_id = brand.id
      AND LOWER(existing_model.name) = LOWER(seed.model_name)
);

INSERT INTO public.car_models (
    brand_id,
    model_id,
    year,
    engine,
    transmission,
    seats,
    fuel_type,
    doors,
    description,
    rating,
    ratings_count,
    market_value_kzt,
    market_value_fetched_at,
    market_value_source,
    market_value_source_url,
    market_value_sample_count,
    market_value_filtered_sample_count,
    market_value_confidence,
    market_value_status,
    market_value_error
)
SELECT DISTINCT
    brand.id,
    model_lookup.id,
    seed.model_year,
    seed.engine,
    seed.transmission,
    seed.seats,
    seed.fuel_type,
    seed.doors,
    seed.description,
    seed.model_rating,
    seed.model_ratings_count,
    seed.market_value_kzt,
    NOW(),
    'manual_seed',
    NULL,
    1,
    1,
    'high',
    'success',
    NULL
FROM tmp_cobalt_partner_car_seed seed
JOIN public.brands brand
    ON LOWER(brand.name) = LOWER(seed.brand_name)
JOIN public.models model_lookup
    ON model_lookup.brand_id = brand.id
   AND LOWER(model_lookup.name) = LOWER(seed.model_name)
ON CONFLICT (brand_id, model_id, year) DO UPDATE
SET
    engine = EXCLUDED.engine,
    transmission = EXCLUDED.transmission,
    seats = EXCLUDED.seats,
    fuel_type = EXCLUDED.fuel_type,
    doors = EXCLUDED.doors,
    description = EXCLUDED.description,
    rating = EXCLUDED.rating,
    ratings_count = EXCLUDED.ratings_count,
    market_value_kzt = EXCLUDED.market_value_kzt,
    market_value_fetched_at = EXCLUDED.market_value_fetched_at,
    market_value_source = EXCLUDED.market_value_source,
    market_value_source_url = EXCLUDED.market_value_source_url,
    market_value_sample_count = EXCLUDED.market_value_sample_count,
    market_value_filtered_sample_count = EXCLUDED.market_value_filtered_sample_count,
    market_value_confidence = EXCLUDED.market_value_confidence,
    market_value_status = EXCLUDED.market_value_status,
    market_value_error = EXCLUDED.market_value_error;

INSERT INTO public.partner_cars (
    partner_user_id,
    car_model_id,
    license_plate,
    ownership_file_name,
    color,
    price_hour,
    price_day,
    status,
    created_at,
    rating,
    ratings_count
)
SELECT
    seed.partner_user_id,
    car_model.id,
    seed.license_plate,
    seed.ownership_file_name,
    seed.color,
    CASE
        WHEN seed.market_value_kzt IS NULL OR seed.market_value_kzt <= 0 THEN NULL
        ELSE ROUND(
            seed.market_value_kzt *
            0.0001 *
            (1 + ((COALESCE(seed.partner_car_rating, seed.model_rating, 3.0) - 3.0) * 0.05)),
            2)
    END,
    CASE
        WHEN seed.market_value_kzt IS NULL OR seed.market_value_kzt <= 0 THEN NULL
        ELSE ROUND(
            ROUND(
                seed.market_value_kzt *
                0.0001 *
                (1 + ((COALESCE(seed.partner_car_rating, seed.model_rating, 3.0) - 3.0) * 0.05)),
                2) * 24 * 0.90,
            2)
    END,
    0,
    NOW(),
    seed.partner_car_rating,
    seed.partner_car_ratings_count
FROM tmp_cobalt_partner_car_seed seed
JOIN public.brands brand
    ON LOWER(brand.name) = LOWER(seed.brand_name)
JOIN public.models model_lookup
    ON model_lookup.brand_id = brand.id
   AND LOWER(model_lookup.name) = LOWER(seed.model_name)
JOIN public.car_models car_model
    ON car_model.brand_id = brand.id
   AND car_model.model_id = model_lookup.id
   AND car_model.year = seed.model_year
ON CONFLICT (license_plate) DO UPDATE
SET
    partner_user_id = EXCLUDED.partner_user_id,
    car_model_id = EXCLUDED.car_model_id,
    ownership_file_name = EXCLUDED.ownership_file_name,
    color = EXCLUDED.color,
    price_hour = EXCLUDED.price_hour,
    price_day = EXCLUDED.price_day,
    status = EXCLUDED.status,
    rating = EXCLUDED.rating,
    ratings_count = EXCLUDED.ratings_count;

INSERT INTO public.car_model_images (
    model_id,
    image_url,
    image_id,
    image_type,
    display_order
)
SELECT
    car_model.id,
    seed.image_url,
    seed.image_id,
    seed.image_type,
    seed.display_order
FROM tmp_cobalt_model_image_seed seed
JOIN public.brands brand
    ON LOWER(brand.name) = LOWER(seed.brand_name)
JOIN public.models model_lookup
    ON model_lookup.brand_id = brand.id
   AND LOWER(model_lookup.name) = LOWER(seed.model_name)
JOIN public.car_models car_model
    ON car_model.brand_id = brand.id
   AND car_model.model_id = model_lookup.id
   AND car_model.year = seed.model_year
WHERE NOT EXISTS (
    SELECT 1
    FROM public.car_model_images existing_image
    WHERE existing_image.model_id = car_model.id
      AND existing_image.image_id = seed.image_id
);

INSERT INTO public.partner_car_images (
    car_id,
    image_url,
    image_id,
    image_type,
    display_order
)
SELECT
    partner_car.id,
    seed.image_url,
    seed.image_id,
    seed.image_type,
    seed.display_order
FROM tmp_cobalt_partner_car_image_seed seed
JOIN public.partner_cars partner_car
    ON partner_car.license_plate = seed.license_plate
WHERE NOT EXISTS (
    SELECT 1
    FROM public.partner_car_images existing_image
    WHERE existing_image.car_id = partner_car.id
      AND existing_image.image_id = seed.image_id
);

INSERT INTO public.features (name)
SELECT DISTINCT LOWER(TRIM(seed.feature_name))
FROM tmp_cobalt_car_model_semantic_feature_seed seed
WHERE NOT EXISTS (
    SELECT 1
    FROM public.features feature
    WHERE LOWER(feature.name) = LOWER(TRIM(seed.feature_name))
);

INSERT INTO public.car_features (car_id, feature_id)
SELECT
    car_model.id,
    feature.id
FROM tmp_cobalt_car_model_semantic_feature_seed seed
JOIN public.brands brand
    ON LOWER(brand.name) = LOWER(seed.brand_name)
JOIN public.models model_lookup
    ON model_lookup.brand_id = brand.id
   AND LOWER(model_lookup.name) = LOWER(seed.model_name)
JOIN public.car_models car_model
    ON car_model.brand_id = brand.id
   AND car_model.model_id = model_lookup.id
   AND car_model.year = seed.model_year
JOIN public.features feature
    ON LOWER(feature.name) = LOWER(TRIM(seed.feature_name))
WHERE NOT EXISTS (
    SELECT 1
    FROM public.car_features car_feature
    WHERE car_feature.car_id = car_model.id
      AND car_feature.feature_id = feature.id
);
