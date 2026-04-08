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
        (
            1,
            4,
            '77777777-7777-7777-7777-777777777777'::uuid,
            'Kia',
            'K5',
            'Ayan Tulegenov'::varchar(255),
            '/internal/public/cars/Kia K5 2021/1/front.png'::varchar(2048),
            jsonb_build_array(
                '/internal/public/cars/Kia K5 2021/1/front.png',
                '/internal/public/cars/Kia K5 2021/1/side.png',
                '/internal/public/cars/Kia K5 2021/1/inside.png'
            ),
            1580.50::numeric(10, 2),
            12,
            14500000.00::numeric(12, 2),
            4.8::numeric(3, 1),
            2
        ),
        (
            2,
            5,
            '77777777-7777-7777-7777-777777777777'::uuid,
            'Kia',
            'K5',
            'Ayan Tulegenov'::varchar(255),
            '/internal/public/cars/Kia K5 2021/2/front.png'::varchar(2048),
            jsonb_build_array(
                '/internal/public/cars/Kia K5 2021/2/front.png'
            ),
            1566.00::numeric(10, 2),
            10,
            14500000.00::numeric(12, 2),
            4.6::numeric(3, 1),
            2
        ),
        (
            3,
            6,
            '77777777-7777-7777-7777-777777777777'::uuid,
            'Mazda',
            'RX7',
            'Ayan Tulegenov'::varchar(255),
            '/internal/public/cars/Mazda RX7 1992/front.png'::varchar(2048),
            jsonb_build_array(
                '/internal/public/cars/Mazda RX7 1992/front.png',
                '/internal/public/cars/Mazda RX7 1992/side.png'
            ),
            2299.50::numeric(10, 2),
            6,
            21000000.00::numeric(12, 2),
            4.9::numeric(3, 1),
            1
        ),
        (
            4,
            7,
            '77777777-7777-7777-7777-777777777777'::uuid,
            'Toyota',
            'Supra',
            'Ayan Tulegenov'::varchar(255),
            '/internal/public/cars/Toyota Supra 1996/front.png'::varchar(2048),
            jsonb_build_array(
                '/internal/public/cars/Toyota Supra 1996/front.png',
                '/internal/public/cars/Toyota Supra 1996/side.png'
            ),
            2725.00::numeric(10, 2),
            8,
            25000000.00::numeric(12, 2),
            4.8::numeric(3, 1),
            1
        )
    ) AS seed(
        sort_order,
        partner_car_id,
        partner_user_id,
        car_brand,
        car_model,
        partner_name,
        cover_image_url,
        image_urls,
        price_hour,
        billable_hours,
        market_value_kzt,
        rating,
        current_available_cars_count
    )
),
scheduled_bookings AS (
    SELECT
        12000 + (user_seed.sort_order * 10) + car_seed.sort_order AS booking_id,
        22000 + (user_seed.sort_order * 10) + car_seed.sort_order AS car_comment_id,
        user_seed.user_id,
        car_seed.partner_car_id,
        car_seed.partner_user_id,
        car_seed.car_brand,
        car_seed.car_model,
        car_seed.partner_name,
        car_seed.cover_image_url,
        car_seed.image_urls,
        car_seed.price_hour,
        (car_seed.price_hour * car_seed.billable_hours)::numeric(10, 2) AS total_price,
        car_seed.billable_hours,
        car_seed.market_value_kzt,
        car_seed.rating,
        car_seed.current_available_cars_count,
        TIMESTAMPTZ '2026-03-18 09:00:00+05'
            + (((car_seed.sort_order - 1) * 3) + (user_seed.sort_order - 1)) * INTERVAL '1 day' AS start_time
    FROM seed_users user_seed
    CROSS JOIN seed_cars car_seed
),
completed_bookings AS (
    SELECT
        booking.booking_id,
        booking.car_comment_id,
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
        booking.current_available_cars_count,
        booking.start_time,
        booking.start_time + booking.billable_hours * INTERVAL '1 hour' AS end_time,
        booking.start_time - INTERVAL '5 days' AS created_at,
        booking.start_time AS trip_started_at,
        booking.start_time + booking.billable_hours * INTERVAL '1 hour' AS trip_completed_at,
        booking.start_time + booking.billable_hours * INTERVAL '1 hour' + INTERVAL '12 hours' AS car_comment_submitted_at
    FROM scheduled_bookings booking
)
INSERT INTO public.bookings (
    id,
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
    status,
    car_comment_id,
    car_comment_submitted_at
)
SELECT
    booking.booking_id,
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
        'currentAvailableCarsCount', booking.current_available_cars_count,
        'daysBeforeBooking', 5,
        'billableHours', booking.billable_hours,
        'ratingCoefficient', ROUND(1 + ((booking.rating - 3.0) * 0.05), 4),
        'advanceBookingCoefficient', 1.00,
        'availabilityCoefficient', 1.00,
        'quotedPriceHour', booking.price_hour,
        'quotedTotalPrice', booking.total_price,
        'currency', 'KZT',
        'isMarketValueStale', FALSE
    ),
    'completed',
    booking.car_comment_id,
    booking.car_comment_submitted_at
FROM completed_bookings booking
WHERE NOT EXISTS (
    SELECT 1
    FROM public.bookings existing
    WHERE existing.id = booking.booking_id
       OR (
            existing.user_id = booking.user_id
        AND existing.partner_car_id = booking.partner_car_id
        AND existing.start_time = booking.start_time
        AND existing.end_time = booking.end_time
       )
);

SELECT setval(
    pg_get_serial_sequence('public.bookings', 'id'),
    COALESCE((SELECT MAX(id) FROM public.bookings), 1),
    TRUE
);
