ALTER TABLE clients
    ADD COLUMN IF NOT EXISTS provision_request_key VARCHAR(128);

CREATE UNIQUE INDEX IF NOT EXISTS uq_clients_provision_request_key
    ON clients (provision_request_key);
