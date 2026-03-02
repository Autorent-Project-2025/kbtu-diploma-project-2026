CREATE TABLE IF NOT EXISTS tickets (
    id UUID PRIMARY KEY,
    full_name VARCHAR(300) NOT NULL,
    email VARCHAR(255) NOT NULL,
    birth_date DATE NOT NULL,
    status INT NOT NULL,
    decision_reason VARCHAR(1000) NULL,
    created_at TIMESTAMPTZ NOT NULL,
    reviewed_by_manager_id UUID NULL,
    reviewed_at TIMESTAMPTZ NULL
);

CREATE INDEX IF NOT EXISTS idx_tickets_status ON tickets (status);
CREATE INDEX IF NOT EXISTS idx_tickets_email ON tickets (email);
CREATE INDEX IF NOT EXISTS idx_tickets_created_at ON tickets (created_at);
