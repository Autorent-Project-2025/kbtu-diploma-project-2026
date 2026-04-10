INSERT INTO tickets (id, ticket_type, status, email, created_at, data)
SELECT
    '99999999-9999-9999-9999-999999999993'::uuid,
    2,
    1,
    'pending.partner@autorent.local',
    TIMESTAMPTZ '2026-04-10 11:00:00+00',
    jsonb_build_object(
        '$type', 'partner',
        'firstName', 'Dias',
        'lastName', 'Nurgaliyev',
        'fullName', 'Dias Nurgaliyev',
        'phoneNumber', '+77017770102',
        'identityDocumentFileName', 'fake-id.png',
        'companyName', 'Nomad Fleet',
        'contactEmail', 'pending.partner@autorent.local'
    )
WHERE NOT EXISTS (
    SELECT 1
    FROM tickets
    WHERE id = '99999999-9999-9999-9999-999999999993'::uuid
       OR (email = 'pending.partner@autorent.local' AND ticket_type = 2 AND status = 1)
);
