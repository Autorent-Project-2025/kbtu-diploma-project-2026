ALTER TABLE car_models
    ADD COLUMN IF NOT EXISTS market_value_kzt NUMERIC(14, 2),
    ADD COLUMN IF NOT EXISTS market_value_fetched_at TIMESTAMPTZ,
    ADD COLUMN IF NOT EXISTS market_value_source VARCHAR(64),
    ADD COLUMN IF NOT EXISTS market_value_source_url VARCHAR(2048),
    ADD COLUMN IF NOT EXISTS market_value_sample_count INT NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS market_value_filtered_sample_count INT NOT NULL DEFAULT 0,
    ADD COLUMN IF NOT EXISTS market_value_confidence VARCHAR(16),
    ADD COLUMN IF NOT EXISTS market_value_status VARCHAR(16),
    ADD COLUMN IF NOT EXISTS market_value_error TEXT;

UPDATE car_models
SET market_value_status = 'pending'
WHERE market_value_status IS NULL;
