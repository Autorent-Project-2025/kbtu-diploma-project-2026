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
    'Miras',
    'Abdrakhmanov',
    NULL,
    'demo_partner_three_identity.pdf',
    DATE '2026-02-15',
    DATE '2031-02-15',
    '88888888-8888-8888-8888-888888888888',
    '+77010000005'
WHERE NOT EXISTS (
    SELECT 1
    FROM partners
    WHERE related_user_id = '88888888-8888-8888-8888-888888888888'
);
