UPDATE public.partner_cars
SET partner_user_id = '77777777-7777-7777-7777-777777777777'::uuid
WHERE license_plate IN ('KZP44411', 'KZP44412', 'KZP44413', 'KZP44414');
