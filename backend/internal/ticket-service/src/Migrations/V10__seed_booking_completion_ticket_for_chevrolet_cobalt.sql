INSERT INTO tickets (id, ticket_type, status, email, created_at, data)
SELECT
    '99999999-9999-9999-9999-999999999994'::uuid,
    4,
    1,
    'client1@autorent.local',
    TIMESTAMPTZ '2026-04-09 22:10:00+05',
    jsonb_build_object(
        '$type', 'booking-completion',
        'firstName', 'Aruzhan',
        'lastName', 'Sarsenova',
        'fullName', 'Aruzhan Sarsenova',
        'phoneNumber', '+77010000011',
        'bookingId', 15041,
        'plannedStartTime', '2026-04-09T10:00:00+05:00',
        'plannedEndTime', '2026-04-09T22:00:00+05:00',
        'tripStartedAt', '2026-04-09T10:00:00+05:00',
        'tripCompletedAt', '2026-04-09T22:00:00+05:00',
        'completionPhotos', jsonb_build_array(
            jsonb_build_object(
                'slot', 'front',
                'fileName', 'chevrolet-cobalt-2020-booking-finish-front.png'
            ),
            jsonb_build_object(
                'slot', 'back',
                'fileName', 'chevrolet-cobalt-2020-booking-finish-back.png'
            ),
            jsonb_build_object(
                'slot', 'side_left',
                'fileName', 'chevrolet-cobalt-2020-booking-finish-side-left.png'
            ),
            jsonb_build_object(
                'slot', 'side_right',
                'fileName', 'chevrolet-cobalt-2020-booking-finish-side-right.png'
            ),
            jsonb_build_object(
                'slot', 'interior',
                'fileName', 'chevrolet-cobalt-2020-booking-finish-interior.png'
            )
        )
    )
WHERE NOT EXISTS (
    SELECT 1
    FROM tickets
    WHERE id = '99999999-9999-9999-9999-999999999994'::uuid
       OR (ticket_type = 4 AND COALESCE((data->>'bookingId')::int, 0) = 15041)
);
