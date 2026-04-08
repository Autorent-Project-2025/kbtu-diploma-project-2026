INSERT INTO partners (
    owner_first_name,
    owner_last_name,
    contract_file_name,
    owner_identity_file_name,
    registration_date,
    partnership_end_date,
    related_user_id,
    phone_number)
VALUES (
    'Ayan',
    'Tulegenov',
    NULL,
    'demo_partner_two_identity.pdf',
    DATE '2026-02-01',
    DATE '2031-02-01',
    '44444444-4444-4444-4444-444444444444',
    '+77010000004')
ON CONFLICT (related_user_id) DO NOTHING;
