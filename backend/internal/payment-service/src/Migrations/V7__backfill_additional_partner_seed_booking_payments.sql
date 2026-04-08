CREATE TEMP TABLE tmp_additional_seed_customer_payments ON COMMIT DROP AS
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
        (1, 4, 1580.50::numeric(18, 2), 12),
        (2, 5, 1566.00::numeric(18, 2), 10),
        (3, 6, 2299.50::numeric(18, 2), 6),
        (4, 7, 2725.00::numeric(18, 2), 8)
    ) AS seed(sort_order, partner_car_id, price_hour, billable_hours)
),
scheduled_bookings AS (
    SELECT
        12000 + (user_seed.sort_order * 10) + car_seed.sort_order AS booking_id,
        user_seed.user_id,
        '77777777-7777-7777-7777-777777777777'::uuid AS partner_user_id,
        car_seed.partner_car_id,
        car_seed.price_hour,
        (car_seed.price_hour * car_seed.billable_hours)::numeric(18, 2) AS gross_amount,
        car_seed.billable_hours,
        TIMESTAMPTZ '2026-03-18 09:00:00+05'
            + (((car_seed.sort_order - 1) * 3) + (user_seed.sort_order - 1)) * INTERVAL '1 day' AS start_time
    FROM seed_users user_seed
    CROSS JOIN seed_cars car_seed
)
SELECT
    booking.booking_id,
    booking.user_id,
    booking.partner_user_id,
    booking.partner_car_id,
    booking.price_hour,
    booking.gross_amount,
    0.1000::numeric(5, 4) AS platform_commission_rate,
    ROUND(booking.gross_amount * 0.10, 2) AS platform_commission_amount,
    ROUND(booking.gross_amount - ROUND(booking.gross_amount * 0.10, 2), 2) AS partner_amount,
    'KZT'::varchar(3) AS currency,
    booking.start_time - INTERVAL '5 days' AS created_at,
    booking.start_time + booking.billable_hours * INTERVAL '1 hour' AS available_at
FROM scheduled_bookings booking;

INSERT INTO public.partner_wallets (
    partner_user_id,
    currency,
    pending_amount,
    available_amount,
    reserved_amount,
    created_at,
    updated_at
)
SELECT
    payment.partner_user_id,
    payment.currency,
    0.00::numeric(18, 2),
    0.00::numeric(18, 2),
    0.00::numeric(18, 2),
    MIN(payment.created_at) AS created_at,
    MAX(payment.available_at) AS updated_at
FROM tmp_additional_seed_customer_payments payment
GROUP BY payment.partner_user_id, payment.currency
ON CONFLICT (partner_user_id) DO NOTHING;

CREATE TEMP TABLE tmp_inserted_additional_customer_payments ON COMMIT DROP AS
WITH payments_to_insert AS (
    SELECT
        seed.booking_id,
        seed.user_id,
        seed.partner_user_id,
        seed.partner_car_id,
        seed.price_hour,
        seed.gross_amount,
        seed.platform_commission_rate,
        seed.platform_commission_amount,
        seed.partner_amount,
        seed.currency,
        seed.created_at,
        seed.available_at,
        wallet.id AS partner_wallet_id
    FROM tmp_additional_seed_customer_payments seed
    JOIN public.partner_wallets wallet
        ON wallet.partner_user_id = seed.partner_user_id
    LEFT JOIN public.customer_payments existing
        ON existing.booking_id = seed.booking_id
    WHERE existing.id IS NULL
),
inserted_payments AS (
    INSERT INTO public.customer_payments (
        booking_id,
        user_id,
        partner_user_id,
        partner_car_id,
        price_hour,
        gross_amount,
        platform_commission_rate,
        platform_commission_amount,
        partner_amount,
        currency,
        status,
        created_at,
        updated_at,
        confirmed_at,
        available_at,
        canceled_at
    )
    SELECT
        payment.booking_id,
        payment.user_id,
        payment.partner_user_id,
        payment.partner_car_id,
        payment.price_hour,
        payment.gross_amount,
        payment.platform_commission_rate,
        payment.platform_commission_amount,
        payment.partner_amount,
        payment.currency,
        'available',
        payment.created_at,
        payment.available_at,
        payment.created_at,
        payment.available_at,
        NULL
    FROM payments_to_insert payment
    RETURNING
        id,
        booking_id,
        partner_user_id,
        partner_amount,
        currency
)
SELECT
    inserted.id AS customer_payment_id,
    payment.partner_wallet_id,
    inserted.booking_id,
    inserted.partner_user_id,
    inserted.partner_amount,
    inserted.currency,
    payment.created_at,
    payment.available_at
FROM inserted_payments inserted
JOIN payments_to_insert payment
    ON payment.booking_id = inserted.booking_id;

UPDATE public.partner_wallets wallet
SET
    available_amount = ROUND(wallet.available_amount + backfill.total_partner_amount, 2),
    updated_at = GREATEST(wallet.updated_at, backfill.last_available_at)
FROM (
    SELECT
        payment.partner_wallet_id,
        SUM(payment.partner_amount)::numeric(18, 2) AS total_partner_amount,
        MAX(payment.available_at) AS last_available_at
    FROM tmp_inserted_additional_customer_payments payment
    GROUP BY payment.partner_wallet_id
) backfill
WHERE wallet.id = backfill.partner_wallet_id;

INSERT INTO public.partner_ledger_entries (
    partner_wallet_id,
    booking_id,
    customer_payment_id,
    partner_payout_id,
    entry_type,
    bucket,
    amount_delta,
    currency,
    description,
    created_at
)
SELECT
    payment.partner_wallet_id,
    payment.booking_id,
    payment.customer_payment_id,
    NULL,
    'bookingpendingcredit',
    'pending',
    payment.partner_amount,
    payment.currency,
    'Booking ' || payment.booking_id || ' confirmed.',
    payment.created_at
FROM tmp_inserted_additional_customer_payments payment;

INSERT INTO public.partner_ledger_entries (
    partner_wallet_id,
    booking_id,
    customer_payment_id,
    partner_payout_id,
    entry_type,
    bucket,
    amount_delta,
    currency,
    description,
    created_at
)
SELECT
    payment.partner_wallet_id,
    payment.booking_id,
    payment.customer_payment_id,
    NULL,
    'bookingpendingrelease',
    'pending',
    ROUND(-payment.partner_amount, 2),
    payment.currency,
    'Booking ' || payment.booking_id || ' completed: pending release.',
    payment.available_at
FROM tmp_inserted_additional_customer_payments payment;

INSERT INTO public.partner_ledger_entries (
    partner_wallet_id,
    booking_id,
    customer_payment_id,
    partner_payout_id,
    entry_type,
    bucket,
    amount_delta,
    currency,
    description,
    created_at
)
SELECT
    payment.partner_wallet_id,
    payment.booking_id,
    payment.customer_payment_id,
    NULL,
    'bookingavailablecredit',
    'available',
    payment.partner_amount,
    payment.currency,
    'Booking ' || payment.booking_id || ' completed: available credit.',
    payment.available_at
FROM tmp_inserted_additional_customer_payments payment;
