UPDATE tickets
SET data = jsonb_set(
    jsonb_set(
        jsonb_set(
            data,
            '{firstName}',
            to_jsonb('Madina'::text),
            TRUE
        ),
        '{lastName}',
        to_jsonb('Zhaksylykova'::text),
        TRUE
    ),
    '{fullName}',
    to_jsonb('Madina Zhaksylykova'::text),
    TRUE
)
WHERE id = '99999999-9999-9999-9999-999999999991'::uuid
  AND ticket_type = 1;
