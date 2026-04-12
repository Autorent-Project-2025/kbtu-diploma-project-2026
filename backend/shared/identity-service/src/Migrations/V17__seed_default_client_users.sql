WITH seed_users AS (
    SELECT *
    FROM (VALUES
        ('44444444-4444-4444-4444-444444444444'::uuid, 'demo_client_1', 'client1@autorent.local', 'DemoClient123!'),
        ('55555555-5555-5555-5555-555555555555'::uuid, 'demo_client_2', 'client2@autorent.local', 'DemoClient123!'),
        ('66666666-6666-6666-6666-666666666666'::uuid, 'demo_client_3', 'client3@autorent.local', 'DemoClient123!')
    ) AS seed(id, username, email, plain_password)
)
INSERT INTO users (id, username, email, password_hash, is_active, subject_type_id, actor_type_id)
SELECT
    seed.id,
    seed.username,
    seed.email,
    crypt(seed.plain_password, gen_salt('bf')),
    TRUE,
    '00000000-0000-0000-0000-000000000101'::uuid,
    '00000000-0000-0000-0000-000000000201'::uuid
FROM seed_users seed
WHERE NOT EXISTS (
    SELECT 1
    FROM users existing
    WHERE existing.id = seed.id
       OR existing.email = seed.email
       OR existing.username = seed.username
);

WITH seed_users AS (
    SELECT *
    FROM (VALUES
        ('44444444-4444-4444-4444-444444444444'::uuid, 'demo_client_1', 'client1@autorent.local'),
        ('55555555-5555-5555-5555-555555555555'::uuid, 'demo_client_2', 'client2@autorent.local'),
        ('66666666-6666-6666-6666-666666666666'::uuid, 'demo_client_3', 'client3@autorent.local')
    ) AS seed(id, username, email)
)
INSERT INTO user_roles (user_id, role_id)
SELECT DISTINCT user_entity.id, role_entity.id
FROM seed_users seed
JOIN users user_entity
    ON user_entity.id = seed.id
    OR user_entity.email = seed.email
    OR user_entity.username = seed.username
JOIN roles role_entity
    ON role_entity.name = 'user'
ON CONFLICT DO NOTHING;
