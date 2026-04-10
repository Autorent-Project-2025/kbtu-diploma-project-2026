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
    completion_review_ticket_id,
    used_subscription,
    pricing_breakdown,
    status
)
SELECT
    15041,
    '44444444-4444-4444-4444-444444444444'::uuid,
    11,
    '22222222-2222-2222-2222-222222222222'::uuid,
    'Chevrolet',
    'Cobalt',
    'Demo Partner',
    '/internal/public/cars/Chevrolet Cobalt 2020/1/front.png',
    jsonb_build_array(
        '/internal/public/cars/Chevrolet Cobalt 2020/1/front.png',
        '/internal/public/cars/Chevrolet Cobalt 2020/1/back.png'
    ),
    TIMESTAMPTZ '2026-04-09 10:00:00+05',
    TIMESTAMPTZ '2026-04-09 22:00:00+05',
    596.40::numeric(10, 2),
    7156.80::numeric(10, 2),
    TIMESTAMPTZ '2026-04-08 09:30:00+05',
    TIMESTAMPTZ '2026-04-09 10:00:00+05',
    TIMESTAMPTZ '2026-04-09 22:00:00+05',
    '99999999-9999-9999-9999-999999999994'::uuid,
    FALSE,
    jsonb_build_object(
        'quotedAtUtc', TIMESTAMPTZ '2026-04-08 09:30:00+05',
        'marketValueKzt', 5600000.00::numeric(12, 2),
        'rating', 4.3::numeric(3, 1),
        'currentAvailableCarsCount', 3,
        'daysBeforeBooking', 1,
        'billableHours', 12,
        'ratingCoefficient', 1.0650::numeric(10, 4),
        'advanceBookingCoefficient', 1.00::numeric(10, 2),
        'availabilityCoefficient', 1.00::numeric(10, 2),
        'quotedPriceHour', 596.40::numeric(10, 2),
        'quotedTotalPrice', 7156.80::numeric(10, 2),
        'currency', 'KZT',
        'isMarketValueStale', FALSE
    ),
    'awaitingreview'
WHERE NOT EXISTS (
    SELECT 1
    FROM public.bookings existing
    WHERE existing.id = 15041
       OR existing.completion_review_ticket_id = '99999999-9999-9999-9999-999999999994'::uuid
       OR (
            existing.user_id = '44444444-4444-4444-4444-444444444444'::uuid
        AND existing.partner_car_id = 11
        AND existing.start_time = TIMESTAMPTZ '2026-04-09 10:00:00+05'
        AND existing.end_time = TIMESTAMPTZ '2026-04-09 22:00:00+05'
       )
);

SELECT setval(
    pg_get_serial_sequence('public.bookings', 'id'),
    COALESCE((SELECT MAX(id) FROM public.bookings), 1),
    TRUE
);
