CREATE TABLE IF NOT EXISTS subject_types (
    name VARCHAR(50) PRIMARY KEY,
    description VARCHAR(255) NOT NULL
);

CREATE TABLE IF NOT EXISTS actor_types (
    name VARCHAR(50) PRIMARY KEY,
    description VARCHAR(255) NOT NULL
);

INSERT INTO subject_types (name, description)
VALUES
    ('user', 'Authenticated end user'),
    ('service', 'Service-to-service subject'),
    ('api_key', 'API key subject'),
    ('system', 'Internal system subject')
ON CONFLICT (name) DO UPDATE
SET description = EXCLUDED.description;

INSERT INTO actor_types (name, description)
VALUES
    ('client', 'Client domain actor'),
    ('partner', 'Partner domain actor'),
    ('admin', 'Administrative actor'),
    ('internal', 'Internal employee or service actor')
ON CONFLICT (name) DO UPDATE
SET description = EXCLUDED.description;

UPDATE users user_entity
SET subject_type = 'user'
WHERE user_entity.subject_type IS NULL
   OR btrim(user_entity.subject_type) = ''
   OR NOT EXISTS (
       SELECT 1
       FROM subject_types subject_type_entity
       WHERE subject_type_entity.name = user_entity.subject_type
   );

UPDATE users user_entity
SET actor_type = 'client'
WHERE user_entity.actor_type IS NULL
   OR btrim(user_entity.actor_type) = ''
   OR NOT EXISTS (
       SELECT 1
       FROM actor_types actor_type_entity
       WHERE actor_type_entity.name = user_entity.actor_type
   );

ALTER TABLE users
    DROP CONSTRAINT IF EXISTS fk_users_subject_type;

ALTER TABLE users
    ADD CONSTRAINT fk_users_subject_type
        FOREIGN KEY (subject_type) REFERENCES subject_types(name);

ALTER TABLE users
    DROP CONSTRAINT IF EXISTS fk_users_actor_type;

ALTER TABLE users
    ADD CONSTRAINT fk_users_actor_type
        FOREIGN KEY (actor_type) REFERENCES actor_types(name);

CREATE INDEX IF NOT EXISTS idx_users_subject_type ON users (subject_type);
CREATE INDEX IF NOT EXISTS idx_users_actor_type ON users (actor_type);
