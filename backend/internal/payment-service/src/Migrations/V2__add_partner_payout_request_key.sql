ALTER TABLE IF EXISTS public.partner_payouts
    ADD COLUMN IF NOT EXISTS request_key VARCHAR(128);

UPDATE public.partner_payouts
SET request_key = 'legacy-' || id::text
WHERE request_key IS NULL;

ALTER TABLE public.partner_payouts
    ALTER COLUMN request_key SET NOT NULL;

CREATE UNIQUE INDEX IF NOT EXISTS ux_partner_payouts_request_key
    ON public.partner_payouts (request_key);

CREATE INDEX IF NOT EXISTS idx_partner_payouts_partner_user_requested_at
    ON public.partner_payouts (partner_user_id, requested_at DESC);
