WITH seed_users AS (
    SELECT *
    FROM (VALUES
        (1, '44444444-4444-4444-4444-444444444444'::uuid),
        (2, '55555555-5555-5555-5555-555555555555'::uuid),
        (3, '66666666-6666-6666-6666-666666666666'::uuid)
    ) AS seed(sort_order, user_id)
),
seed_cars AS (
    SELECT *
    FROM (VALUES
        (1, 1, 8),
        (2, 2, 6),
        (3, 3, 10)
    ) AS seed(sort_order, partner_car_id, billable_hours)
),
seed_bookings AS (
    SELECT
        11000 + (user_seed.sort_order * 10) + car_seed.sort_order AS booking_id,
        21000 + (user_seed.sort_order * 10) + car_seed.sort_order AS car_comment_id,
        user_seed.user_id,
        car_seed.partner_car_id,
        TIMESTAMPTZ '2026-03-01 10:00:00+05'
            + (((car_seed.sort_order - 1) * 3) + (user_seed.sort_order - 1)) * INTERVAL '1 day' AS start_time,
        TIMESTAMPTZ '2026-03-01 10:00:00+05'
            + (((car_seed.sort_order - 1) * 3) + (user_seed.sort_order - 1)) * INTERVAL '1 day'
            + car_seed.billable_hours * INTERVAL '1 hour' AS end_time,
        TIMESTAMPTZ '2026-03-01 10:00:00+05'
            + (((car_seed.sort_order - 1) * 3) + (user_seed.sort_order - 1)) * INTERVAL '1 day'
            + car_seed.billable_hours * INTERVAL '1 hour'
            + INTERVAL '12 hours' AS car_comment_submitted_at
    FROM seed_users user_seed
    CROSS JOIN seed_cars car_seed
)
UPDATE public.bookings booking
SET id = seed.booking_id
FROM seed_bookings seed
WHERE booking.user_id = seed.user_id
  AND booking.partner_car_id = seed.partner_car_id
  AND booking.start_time = seed.start_time
  AND booking.end_time = seed.end_time
  AND booking.id <> seed.booking_id
  AND NOT EXISTS (
      SELECT 1
      FROM public.bookings conflict
      WHERE conflict.id = seed.booking_id
  );

WITH seed_users AS (
    SELECT *
    FROM (VALUES
        (1, '44444444-4444-4444-4444-444444444444'::uuid),
        (2, '55555555-5555-5555-5555-555555555555'::uuid),
        (3, '66666666-6666-6666-6666-666666666666'::uuid)
    ) AS seed(sort_order, user_id)
),
seed_cars AS (
    SELECT *
    FROM (VALUES
        (1, 1, 8),
        (2, 2, 6),
        (3, 3, 10)
    ) AS seed(sort_order, partner_car_id, billable_hours)
),
seed_bookings AS (
    SELECT
        11000 + (user_seed.sort_order * 10) + car_seed.sort_order AS booking_id,
        21000 + (user_seed.sort_order * 10) + car_seed.sort_order AS car_comment_id,
        user_seed.user_id,
        car_seed.partner_car_id,
        TIMESTAMPTZ '2026-03-01 10:00:00+05'
            + (((car_seed.sort_order - 1) * 3) + (user_seed.sort_order - 1)) * INTERVAL '1 day' AS start_time,
        TIMESTAMPTZ '2026-03-01 10:00:00+05'
            + (((car_seed.sort_order - 1) * 3) + (user_seed.sort_order - 1)) * INTERVAL '1 day'
            + car_seed.billable_hours * INTERVAL '1 hour' AS end_time,
        TIMESTAMPTZ '2026-03-01 10:00:00+05'
            + (((car_seed.sort_order - 1) * 3) + (user_seed.sort_order - 1)) * INTERVAL '1 day'
            + car_seed.billable_hours * INTERVAL '1 hour'
            + INTERVAL '12 hours' AS car_comment_submitted_at
    FROM seed_users user_seed
    CROSS JOIN seed_cars car_seed
)
UPDATE public.bookings booking
SET
    car_comment_id = seed.car_comment_id,
    car_comment_submitted_at = seed.car_comment_submitted_at
FROM seed_bookings seed
WHERE booking.id = seed.booking_id
   OR (
      booking.user_id = seed.user_id
  AND booking.partner_car_id = seed.partner_car_id
  AND booking.start_time = seed.start_time
  AND booking.end_time = seed.end_time
   );

SELECT setval(
    pg_get_serial_sequence('public.bookings', 'id'),
    COALESCE((SELECT MAX(id) FROM public.bookings), 1),
    TRUE
);
