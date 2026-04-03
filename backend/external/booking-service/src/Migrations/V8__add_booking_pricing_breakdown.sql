ALTER TABLE bookings
    ADD COLUMN IF NOT EXISTS pricing_breakdown JSONB;
