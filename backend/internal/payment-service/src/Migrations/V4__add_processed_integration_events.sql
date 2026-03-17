CREATE TABLE IF NOT EXISTS processed_integration_events (
    id BIGSERIAL PRIMARY KEY,
    event_id VARCHAR(200) NOT NULL,
    routing_key VARCHAR(200) NOT NULL,
    processed_at TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE UNIQUE INDEX IF NOT EXISTS uq_processed_integration_events_event_id
    ON processed_integration_events (event_id);
