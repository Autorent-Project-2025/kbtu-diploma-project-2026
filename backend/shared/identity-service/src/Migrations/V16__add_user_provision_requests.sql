CREATE TABLE IF NOT EXISTS user_provision_requests (
    id UUID PRIMARY KEY,
    request_key VARCHAR(128) NOT NULL,
    user_id UUID NOT NULL REFERENCES users (id) ON DELETE CASCADE,
    full_name VARCHAR(300) NOT NULL,
    email VARCHAR(255) NOT NULL,
    birth_date DATE NOT NULL,
    subject_type VARCHAR(64) NOT NULL,
    actor_type VARCHAR(64) NOT NULL,
    created_at_utc TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX IF NOT EXISTS uq_user_provision_requests_request_key
    ON user_provision_requests (request_key);

CREATE INDEX IF NOT EXISTS idx_user_provision_requests_user_id
    ON user_provision_requests (user_id);
