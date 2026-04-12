CREATE TABLE IF NOT EXISTS brand_model_aliases (
    id serial PRIMARY KEY,
    alias text NOT NULL,
    canonical_brand text NOT NULL,
    canonical_model text NULL
);

CREATE UNIQUE INDEX IF NOT EXISTS idx_brand_model_aliases_alias ON brand_model_aliases (lower(alias));

-- Seed common cyrillic aliases for existing catalog entries
INSERT INTO brand_model_aliases (alias, canonical_brand, canonical_model) VALUES
    ('шевроле', 'chevrolet', NULL),
    ('кобальт', 'chevrolet', 'cobalt'),
    ('тойота', 'toyota', NULL),
    ('камри', 'toyota', 'camry'),
    ('королла', 'toyota', 'corolla'),
    ('супра', 'toyota', 'supra'),
    ('ниссан', 'nissan', NULL),
    ('скайлайн', 'nissan', 'skyline'),
    ('мазда', 'mazda', NULL),
    ('ауди', 'audi', NULL),
    ('мерседес', 'mercedes', NULL),
    ('бмв', 'bmw', NULL),
    ('лексус', 'lexus', NULL),
    ('киа', 'kia', NULL)
ON CONFLICT DO NOTHING;
