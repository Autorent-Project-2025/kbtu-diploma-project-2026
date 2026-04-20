-- Switch from truncated 128-dim embeddings to full 1024-dim (bge-m3 multilingual).
-- Existing embeddings are padded-zero legacy data; they will be overwritten
-- on the next reindex pass (AUTO_INDEX_ON_STARTUP=true).

DROP INDEX IF EXISTS idx_ai_car_documents_vector;

ALTER TABLE ai_car_documents
    ALTER COLUMN vector_embedding TYPE vector(1024)
    USING array_fill(0::real, ARRAY[1024])::vector(1024);

CREATE INDEX idx_ai_car_documents_vector
    ON ai_car_documents
    USING ivfflat (vector_embedding vector_cosine_ops)
    WITH (lists = 100);
