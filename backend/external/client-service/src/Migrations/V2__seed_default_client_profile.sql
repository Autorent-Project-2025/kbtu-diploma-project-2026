INSERT INTO clients (
    first_name,
    last_name,
    birth_date,
    identity_document_file_name,
    driver_license_file_name,
    related_user_id,
    phone_number,
    avatar_url)
VALUES (
    'Demo',
    'User',
    DATE '1998-01-01',
    NULL,
    NULL,
    '11111111-1111-1111-1111-111111111111',
    '+77010000001',
    NULL)
ON CONFLICT (related_user_id) DO NOTHING;
