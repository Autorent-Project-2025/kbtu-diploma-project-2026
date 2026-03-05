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
    'Demo',
    'Partner',
    NULL,
    'demo_partner_identity.pdf',
    DATE '2026-01-01',
    DATE '2030-01-01',
    '22222222-2222-2222-2222-222222222222',
    '+77010000002')
ON CONFLICT (related_user_id) DO NOTHING;
