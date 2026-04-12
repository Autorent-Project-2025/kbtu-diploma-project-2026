ALTER TABLE partner_cars ADD COLUMN IF NOT EXISTS is_active BOOLEAN NOT NULL DEFAULT TRUE;

CREATE INDEX IF NOT EXISTS idx_partner_cars_is_active ON partner_cars (is_active);
