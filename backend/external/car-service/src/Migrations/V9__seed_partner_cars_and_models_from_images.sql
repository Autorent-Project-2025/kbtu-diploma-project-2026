WITH seed_rows AS (
    SELECT *
    FROM (
        VALUES
            ('Toyota', 'Corolla', 'Toyota-Corolla-2010.jpg', '1.6L', 'Automatic', 5, 'Petrol', 4, 'Reliable compact sedan, good for city driving.', 4.7::numeric, 12, 'KZP22201', 'White', 3900.00::numeric, 32000.00::numeric, 'ownership-demo-corolla.pdf', 4.8::numeric, 18),
            ('Toyota', 'Camry', 'Toyota-Camry-2012.jpg', '2.5L', 'Automatic', 5, 'Petrol', 4, 'Comfortable business-class sedan for long trips.', 4.8::numeric, 20, 'KZP22202', 'Black', 4700.00::numeric, 39500.00::numeric, 'ownership-demo-camry.pdf', 4.9::numeric, 24),
            ('Nissan', 'Skyline', 'Nissan-Skyline-2003.jpg', '3.5L V6', 'Automatic', 5, 'Petrol', 4, 'Sport-oriented sedan with strong performance.', 4.6::numeric, 9, 'KZP22203', 'Silver', 5200.00::numeric, 43000.00::numeric, 'ownership-demo-skyline.pdf', 4.7::numeric, 11)
    ) AS source(
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
),
normalized_seed AS (
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
    FROM seed_rows seed
),
inserted_brands AS (
    INSERT INTO public.brands (name)
    SELECT DISTINCT normalized.brand_name
    FROM normalized_seed normalized
    WHERE NOT EXISTS (
        SELECT 1
        FROM public.brands existing_brand
        WHERE LOWER(existing_brand.name) = LOWER(normalized.brand_name)
    )
    RETURNING id
),
inserted_models AS (
    INSERT INTO public.models (brand_id, name)
    SELECT DISTINCT brand.id, normalized.model_name
    FROM normalized_seed normalized
    JOIN public.brands brand
        ON LOWER(brand.name) = LOWER(normalized.brand_name)
    WHERE NOT EXISTS (
        SELECT 1
        FROM public.models existing_model
        WHERE existing_model.brand_id = brand.id
          AND LOWER(existing_model.name) = LOWER(normalized.model_name)
    )
    RETURNING id
),
upserted_car_models AS (
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
        normalized.model_year,
        normalized.engine,
        normalized.transmission,
        normalized.seats,
        normalized.fuel_type,
        normalized.doors,
        normalized.description,
        normalized.model_rating,
        normalized.model_ratings_count
    FROM normalized_seed normalized
    JOIN public.brands brand
        ON LOWER(brand.name) = LOWER(normalized.brand_name)
    JOIN public.models model_lookup
        ON model_lookup.brand_id = brand.id
       AND LOWER(model_lookup.name) = LOWER(normalized.model_name)
    ON CONFLICT (brand_id, model_id, year) DO UPDATE
    SET
        engine = EXCLUDED.engine,
        transmission = EXCLUDED.transmission,
        seats = EXCLUDED.seats,
        fuel_type = EXCLUDED.fuel_type,
        doors = EXCLUDED.doors,
        description = EXCLUDED.description,
        rating = EXCLUDED.rating,
        ratings_count = EXCLUDED.ratings_count
    RETURNING id
),
car_model_targets AS (
    SELECT
        normalized.brand_name,
        normalized.model_name,
        normalized.image_file_name,
        normalized.image_url,
        normalized.model_year,
        normalized.license_plate,
        normalized.color,
        normalized.price_hour,
        normalized.price_day,
        normalized.ownership_file_name,
        normalized.partner_car_rating,
        normalized.partner_car_ratings_count,
        car_model.id AS car_model_id
    FROM normalized_seed normalized
    JOIN public.brands brand
        ON LOWER(brand.name) = LOWER(normalized.brand_name)
    JOIN public.models model_lookup
        ON model_lookup.brand_id = brand.id
       AND LOWER(model_lookup.name) = LOWER(normalized.model_name)
    JOIN public.car_models car_model
        ON car_model.brand_id = brand.id
       AND car_model.model_id = model_lookup.id
       AND car_model.year = normalized.model_year
),
inserted_model_images AS (
    INSERT INTO public.car_model_images (
        model_id,
        image_url,
        image_id,
        image_type,
        display_order
    )
    SELECT
        target.car_model_id,
        target.image_url,
        target.image_file_name,
        4,
        1
    FROM car_model_targets target
    WHERE NOT EXISTS (
        SELECT 1
        FROM public.car_model_images existing_image
        WHERE existing_image.model_id = target.car_model_id
          AND existing_image.image_id = target.image_file_name
          AND existing_image.image_type = 4
          AND existing_image.display_order = 1
    )
    RETURNING id
),
upserted_partner_cars AS (
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
        target.car_model_id,
        target.license_plate,
        target.ownership_file_name,
        target.color,
        target.price_hour,
        target.price_day,
        0,
        NOW(),
        target.partner_car_rating,
        target.partner_car_ratings_count
    FROM car_model_targets target
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
        ratings_count = EXCLUDED.ratings_count
    RETURNING id
)
INSERT INTO public.partner_car_images (
    car_id,
    image_url,
    image_id,
    image_type,
    display_order
)
SELECT
    partner_car.id,
    target.image_url,
    target.image_file_name,
    4,
    1
FROM car_model_targets target
JOIN public.partner_cars partner_car
    ON partner_car.license_plate = target.license_plate
WHERE NOT EXISTS (
    SELECT 1
    FROM public.partner_car_images existing_image
    WHERE existing_image.car_id = partner_car.id
      AND existing_image.image_id = target.image_file_name
      AND existing_image.image_type = 4
      AND existing_image.display_order = 1
);
