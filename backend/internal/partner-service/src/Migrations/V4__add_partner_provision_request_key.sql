ALTER TABLE partners
    ADD COLUMN IF NOT EXISTS provision_request_key VARCHAR(128);

CREATE UNIQUE INDEX IF NOT EXISTS uq_partners_provision_request_key
    ON partners (provision_request_key);
