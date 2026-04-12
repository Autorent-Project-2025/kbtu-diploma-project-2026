INSERT INTO users (id, username, email, password_hash, is_active, subject_type_id, actor_type_id)
SELECT
    'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb'::uuid,
    'demo_supermanager',
    'supermanager@autorent.local',
    crypt('DemoSuperManager123!', gen_salt('bf')),
    TRUE,
    '00000000-0000-0000-0000-000000000101'::uuid, -- subject_type: user
    '00000000-0000-0000-0000-000000000204'::uuid  -- actor_type: internal
WHERE NOT EXISTS (
    SELECT 1
    FROM users
    WHERE id = 'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb'::uuid
       OR email = 'supermanager@autorent.local'
       OR username = 'demo_supermanager');

INSERT INTO user_roles (user_id, role_id)
SELECT
    user_entity.id,
    role_entity.id
FROM users user_entity
JOIN roles role_entity ON role_entity.name = 'supermanager'
WHERE user_entity.email = 'supermanager@autorent.local'
ON CONFLICT DO NOTHING;
