CREATE TEMP TABLE IF NOT EXISTS tmp_partner_car_seed (
    brand_name VARCHAR(255) NOT NULL,
    model_name VARCHAR(255) NOT NULL,
    image_file_name VARCHAR(255) NOT NULL,
    model_year INT NOT NULL,
    engine VARCHAR(100),
    transmission VARCHAR(100),
    seats INT,
    fuel_type VARCHAR(50),
    doors INT,
    description VARCHAR(585),
    model_rating NUMERIC(2, 1),
    model_ratings_count INT,
    license_plate VARCHAR(20) NOT NULL,
    color VARCHAR(50),
    price_hour NUMERIC(10, 2),
    price_day NUMERIC(10, 2),
    ownership_file_name VARCHAR(255),
    partner_car_rating NUMERIC(2, 1),
    partner_car_ratings_count INT,
    image_url TEXT NOT NULL
) ON COMMIT DROP;

TRUNCATE TABLE tmp_partner_car_seed;

INSERT INTO tmp_partner_car_seed (
    brand_name,
    model_name,
    image_file_name,
    model_year,
    engine,
    transmission,
    seats,
    fuel_type,
    doors,
    description,
    model_rating,
    model_ratings_count,
    license_plate,
    color,
    price_hour,
    price_day,
    ownership_file_name,
    partner_car_rating,
    partner_car_ratings_count,
    image_url
)
SELECT
    seed.brand_name,
    seed.model_name,
    seed.image_file_name,
    CAST(substring(seed.image_file_name FROM '([0-9]{4})(?=\.[^.]+$)') AS INT) AS model_year,
    seed.engine,
    seed.transmission,
    seed.seats,
    seed.fuel_type,
    seed.doors,
    seed.description,
    seed.model_rating,
    seed.model_ratings_count,
    seed.license_plate,
    seed.color,
    seed.price_hour,
    seed.price_day,
    seed.ownership_file_name,
    seed.partner_car_rating,
    seed.partner_car_ratings_count,
    '/internal/public/' || seed.image_file_name AS image_url
FROM (
    VALUES
        ('Toyota', 'Corolla', 'Toyota-Corolla-2010.jpg', '1.6L', 'Automatic', 5, 'Petrol', 4, 'Reliable compact sedan, good for city driving.', 4.7::numeric, 12, 'KZP22201', 'White', 3900.00::numeric, 32000.00::numeric, 'ownership-demo-corolla.pdf', 4.8::numeric, 18),
        ('Toyota', 'Camry', 'Toyota-Camry-2012.jpg', '2.5L', 'Automatic', 5, 'Petrol', 4, 'Comfortable business-class sedan for long trips.', 4.8::numeric, 20, 'KZP22202', 'Black', 4700.00::numeric, 39500.00::numeric, 'ownership-demo-camry.pdf', 4.9::numeric, 24),
        ('Nissan', 'Skyline', 'Nissan-Skyline-2003.jpg', '3.5L V6', 'Automatic', 5, 'Petrol', 4, 'Sport-oriented sedan with strong performance.', 4.6::numeric, 9, 'KZP22203', 'Silver', 5200.00::numeric, 43000.00::numeric, 'ownership-demo-skyline.pdf', 4.7::numeric, 11)
) AS seed(
    brand_name,
    model_name,
    image_file_name,
    engine,
    transmission,
    seats,
    fuel_type,
    doors,
    description,
    model_rating,
    model_ratings_count,
    license_plate,
    color,
    price_hour,
    price_day,
    ownership_file_name,
    partner_car_rating,
    partner_car_ratings_count
)
WHERE substring(seed.image_file_name FROM '([0-9]{4})(?=\.[^.]+$)') IS NOT NULL;

INSERT INTO public.brands (name)
SELECT DISTINCT seed.brand_name
FROM tmp_partner_car_seed seed
WHERE NOT EXISTS (
    SELECT 1
    FROM public.brands existing_brand
    WHERE LOWER(existing_brand.name) = LOWER(seed.brand_name)
);

INSERT INTO public.models (brand_id, name)
SELECT DISTINCT brand.id, seed.model_name
FROM tmp_partner_car_seed seed
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
    ratings_count
)
SELECT
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
    seed.model_ratings_count
FROM tmp_partner_car_seed seed
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
    ratings_count = EXCLUDED.ratings_count;

INSERT INTO public.partner_cars (
    partner_id,
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
    '22222222-2222-2222-2222-222222222222'::uuid,
    car_model.id,
    seed.license_plate,
    seed.ownership_file_name,
    seed.color,
    seed.price_hour,
    seed.price_day,
    0,
    NOW(),
    seed.partner_car_rating,
    seed.partner_car_ratings_count
FROM tmp_partner_car_seed seed
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
    partner_id = EXCLUDED.partner_id,
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
    seed.image_file_name,
    4,
    1
FROM tmp_partner_car_seed seed
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
      AND existing_image.image_id = seed.image_file_name
      AND existing_image.image_type = 4
      AND existing_image.display_order = 1
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
    seed.image_file_name,
    4,
    1
FROM tmp_partner_car_seed seed
JOIN public.partner_cars partner_car
    ON partner_car.license_plate = seed.license_plate
WHERE NOT EXISTS (
    SELECT 1
    FROM public.partner_car_images existing_image
    WHERE existing_image.car_id = partner_car.id
      AND existing_image.image_id = seed.image_file_name
      AND existing_image.image_type = 4
      AND existing_image.display_order = 1
);
