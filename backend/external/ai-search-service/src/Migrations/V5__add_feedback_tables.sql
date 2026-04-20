-- Click-tracking table: captures which car a user clicked from an AI
-- recommendation result. Feeds the personalization loop and provides a
-- relevance signal for future re-ranking.
CREATE TABLE IF NOT EXISTS ai_recommendation_clicks (
    id              BIGSERIAL PRIMARY KEY,
    user_id         UUID,
    session_id      TEXT,
    prompt          TEXT NOT NULL,
    partner_car_id  BIGINT NOT NULL,
    position        INT NOT NULL,
    clicked_at      TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_ai_clicks_user_time
    ON ai_recommendation_clicks (user_id, clicked_at DESC);
CREATE INDEX IF NOT EXISTS idx_ai_clicks_car
    ON ai_recommendation_clicks (partner_car_id);

-- Per-user preference vector. Recomputed nightly from bookings + clicks.
-- Dimension matches ai_car_documents.vector_embedding (1024 for bge-m3).
CREATE TABLE IF NOT EXISTS user_embeddings (
    user_id            UUID PRIMARY KEY,
    vector_embedding   vector(1024) NOT NULL,
    sample_count       INT NOT NULL DEFAULT 0,
    refreshed_at       TIMESTAMPTZ NOT NULL DEFAULT now()
);
