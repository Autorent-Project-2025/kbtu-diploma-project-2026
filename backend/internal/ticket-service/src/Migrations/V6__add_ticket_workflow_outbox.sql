CREATE TABLE IF NOT EXISTS ticket_workflow_outbox_messages (
    id BIGSERIAL PRIMARY KEY,
    ticket_id UUID NOT NULL REFERENCES tickets (id) ON DELETE CASCADE,
    event_key VARCHAR(200) NOT NULL,
    event_type VARCHAR(100) NOT NULL,
    payload JSONB NOT NULL,
    attempt_count INTEGER NOT NULL DEFAULT 0,
    last_error TEXT NULL,
    created_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    next_attempt_at TIMESTAMPTZ NOT NULL DEFAULT NOW(),
    processed_at TIMESTAMPTZ NULL,
    locked_until TIMESTAMPTZ NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS uq_ticket_workflow_outbox_event_key
    ON ticket_workflow_outbox_messages (event_key);

CREATE INDEX IF NOT EXISTS idx_ticket_workflow_outbox_dispatch
    ON ticket_workflow_outbox_messages (processed_at, next_attempt_at, id);

CREATE INDEX IF NOT EXISTS idx_ticket_workflow_outbox_ticket_id
    ON ticket_workflow_outbox_messages (ticket_id);
