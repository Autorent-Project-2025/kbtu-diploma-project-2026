ALTER TABLE public.partner_cars
    ADD COLUMN IF NOT EXISTS ownership_file_name VARCHAR(255);
