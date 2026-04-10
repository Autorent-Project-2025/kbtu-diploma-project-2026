INSERT INTO tickets (id, ticket_type, status, email, created_at, data)
SELECT
    '99999999-9999-9999-9999-999999999991'::uuid,
    1,
    1,
    'pending.client@autorent.local',
    TIMESTAMPTZ '2026-04-10 09:00:00+00',
    jsonb_build_object(
        '$type', 'client',
        'firstName', 'Aruzhan',
        'lastName', 'Sarsenova',
        'fullName', 'Aruzhan Sarsenova',
        'birthDate', '2001-04-17',
        'phoneNumber', '+77015550101',
        'identityDocumentFileName', 'fake-id.png',
        'driverLicenseFileName', 'fake-driver-license.png'
    )
WHERE NOT EXISTS (
    SELECT 1
    FROM tickets
    WHERE id = '99999999-9999-9999-9999-999999999991'::uuid
       OR (email = 'pending.client@autorent.local' AND ticket_type = 1 AND status = 1)
);
