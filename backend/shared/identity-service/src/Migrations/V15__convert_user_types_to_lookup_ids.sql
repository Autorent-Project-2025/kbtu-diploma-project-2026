CREATE EXTENSION IF NOT EXISTS pgcrypto;

ALTER TABLE users
    DROP CONSTRAINT IF EXISTS fk_users_subject_type;

ALTER TABLE users
    DROP CONSTRAINT IF EXISTS fk_users_actor_type;

ALTER TABLE subject_types
    ADD COLUMN IF NOT EXISTS id UUID;

ALTER TABLE actor_types
    ADD COLUMN IF NOT EXISTS id UUID;

UPDATE subject_types
SET id = CASE name
    WHEN 'user' THEN '00000000-0000-0000-0000-000000000101'::uuid
    WHEN 'service' THEN '00000000-0000-0000-0000-000000000102'::uuid
    WHEN 'api_key' THEN '00000000-0000-0000-0000-000000000103'::uuid
    WHEN 'system' THEN '00000000-0000-0000-0000-000000000104'::uuid
    ELSE COALESCE(id, gen_random_uuid())
END;

UPDATE actor_types
SET id = CASE name
    WHEN 'client' THEN '00000000-0000-0000-0000-000000000201'::uuid
    WHEN 'partner' THEN '00000000-0000-0000-0000-000000000202'::uuid
    WHEN 'admin' THEN '00000000-0000-0000-0000-000000000203'::uuid
    WHEN 'internal' THEN '00000000-0000-0000-0000-000000000204'::uuid
    ELSE COALESCE(id, gen_random_uuid())
END;

ALTER TABLE subject_types
    DROP CONSTRAINT IF EXISTS subject_types_pkey;

ALTER TABLE actor_types
    DROP CONSTRAINT IF EXISTS actor_types_pkey;

ALTER TABLE subject_types
    ALTER COLUMN id SET NOT NULL;

ALTER TABLE actor_types
    ALTER COLUMN id SET NOT NULL;

ALTER TABLE subject_types
    ADD CONSTRAINT subject_types_pkey PRIMARY KEY (id);

ALTER TABLE actor_types
    ADD CONSTRAINT actor_types_pkey PRIMARY KEY (id);

ALTER TABLE subject_types
    DROP CONSTRAINT IF EXISTS uq_subject_types_name;

ALTER TABLE actor_types
    DROP CONSTRAINT IF EXISTS uq_actor_types_name;

ALTER TABLE subject_types
    ADD CONSTRAINT uq_subject_types_name UNIQUE (name);

ALTER TABLE actor_types
    ADD CONSTRAINT uq_actor_types_name UNIQUE (name);

ALTER TABLE subject_types
    ALTER COLUMN id SET DEFAULT gen_random_uuid();

ALTER TABLE actor_types
    ALTER COLUMN id SET DEFAULT gen_random_uuid();

ALTER TABLE users
    ADD COLUMN IF NOT EXISTS subject_type_id UUID;

ALTER TABLE users
    ADD COLUMN IF NOT EXISTS actor_type_id UUID;

UPDATE users user_entity
SET subject_type_id = subject_type_entity.id
FROM subject_types subject_type_entity
WHERE subject_type_entity.name = user_entity.subject_type;

UPDATE users user_entity
SET actor_type_id = actor_type_entity.id
FROM actor_types actor_type_entity
WHERE actor_type_entity.name = user_entity.actor_type;

UPDATE users
SET subject_type_id = '00000000-0000-0000-0000-000000000101'::uuid
WHERE subject_type_id IS NULL;

UPDATE users
SET actor_type_id = '00000000-0000-0000-0000-000000000201'::uuid
WHERE actor_type_id IS NULL;

ALTER TABLE users
    ALTER COLUMN subject_type_id SET DEFAULT '00000000-0000-0000-0000-000000000101'::uuid;

ALTER TABLE users
    ALTER COLUMN actor_type_id SET DEFAULT '00000000-0000-0000-0000-000000000201'::uuid;

ALTER TABLE users
    ALTER COLUMN subject_type_id SET NOT NULL;

ALTER TABLE users
    ALTER COLUMN actor_type_id SET NOT NULL;

ALTER TABLE users
    DROP CONSTRAINT IF EXISTS fk_users_subject_type_id;

ALTER TABLE users
    ADD CONSTRAINT fk_users_subject_type_id
        FOREIGN KEY (subject_type_id) REFERENCES subject_types(id);

ALTER TABLE users
    DROP CONSTRAINT IF EXISTS fk_users_actor_type_id;

ALTER TABLE users
    ADD CONSTRAINT fk_users_actor_type_id
        FOREIGN KEY (actor_type_id) REFERENCES actor_types(id);

CREATE INDEX IF NOT EXISTS idx_users_subject_type_id ON users (subject_type_id);
CREATE INDEX IF NOT EXISTS idx_users_actor_type_id ON users (actor_type_id);

DROP INDEX IF EXISTS idx_users_subject_type;
DROP INDEX IF EXISTS idx_users_actor_type;

ALTER TABLE users
    DROP COLUMN IF EXISTS subject_type;

ALTER TABLE users
    DROP COLUMN IF EXISTS actor_type;
