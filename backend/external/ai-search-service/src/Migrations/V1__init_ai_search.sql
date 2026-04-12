CREATE EXTENSION IF NOT EXISTS vector;

CREATE TABLE IF NOT EXISTS ai_car_documents (
    partner_car_id integer PRIMARY KEY,
    car_model_id integer NOT NULL,
    partner_user_id uuid NOT NULL,
    carrier_name text NULL,
    brand text NOT NULL,
    model text NOT NULL,
    year integer NOT NULL,
    title text NOT NULL,
    description text NULL,
    color text NULL,
    transmission text NULL,
    fuel_type text NULL,
    engine text NULL,
    seats integer NULL,
    price_hour numeric(12, 2) NULL,
    price_day numeric(12, 2) NULL,
    rating numeric(4, 2) NULL,
    ratings_count integer NOT NULL DEFAULT 0,
    image_url text NULL,
    details_url text NOT NULL,
    booking_url text NOT NULL,
    tags jsonb NOT NULL DEFAULT '[]'::jsonb,
    searchable_text text NOT NULL,
    vector_embedding vector(128) NOT NULL,
    lexical_document tsvector GENERATED ALWAYS AS (
        to_tsvector('simple', coalesce(title, '') || ' ' || coalesce(searchable_text, ''))
    ) STORED,
    updated_at timestamptz NOT NULL DEFAULT now()
);

CREATE INDEX IF NOT EXISTS idx_ai_car_documents_lexical
    ON ai_car_documents
    USING gin (lexical_document);

CREATE INDEX IF NOT EXISTS idx_ai_car_documents_partner_user
    ON ai_car_documents (partner_user_id);

CREATE INDEX IF NOT EXISTS idx_ai_car_documents_price_hour
    ON ai_car_documents (price_hour);

CREATE INDEX IF NOT EXISTS idx_ai_car_documents_vector
    ON ai_car_documents
    USING ivfflat (vector_embedding vector_cosine_ops)
    WITH (lists = 100);
