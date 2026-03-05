-- Post-refactor hardening: normalize user_id type and ensure image_id columns exist.

DO $$
DECLARE
    user_id_type TEXT;
BEGIN
    SELECT data_type
      INTO user_id_type
      FROM information_schema.columns
     WHERE table_schema = 'public'
       AND table_name = 'car_comments'
       AND column_name = 'user_id';

    IF user_id_type = 'integer' THEN
        ALTER TABLE public.car_comments
            ALTER COLUMN user_id TYPE VARCHAR(64)
            USING user_id::text;
    END IF;
END $$;

ALTER TABLE public.car_model_images
    ADD COLUMN IF NOT EXISTS image_id VARCHAR(255);

ALTER TABLE public.partner_car_images
    ADD COLUMN IF NOT EXISTS image_id VARCHAR(255);

CREATE INDEX IF NOT EXISTS idx_car_model_images_image_id
    ON public.car_model_images (image_id);

CREATE INDEX IF NOT EXISTS idx_partner_car_images_image_id
    ON public.partner_car_images (image_id);
