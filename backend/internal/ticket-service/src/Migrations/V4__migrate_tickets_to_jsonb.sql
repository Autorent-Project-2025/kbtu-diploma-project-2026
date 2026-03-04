ALTER TABLE tickets
    ADD COLUMN IF NOT EXISTS data JSONB;

UPDATE tickets
SET data = jsonb_strip_nulls(
    jsonb_build_object(
        'firstName', COALESCE(first_name, split_part(trim(full_name), ' ', 1)),
        'lastName', COALESCE(
            last_name,
            CASE
                WHEN strpos(trim(full_name), ' ') = 0 THEN split_part(trim(full_name), ' ', 1)
                ELSE ltrim(substr(trim(full_name), strpos(trim(full_name), ' ') + 1))
            END
        ),
        'fullName', COALESCE(full_name, concat_ws(' ', first_name, last_name)),
        'birthDate', birth_date,
        'phoneNumber', phone_number,
        'identityDocumentFileName', identity_document_file_name,
        'driverLicenseFileName', driver_license_file_name,
        'avatarUrl', avatar_url,
        'decisionReason', decision_reason,
        'reviewedByManagerId', reviewed_by_manager_id,
        'reviewedAt', reviewed_at
    )
)
WHERE data IS NULL;

UPDATE tickets
SET data = '{}'::jsonb
WHERE data IS NULL;

ALTER TABLE tickets
    ALTER COLUMN data SET NOT NULL;

ALTER TABLE tickets
    ALTER COLUMN ticket_type DROP DEFAULT;

ALTER TABLE tickets
    DROP COLUMN IF EXISTS first_name,
    DROP COLUMN IF EXISTS last_name,
    DROP COLUMN IF EXISTS full_name,
    DROP COLUMN IF EXISTS birth_date,
    DROP COLUMN IF EXISTS phone_number,
    DROP COLUMN IF EXISTS identity_document_file_name,
    DROP COLUMN IF EXISTS driver_license_file_name,
    DROP COLUMN IF EXISTS avatar_url,
    DROP COLUMN IF EXISTS decision_reason,
    DROP COLUMN IF EXISTS reviewed_by_manager_id,
    DROP COLUMN IF EXISTS reviewed_at;
