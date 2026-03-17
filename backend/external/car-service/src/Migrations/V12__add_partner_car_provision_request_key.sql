ALTER TABLE partner_cars
    ADD COLUMN IF NOT EXISTS provision_request_key VARCHAR(128);

CREATE UNIQUE INDEX IF NOT EXISTS uq_partner_cars_provision_request_key
    ON partner_cars (provision_request_key);
