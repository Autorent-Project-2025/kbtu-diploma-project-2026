ALTER TABLE users
    ADD COLUMN IF NOT EXISTS subject_type VARCHAR(50);

ALTER TABLE users
    ADD COLUMN IF NOT EXISTS actor_type VARCHAR(50);

UPDATE users
SET subject_type = 'user'
WHERE subject_type IS NULL
   OR btrim(subject_type) = '';

UPDATE users
SET actor_type = 'client'
WHERE actor_type IS NULL
   OR btrim(actor_type) = '';

ALTER TABLE users
    ALTER COLUMN subject_type SET DEFAULT 'user';

ALTER TABLE users
    ALTER COLUMN actor_type SET DEFAULT 'client';

ALTER TABLE users
    ALTER COLUMN subject_type SET NOT NULL;

ALTER TABLE users
    ALTER COLUMN actor_type SET NOT NULL;
