CREATE TABLE IF NOT EXISTS ai_chat_histories (
    user_id text PRIMARY KEY,
    messages jsonb NOT NULL DEFAULT '[]'::jsonb,
    updated_at timestamptz NOT NULL DEFAULT now(),
    CONSTRAINT chk_ai_chat_histories_messages_array
        CHECK (jsonb_typeof(messages) = 'array')
);

CREATE INDEX IF NOT EXISTS idx_ai_chat_histories_updated_at
    ON ai_chat_histories (updated_at DESC);
