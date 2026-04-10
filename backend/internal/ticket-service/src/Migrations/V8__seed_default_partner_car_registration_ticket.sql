INSERT INTO tickets (id, ticket_type, status, email, created_at, data)
SELECT
    '99999999-9999-9999-9999-999999999992'::uuid,
    3,
    1,
    'partner@autorent.local',
    TIMESTAMPTZ '2026-04-10 10:00:00+00',
    jsonb_build_object(
        '$type', 'partner-car',
        'firstName', 'Demo',
        'lastName', 'Partner',
        'fullName', 'Demo Partner',
        'phoneNumber', '+77010000002',
        'relatedPartnerUserId', '22222222-2222-2222-2222-222222222222',
        'carBrand', 'Chevrolet',
        'carModel', 'Cobalt',
        'carYear', 2020,
        'licensePlate', 'KZT22277',
        'ownershipDocumentFileName', 'fake-car-own-document.pdf',
        'carImages', jsonb_build_array(
            jsonb_build_object(
                'imageId', 'cars/Chevrolet Cobalt 2020/1/front.png',
                'imageUrl', 'http://localhost:9186/internal/public/cars/Chevrolet%20Cobalt%202020/1/front.png'
            ),
            jsonb_build_object(
                'imageId', 'cars/Chevrolet Cobalt 2020/1/back.png',
                'imageUrl', 'http://localhost:9186/internal/public/cars/Chevrolet%20Cobalt%202020/1/back.png'
            )
        )
    )
WHERE NOT EXISTS (
    SELECT 1
    FROM tickets
    WHERE id = '99999999-9999-9999-9999-999999999992'::uuid
       OR (
            ticket_type = 3
        AND COALESCE(data->>'relatedPartnerUserId', '') = '22222222-2222-2222-2222-222222222222'
        AND COALESCE(data->>'licensePlate', '') = 'KZT22277'
       )
);
