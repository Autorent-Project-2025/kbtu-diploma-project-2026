CREATE TEMP TABLE tmp_recent_seed_partner_car_reassignment (
    license_plate VARCHAR(20) NOT NULL,
    new_partner_user_id UUID NOT NULL
) ON COMMIT DROP;

INSERT INTO tmp_recent_seed_partner_car_reassignment (
    license_plate,
    new_partner_user_id
)
VALUES
    ('KZP88811', '77777777-7777-7777-7777-777777777777'::uuid),
    ('KZP88812', '88888888-8888-8888-8888-888888888888'::uuid),
    ('KZP88813', '22222222-2222-2222-2222-222222222222'::uuid);

UPDATE public.partner_cars partner_car
SET partner_user_id = reassignment.new_partner_user_id
FROM tmp_recent_seed_partner_car_reassignment reassignment
WHERE partner_car.license_plate = reassignment.license_plate
  AND partner_car.partner_user_id IS DISTINCT FROM reassignment.new_partner_user_id;
