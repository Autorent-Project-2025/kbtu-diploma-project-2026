CREATE TABLE IF NOT EXISTS partners (
    id SERIAL PRIMARY KEY,
    owner_first_name VARCHAR(100) NOT NULL,
    owner_last_name VARCHAR(100) NOT NULL,
    created_on TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    contract_file_name VARCHAR(255) NOT NULL,
    owner_identity_file_name VARCHAR(255) NOT NULL,
    registration_date DATE NOT NULL,
    partnership_end_date DATE NOT NULL,
    related_user_id VARCHAR(64) NOT NULL,
    phone_number VARCHAR(32) NOT NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS ux_partners_related_user_id ON partners (related_user_id);
CREATE INDEX IF NOT EXISTS idx_partners_created_on ON partners (created_on DESC);
