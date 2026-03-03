ALTER TABLE tickets
    ADD COLUMN IF NOT EXISTS first_name VARCHAR(100);

ALTER TABLE tickets
    ADD COLUMN IF NOT EXISTS last_name VARCHAR(100);

UPDATE tickets
SET
    first_name = CASE
        WHEN split_part(trim(full_name), ' ', 1) = '' THEN 'Unknown'
        ELSE split_part(trim(full_name), ' ', 1)
    END,
    last_name = CASE
        WHEN strpos(trim(full_name), ' ') = 0 THEN
            CASE
                WHEN split_part(trim(full_name), ' ', 1) = '' THEN 'Unknown'
                ELSE split_part(trim(full_name), ' ', 1)
            END
        ELSE ltrim(substr(trim(full_name), strpos(trim(full_name), ' ') + 1))
    END
WHERE first_name IS NULL OR last_name IS NULL;

ALTER TABLE tickets
    ALTER COLUMN first_name SET NOT NULL;

ALTER TABLE tickets
    ALTER COLUMN last_name SET NOT NULL;

ALTER TABLE tickets
    ADD COLUMN IF NOT EXISTS phone_number VARCHAR(32);

UPDATE tickets
SET phone_number = 'not-provided'
WHERE phone_number IS NULL;

ALTER TABLE tickets
    ALTER COLUMN phone_number SET NOT NULL;

ALTER TABLE tickets
    ADD COLUMN IF NOT EXISTS identity_document_file_name VARCHAR(255) NULL;

ALTER TABLE tickets
    ADD COLUMN IF NOT EXISTS driver_license_file_name VARCHAR(255) NULL;

ALTER TABLE tickets
    ADD COLUMN IF NOT EXISTS avatar_url VARCHAR(1024) NULL;
