UPDATE tickets
SET data = CASE
    WHEN ticket_type = 1 THEN
        jsonb_strip_nulls(
            jsonb_build_object(
                '$type', 'client',
                'firstName', COALESCE(NULLIF(data->>'firstName', ''), split_part(trim(COALESCE(data->>'fullName', '')), ' ', 1), 'Unknown'),
                'lastName', COALESCE(
                    NULLIF(data->>'lastName', ''),
                    CASE
                        WHEN strpos(trim(COALESCE(data->>'fullName', '')), ' ') = 0 THEN split_part(trim(COALESCE(data->>'fullName', '')), ' ', 1)
                        ELSE ltrim(substr(trim(COALESCE(data->>'fullName', '')), strpos(trim(COALESCE(data->>'fullName', '')), ' ') + 1))
                    END,
                    'Unknown'
                ),
                'fullName', COALESCE(NULLIF(data->>'fullName', ''), concat_ws(' ', NULLIF(data->>'firstName', ''), NULLIF(data->>'lastName', ''))),
                'birthDate', to_jsonb(COALESCE(NULLIF(data->>'birthDate', ''), created_at::date::text)),
                'phoneNumber', COALESCE(NULLIF(data->>'phoneNumber', ''), 'not-provided'),
                'identityDocumentFileName', NULLIF(data->>'identityDocumentFileName', ''),
                'driverLicenseFileName', NULLIF(data->>'driverLicenseFileName', ''),
                'avatarUrl', NULLIF(data->>'avatarUrl', ''),
                'decisionReason', NULLIF(data->>'decisionReason', ''),
                'reviewedByManagerId', NULLIF(data->>'reviewedByManagerId', ''),
                'reviewedAt', NULLIF(data->>'reviewedAt', '')
            )
        )
    WHEN ticket_type = 2 THEN
        jsonb_strip_nulls(
            jsonb_build_object(
                '$type', 'partner',
                'firstName', COALESCE(NULLIF(data->>'firstName', ''), split_part(trim(COALESCE(data->>'fullName', '')), ' ', 1), 'Unknown'),
                'lastName', COALESCE(
                    NULLIF(data->>'lastName', ''),
                    CASE
                        WHEN strpos(trim(COALESCE(data->>'fullName', '')), ' ') = 0 THEN split_part(trim(COALESCE(data->>'fullName', '')), ' ', 1)
                        ELSE ltrim(substr(trim(COALESCE(data->>'fullName', '')), strpos(trim(COALESCE(data->>'fullName', '')), ' ') + 1))
                    END,
                    'Unknown'
                ),
                'fullName', COALESCE(NULLIF(data->>'fullName', ''), concat_ws(' ', NULLIF(data->>'firstName', ''), NULLIF(data->>'lastName', ''))),
                'phoneNumber', COALESCE(NULLIF(data->>'phoneNumber', ''), 'not-provided'),
                'identityDocumentFileName', NULLIF(data->>'identityDocumentFileName', ''),
                'companyName', COALESCE(NULLIF(data->>'companyName', ''), NULLIF(data->>'fullName', ''), email),
                'contactEmail', COALESCE(NULLIF(data->>'contactEmail', ''), email),
                'decisionReason', NULLIF(data->>'decisionReason', ''),
                'reviewedByManagerId', NULLIF(data->>'reviewedByManagerId', ''),
                'reviewedAt', NULLIF(data->>'reviewedAt', '')
            )
        )
    ELSE data
END
WHERE COALESCE(data->>'$type', '') = '';
