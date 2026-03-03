ALTER TABLE tickets
    ADD COLUMN IF NOT EXISTS ticket_type INT NOT NULL DEFAULT 1;

ALTER TABLE tickets
    ALTER COLUMN birth_date DROP NOT NULL;

CREATE INDEX IF NOT EXISTS idx_tickets_ticket_type ON tickets (ticket_type);
