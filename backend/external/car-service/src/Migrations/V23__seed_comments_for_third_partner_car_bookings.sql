WITH seed_users AS (
    SELECT *
    FROM (VALUES
        (1, '44444444-4444-4444-4444-444444444444'::varchar(64), 'Aruzhan Sarsenova'),
        (2, '55555555-5555-5555-5555-555555555555'::varchar(64), 'Dias Nurgaliyev'),
        (3, '66666666-6666-6666-6666-666666666666'::varchar(64), 'Aigerim Toktarova')
    ) AS seed(sort_order, user_id, user_name)
),
seed_cars AS (
    SELECT *
    FROM (VALUES
        (1, 'KZP88811'::varchar(20), 10),
        (2, 'KZP88812'::varchar(20), 8),
        (3, 'KZP88813'::varchar(20), 12)
    ) AS seed(sort_order, license_plate, billable_hours)
),
seed_reviews AS (
    SELECT
        23000 + (user_seed.sort_order * 10) + car_seed.sort_order AS comment_id,
        13000 + (user_seed.sort_order * 10) + car_seed.sort_order AS booking_id,
        user_seed.user_id,
        user_seed.user_name,
        partner_car.id AS partner_car_id,
        partner_car.car_model_id,
        CASE
            WHEN car_seed.sort_order = 1 AND user_seed.sort_order = 2 THEN 4
            WHEN car_seed.sort_order = 2 AND user_seed.sort_order = 3 THEN 4
            WHEN car_seed.sort_order = 3 AND user_seed.sort_order = 1 THEN 4
            WHEN car_seed.sort_order = 3 AND user_seed.sort_order = 2 THEN 4
            ELSE 5
        END AS rating,
        CASE
            WHEN car_seed.sort_order = 1 AND user_seed.sort_order = 1 THEN 'Audi felt composed on the highway and the cabin was quiet enough for work calls on the go.'
            WHEN car_seed.sort_order = 1 AND user_seed.sort_order = 2 THEN 'The A6 looked sharp and was comfortable, though the suspension felt a bit firm on rough streets.'
            WHEN car_seed.sort_order = 1 THEN 'Balanced business sedan with good traction and a solid premium feel.'
            WHEN car_seed.sort_order = 2 AND user_seed.sort_order = 1 THEN 'Rear-seat comfort and isolation were excellent, this one really felt like a flagship car.'
            WHEN car_seed.sort_order = 2 AND user_seed.sort_order = 2 THEN 'Very smooth V8 cruiser, ideal for a formal trip and much quieter than expected.'
            WHEN car_seed.sort_order = 2 THEN 'Strong luxury impression overall, just a little large for tight parking downtown.'
            WHEN car_seed.sort_order = 3 AND user_seed.sort_order = 1 THEN 'Old Camry was simple but honest, easy to drive and perfect for everyday errands.'
            WHEN car_seed.sort_order = 3 AND user_seed.sort_order = 2 THEN 'Comfortable enough for the price and everything worked without surprises.'
            ELSE 'Affordable and practical option, especially if you just need a reliable sedan for the day.'
        END AS content,
        (
            TIMESTAMP '2026-03-28 10:00:00'
            + (((car_seed.sort_order - 1) * 3) + (user_seed.sort_order - 1)) * INTERVAL '1 day'
            + car_seed.billable_hours * INTERVAL '1 hour'
            + INTERVAL '10 hours'
        ) AS created_on
    FROM seed_users user_seed
    CROSS JOIN seed_cars car_seed
    JOIN public.partner_cars partner_car
        ON partner_car.license_plate = car_seed.license_plate
)
INSERT INTO public.car_comments (
    id,
    user_id,
    user_name,
    car_id,
    partner_car_id,
    booking_id,
    content,
    rating,
    created_on
)
SELECT
    review.comment_id,
    review.user_id,
    review.user_name,
    review.car_model_id,
    review.partner_car_id,
    review.booking_id,
    review.content,
    review.rating,
    review.created_on
FROM seed_reviews review
WHERE NOT EXISTS (
    SELECT 1
    FROM public.car_comments existing
    WHERE existing.id = review.comment_id
       OR existing.booking_id = review.booking_id
);

SELECT setval(
    pg_get_serial_sequence('public.car_comments', 'id'),
    COALESCE((SELECT MAX(id) FROM public.car_comments), 1),
    TRUE
);

WITH partner_car_rating_aggregates AS (
    SELECT
        partner_car.id AS partner_car_id,
        COUNT(comment.id)::int AS ratings_count,
        CASE
            WHEN COUNT(comment.id) = 0 THEN NULL
            ELSE ROUND(AVG(comment.rating)::numeric, 1)
        END AS rating
    FROM public.partner_cars partner_car
    LEFT JOIN public.car_comments comment
        ON comment.partner_car_id = partner_car.id
    GROUP BY partner_car.id
)
UPDATE public.partner_cars partner_car
SET
    ratings_count = aggregate.ratings_count,
    rating = aggregate.rating
FROM partner_car_rating_aggregates aggregate
WHERE partner_car.id = aggregate.partner_car_id;

WITH car_model_rating_aggregates AS (
    SELECT
        car_model.id AS car_model_id,
        COUNT(comment.id)::int AS ratings_count,
        CASE
            WHEN COUNT(comment.id) = 0 THEN NULL
            ELSE ROUND(AVG(comment.rating)::numeric, 1)
        END AS rating
    FROM public.car_models car_model
    LEFT JOIN public.car_comments comment
        ON comment.car_id = car_model.id
    GROUP BY car_model.id
)
UPDATE public.car_models car_model
SET
    ratings_count = aggregate.ratings_count,
    rating = aggregate.rating
FROM car_model_rating_aggregates aggregate
WHERE car_model.id = aggregate.car_model_id;
