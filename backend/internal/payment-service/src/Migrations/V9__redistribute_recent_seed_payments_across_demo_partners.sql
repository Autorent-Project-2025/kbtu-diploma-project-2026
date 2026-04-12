CREATE TEMP TABLE tmp_recent_seed_payment_partner_reassignment (
    partner_car_id INT NOT NULL,
    new_partner_user_id UUID NOT NULL
) ON COMMIT DROP;

INSERT INTO tmp_recent_seed_payment_partner_reassignment (
    partner_car_id,
    new_partner_user_id
)
VALUES
    (8, '77777777-7777-7777-7777-777777777777'::uuid),
    (10, '88888888-8888-8888-8888-888888888888'::uuid),
    (9, '22222222-2222-2222-2222-222222222222'::uuid);

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
    reassignment.new_partner_user_id,
    'KZT',
    0.00::numeric(18, 2),
    0.00::numeric(18, 2),
    0.00::numeric(18, 2),
    NOW(),
    NOW()
FROM tmp_recent_seed_payment_partner_reassignment reassignment
ON CONFLICT (partner_user_id) DO NOTHING;

CREATE TEMP TABLE tmp_recent_seed_payment_moves ON COMMIT DROP AS
SELECT
    payment.id AS customer_payment_id,
    payment.booking_id,
    payment.partner_amount,
    payment.partner_user_id AS old_partner_user_id,
    reassignment.new_partner_user_id,
    new_wallet.id AS new_partner_wallet_id
FROM public.customer_payments payment
JOIN tmp_recent_seed_payment_partner_reassignment reassignment
    ON reassignment.partner_car_id = payment.partner_car_id
JOIN public.partner_wallets new_wallet
    ON new_wallet.partner_user_id = reassignment.new_partner_user_id
WHERE payment.booking_id BETWEEN 13011 AND 13033
  AND payment.partner_user_id IS DISTINCT FROM reassignment.new_partner_user_id;

UPDATE public.partner_wallets wallet
SET
    available_amount = ROUND(wallet.available_amount - delta.total_partner_amount, 2),
    updated_at = GREATEST(wallet.updated_at, NOW())
FROM (
    SELECT
        move.old_partner_user_id AS partner_user_id,
        SUM(move.partner_amount)::numeric(18, 2) AS total_partner_amount
    FROM tmp_recent_seed_payment_moves move
    GROUP BY move.old_partner_user_id
) delta
WHERE wallet.partner_user_id = delta.partner_user_id;

UPDATE public.partner_wallets wallet
SET
    available_amount = ROUND(wallet.available_amount + delta.total_partner_amount, 2),
    updated_at = GREATEST(wallet.updated_at, NOW())
FROM (
    SELECT
        move.new_partner_user_id AS partner_user_id,
        SUM(move.partner_amount)::numeric(18, 2) AS total_partner_amount
    FROM tmp_recent_seed_payment_moves move
    GROUP BY move.new_partner_user_id
) delta
WHERE wallet.partner_user_id = delta.partner_user_id;

UPDATE public.customer_payments payment
SET partner_user_id = move.new_partner_user_id
FROM tmp_recent_seed_payment_moves move
WHERE payment.id = move.customer_payment_id;

UPDATE public.partner_ledger_entries entry
SET partner_wallet_id = move.new_partner_wallet_id
FROM tmp_recent_seed_payment_moves move
WHERE entry.customer_payment_id = move.customer_payment_id
  AND entry.partner_wallet_id IS DISTINCT FROM move.new_partner_wallet_id;
