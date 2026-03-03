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
