UPDATE partners
SET related_user_id = '77777777-7777-7777-7777-777777777777'
WHERE related_user_id = '44444444-4444-4444-4444-444444444444'
  AND owner_first_name = 'Ayan'
  AND owner_last_name = 'Tulegenov';

INSERT INTO partners (
    owner_first_name,
    owner_last_name,
    contract_file_name,
    owner_identity_file_name,
    registration_date,
    partnership_end_date,
    related_user_id,
    phone_number)
SELECT
    'Ayan',
    'Tulegenov',
    NULL,
    'demo_partner_two_identity.pdf',
    DATE '2026-02-01',
    DATE '2031-02-01',
    '77777777-7777-7777-7777-777777777777',
    '+77010000004'
WHERE NOT EXISTS (
    SELECT 1
    FROM partners
    WHERE related_user_id = '77777777-7777-7777-7777-777777777777'
);
