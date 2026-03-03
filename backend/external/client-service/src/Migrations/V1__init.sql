CREATE TABLE IF NOT EXISTS clients (
    id SERIAL PRIMARY KEY,
    first_name VARCHAR(100) NOT NULL,
    last_name VARCHAR(100) NOT NULL,
    created_on TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    birth_date DATE NOT NULL,
    identity_document_file_name VARCHAR(255) NULL,
    driver_license_file_name VARCHAR(255) NULL,
    related_user_id VARCHAR(64) NOT NULL,
    phone_number VARCHAR(32) NOT NULL,
    avatar_url VARCHAR(1024) NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_clients_related_user_id ON clients (related_user_id);
CREATE INDEX IF NOT EXISTS idx_clients_created_on ON clients (created_on DESC);
