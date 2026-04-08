CREATE TEMP TABLE tmp_seed_car_images (
    license_plate VARCHAR(20) NOT NULL,
    legacy_image_id VARCHAR(255) NOT NULL,
    image_id VARCHAR(255) NOT NULL,
    image_url TEXT NOT NULL,
    image_type INT NOT NULL,
    display_order INT NOT NULL
) ON COMMIT DROP;

INSERT INTO tmp_seed_car_images (
    license_plate,
    legacy_image_id,
    image_id,
    image_url,
    image_type,
    display_order
)
VALUES
    (
        'KZP22202',
        'Toyota-Camry-2012.jpg',
        'cars/Toyota Camry 2012/Toyota-Camry-2012-front.png',
        '/internal/public/cars/Toyota Camry 2012/Toyota-Camry-2012-front.png',
        0,
        1
    ),
    (
        'KZP22202',
        'Toyota-Camry-2012.jpg',
        'cars/Toyota Camry 2012/Toyota-Camry-2012-side.jpg',
        '/internal/public/cars/Toyota Camry 2012/Toyota-Camry-2012-side.jpg',
        1,
        2
    ),
    (
        'KZP22201',
        'Toyota-Corolla-2010.jpg',
        'cars/Toyota Corolla 2010/Toyota-Corolla-2010-front.png',
        '/internal/public/cars/Toyota Corolla 2010/Toyota-Corolla-2010-front.png',
        0,
        1
    ),
    (
        'KZP22201',
        'Toyota-Corolla-2010.jpg',
        'cars/Toyota Corolla 2010/Toyota-Corolla-2010-back.jpg',
        '/internal/public/cars/Toyota Corolla 2010/Toyota-Corolla-2010-back.jpg',
        3,
        2
    ),
    (
        'KZP22203',
        'Nissan-Skyline-2003.jpg',
        'cars/Nissan Skyline 2003/Nissan-Skyline-2003-front.jpg',
        '/internal/public/cars/Nissan Skyline 2003/Nissan-Skyline-2003-front.jpg',
        0,
        1
    ),
    (
        'KZP22203',
        'Nissan-Skyline-2003.jpg',
        'cars/Nissan Skyline 2003/Nissan-Skyline-2003-back.png',
        '/internal/public/cars/Nissan Skyline 2003/Nissan-Skyline-2003-back.png',
        3,
        2
    ),
    (
        'KZP22203',
        'Nissan-Skyline-2003.jpg',
        'cars/Nissan Skyline 2003/Nissan-Skyline-2003-inside.png',
        '/internal/public/cars/Nissan Skyline 2003/Nissan-Skyline-2003-inside.png',
        2,
        3
    );

CREATE TEMP TABLE tmp_seed_car_image_targets ON COMMIT DROP AS
SELECT
    seed.license_plate,
    seed.legacy_image_id,
    seed.image_id,
    seed.image_url,
    seed.image_type,
    seed.display_order,
    partner_car.id AS car_id,
    partner_car.car_model_id AS model_id
FROM tmp_seed_car_images seed
JOIN public.partner_cars partner_car
    ON partner_car.license_plate = seed.license_plate;

DELETE FROM public.car_model_images existing
USING tmp_seed_car_image_targets target
WHERE existing.model_id = target.model_id
  AND existing.image_id IN (target.legacy_image_id, target.image_id);

DELETE FROM public.partner_car_images existing
USING tmp_seed_car_image_targets target
WHERE existing.car_id = target.car_id
  AND existing.image_id IN (target.legacy_image_id, target.image_id);

INSERT INTO public.car_model_images (
    model_id,
    image_url,
    image_id,
    image_type,
    display_order
)
SELECT DISTINCT
    target.model_id,
    target.image_url,
    target.image_id,
    target.image_type,
    target.display_order
FROM tmp_seed_car_image_targets target;

INSERT INTO public.partner_car_images (
    car_id,
    image_url,
    image_id,
    image_type,
    display_order
)
SELECT DISTINCT
    target.car_id,
    target.image_url,
    target.image_id,
    target.image_type,
    target.display_order
FROM tmp_seed_car_image_targets target;
