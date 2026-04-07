INSERT INTO clients (
    first_name,
    last_name,
    birth_date,
    identity_document_file_name,
    driver_license_file_name,
    related_user_id,
    phone_number,
    avatar_url
)
SELECT
    seed.first_name,
    seed.last_name,
    seed.birth_date,
    NULL,
    NULL,
    seed.related_user_id,
    seed.phone_number,
    NULL
FROM (VALUES
    ('44444444-4444-4444-4444-444444444444', 'Aruzhan', 'Sarsenova', DATE '1999-03-14', '+77010000011'),
    ('55555555-5555-5555-5555-555555555555', 'Dias', 'Nurgaliyev', DATE '1997-07-22', '+77010000012'),
    ('66666666-6666-6666-6666-666666666666', 'Aigerim', 'Toktarova', DATE '2000-11-05', '+77010000013')
) AS seed(related_user_id, first_name, last_name, birth_date, phone_number)
ON CONFLICT (related_user_id) DO NOTHING;
