-- Seed reviews for the demo completed bookings created in booking-service V11.
-- The booking ids and comment ids are deterministic so both databases stay aligned.
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
        (1, 1, 8, 5, 'Clean and easy to drive, great option for city trips.'),
        (2, 2, 6, 5, 'Comfortable ride, tidy interior, and smooth booking flow.'),
        (3, 3, 10, 4, 'Strong overall impression, but the suspension felt a bit stiff.')
    ) AS seed(sort_order, partner_car_id, billable_hours, rating_user_1, content_user_1)
),
seed_reviews AS (
    SELECT
        21000 + (user_seed.sort_order * 10) + car_seed.sort_order AS comment_id,
        11000 + (user_seed.sort_order * 10) + car_seed.sort_order AS booking_id,
        user_seed.user_id,
        user_seed.user_name,
        car_seed.partner_car_id,
        partner_car.car_model_id,
        CASE
            WHEN user_seed.sort_order = 1 THEN car_seed.rating_user_1
            WHEN user_seed.sort_order = 2 AND car_seed.sort_order = 1 THEN 4
            WHEN user_seed.sort_order = 2 AND car_seed.sort_order = 2 THEN 5
            WHEN user_seed.sort_order = 2 AND car_seed.sort_order = 3 THEN 4
            WHEN user_seed.sort_order = 3 AND car_seed.sort_order = 1 THEN 5
            WHEN user_seed.sort_order = 3 AND car_seed.sort_order = 2 THEN 4
            ELSE 5
        END AS rating,
        CASE
            WHEN user_seed.sort_order = 1 THEN car_seed.content_user_1
            WHEN user_seed.sort_order = 2 AND car_seed.sort_order = 1 THEN 'Pickup was simple, fuel usage was fine, and the drive felt easy.'
            WHEN user_seed.sort_order = 2 AND car_seed.sort_order = 2 THEN 'Very comfortable for a longer ride, the whole trip went smoothly.'
            WHEN user_seed.sort_order = 2 AND car_seed.sort_order = 3 THEN 'Good dynamics, but the car felt a little harsh for daily driving.'
            WHEN user_seed.sort_order = 3 AND car_seed.sort_order = 1 THEN 'Efficient and predictable car, only positive impressions after the trip.'
            WHEN user_seed.sort_order = 3 AND car_seed.sort_order = 2 THEN 'Overall very good, although the interior could feel a bit fresher.'
            ELSE 'Memorable car and enjoyable drive, I would rent it again.'
        END AS content,
        (
            TIMESTAMP '2026-03-01 10:00:00'
            + (((car_seed.sort_order - 1) * 3) + (user_seed.sort_order - 1)) * INTERVAL '1 day'
            + car_seed.billable_hours * INTERVAL '1 hour'
            + INTERVAL '12 hours'
        ) AS created_on
    FROM seed_users user_seed
    CROSS JOIN seed_cars car_seed
    JOIN public.partner_cars partner_car
        ON partner_car.id = car_seed.partner_car_id
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
