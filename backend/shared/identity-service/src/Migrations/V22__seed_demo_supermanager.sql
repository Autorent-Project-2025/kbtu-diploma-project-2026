INSERT INTO users (id, username, email, password_hash, is_active)
SELECT
    '44444444-4444-4444-4444-444444444444'::uuid,
    'demo_supermanager',
    'supermanager@autorent.local',
    crypt('DemoSuperManager123!', gen_salt('bf')),
    TRUE
WHERE NOT EXISTS (
    SELECT 1
    FROM users
    WHERE id = '44444444-4444-4444-4444-444444444444'::uuid
       OR email = 'supermanager@autorent.local'
       OR username = 'demo_supermanager');

INSERT INTO user_roles (user_id, role_id)
SELECT
    user_entity.id,
    role_entity.id
FROM users user_entity
JOIN roles role_entity ON role_entity.name = 'supermanager'
WHERE user_entity.id = '44444444-4444-4444-4444-444444444444'::uuid
   OR user_entity.email = 'supermanager@autorent.local'
   OR user_entity.username = 'demo_supermanager'
ON CONFLICT DO NOTHING;
