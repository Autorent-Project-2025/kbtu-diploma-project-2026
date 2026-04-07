-- Relies on the default demo cars seeded by car-service migration V9.
-- The seeded partner cars are expected to have ids 1..3 in this order:
-- 1 = Toyota Corolla, 2 = Toyota Camry, 3 = Nissan Skyline.
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
        (1, 1, 'Toyota', 'Corolla', 'Toyota-Corolla-2010.jpg', 3900.00::numeric(10, 2), 8, 4800000.00::numeric(12, 2), 4.8::numeric(3, 1)),
        (2, 2, 'Toyota', 'Camry', 'Toyota-Camry-2012.jpg', 4700.00::numeric(10, 2), 6, 7200000.00::numeric(12, 2), 4.9::numeric(3, 1)),
        (3, 3, 'Nissan', 'Skyline', 'Nissan-Skyline-2003.jpg', 5200.00::numeric(10, 2), 10, 6500000.00::numeric(12, 2), 4.7::numeric(3, 1))
    ) AS seed(
        sort_order,
        partner_car_id,
        car_brand,
        car_model,
        image_id,
        price_hour,
        billable_hours,
        market_value_kzt,
        rating
    )
),
scheduled_bookings AS (
    SELECT
        user_seed.user_id,
        car_seed.partner_car_id,
        '22222222-2222-2222-2222-222222222222'::uuid AS partner_user_id,
        car_seed.car_brand,
        car_seed.car_model,
        'Demo Partner'::varchar(255) AS partner_name,
        '/internal/public/' || car_seed.image_id AS cover_image_url,
        jsonb_build_array('/internal/public/' || car_seed.image_id) AS image_urls,
        car_seed.price_hour,
        (car_seed.price_hour * car_seed.billable_hours)::numeric(10, 2) AS total_price,
        car_seed.billable_hours,
        car_seed.market_value_kzt,
        car_seed.rating,
        TIMESTAMPTZ '2026-03-01 10:00:00+05'
            + (((car_seed.sort_order - 1) * 3) + (user_seed.sort_order - 1)) * INTERVAL '1 day' AS start_time
    FROM seed_users user_seed
    CROSS JOIN seed_cars car_seed
),
completed_bookings AS (
    SELECT
        booking.user_id,
        booking.partner_car_id,
        booking.partner_user_id,
        booking.car_brand,
        booking.car_model,
        booking.partner_name,
        booking.cover_image_url,
        booking.image_urls,
        booking.price_hour,
        booking.total_price,
        booking.billable_hours,
        booking.market_value_kzt,
        booking.rating,
        booking.start_time,
        booking.start_time + booking.billable_hours * INTERVAL '1 hour' AS end_time,
        booking.start_time - INTERVAL '4 days' AS created_at,
        booking.start_time AS trip_started_at,
        booking.start_time + booking.billable_hours * INTERVAL '1 hour' AS trip_completed_at
    FROM scheduled_bookings booking
)
INSERT INTO public.bookings (
    user_id,
    partner_car_id,
    partner_user_id,
    car_brand,
    car_model,
    partner_name,
    cover_image_url,
    image_urls,
    start_time,
    end_time,
    price_hour,
    total_price,
    created_at,
    trip_started_at,
    trip_completed_at,
    used_subscription,
    pricing_breakdown,
    status
)
SELECT
    booking.user_id,
    booking.partner_car_id,
    booking.partner_user_id,
    booking.car_brand,
    booking.car_model,
    booking.partner_name,
    booking.cover_image_url,
    booking.image_urls,
    booking.start_time,
    booking.end_time,
    booking.price_hour,
    booking.total_price,
    booking.created_at,
    booking.trip_started_at,
    booking.trip_completed_at,
    FALSE,
    jsonb_build_object(
        'quotedAtUtc', booking.created_at,
        'marketValueKzt', booking.market_value_kzt,
        'rating', booking.rating,
        'currentAvailableCarsCount', 3,
        'daysBeforeBooking', 4,
        'billableHours', booking.billable_hours,
        'ratingCoefficient', 1.00,
        'advanceBookingCoefficient', 1.00,
        'availabilityCoefficient', 1.00,
        'quotedPriceHour', booking.price_hour,
        'quotedTotalPrice', booking.total_price,
        'currency', 'KZT',
        'isMarketValueStale', FALSE
    ),
    'completed'
FROM completed_bookings booking
WHERE NOT EXISTS (
    SELECT 1
    FROM public.bookings existing
    WHERE existing.user_id = booking.user_id
      AND existing.partner_car_id = booking.partner_car_id
      AND existing.start_time = booking.start_time
      AND existing.end_time = booking.end_time
);
