CREATE TEMP TABLE tmp_recent_seed_booking_partner_reassignment (
    partner_car_id INT NOT NULL,
    new_partner_user_id UUID NOT NULL,
    new_partner_name VARCHAR(255) NOT NULL
) ON COMMIT DROP;

INSERT INTO tmp_recent_seed_booking_partner_reassignment (
    partner_car_id,
    new_partner_user_id,
    new_partner_name
)
VALUES
    (8, '77777777-7777-7777-7777-777777777777'::uuid, 'Ayan Tulegenov'),
    (10, '88888888-8888-8888-8888-888888888888'::uuid, 'Miras Abdrakhmanov'),
    (9, '22222222-2222-2222-2222-222222222222'::uuid, 'Demo Partner');

UPDATE public.bookings booking
SET
    partner_user_id = reassignment.new_partner_user_id,
    partner_name = reassignment.new_partner_name
FROM tmp_recent_seed_booking_partner_reassignment reassignment
WHERE booking.partner_car_id = reassignment.partner_car_id
  AND booking.id BETWEEN 13011 AND 13033
  AND (
      booking.partner_user_id IS DISTINCT FROM reassignment.new_partner_user_id
      OR booking.partner_name IS DISTINCT FROM reassignment.new_partner_name
  );
