INSERT INTO users (id, username, email, password_hash, is_active)
SELECT
    '11111111-1111-1111-1111-111111111111'::uuid,
    'demo_user',
    'user@autorent.local',
    crypt('DemoUser123!', gen_salt('bf')),
    TRUE
WHERE NOT EXISTS (
    SELECT 1
    FROM users
    WHERE id = '11111111-1111-1111-1111-111111111111'::uuid
       OR email = 'user@autorent.local'
       OR username = 'demo_user');

INSERT INTO user_roles (user_id, role_id)
SELECT
    user_entity.id,
    role_entity.id
FROM users user_entity
JOIN roles role_entity ON role_entity.name = 'user'
WHERE user_entity.id = '11111111-1111-1111-1111-111111111111'::uuid
   OR user_entity.email = 'user@autorent.local'
   OR user_entity.username = 'demo_user'
ON CONFLICT DO NOTHING;

INSERT INTO users (id, username, email, password_hash, is_active)
SELECT
    '22222222-2222-2222-2222-222222222222'::uuid,
    'demo_partner',
    'partner@autorent.local',
    crypt('DemoPartner123!', gen_salt('bf')),
    TRUE
WHERE NOT EXISTS (
    SELECT 1
    FROM users
    WHERE id = '22222222-2222-2222-2222-222222222222'::uuid
       OR email = 'partner@autorent.local'
       OR username = 'demo_partner');

INSERT INTO user_roles (user_id, role_id)
SELECT
    user_entity.id,
    role_entity.id
FROM users user_entity
JOIN roles role_entity ON role_entity.name = 'user'
WHERE user_entity.id = '22222222-2222-2222-2222-222222222222'::uuid
   OR user_entity.email = 'partner@autorent.local'
   OR user_entity.username = 'demo_partner'
ON CONFLICT DO NOTHING;

INSERT INTO users (id, username, email, password_hash, is_active)
SELECT
    '33333333-3333-3333-3333-333333333333'::uuid,
    'demo_manager',
    'manager@autorent.local',
    crypt('DemoManager123!', gen_salt('bf')),
    TRUE
WHERE NOT EXISTS (
    SELECT 1
    FROM users
    WHERE id = '33333333-3333-3333-3333-333333333333'::uuid
       OR email = 'manager@autorent.local'
       OR username = 'demo_manager');

INSERT INTO user_roles (user_id, role_id)
SELECT
    user_entity.id,
    role_entity.id
FROM users user_entity
JOIN roles role_entity ON role_entity.name = 'manager'
WHERE user_entity.id = '33333333-3333-3333-3333-333333333333'::uuid
   OR user_entity.email = 'manager@autorent.local'
   OR user_entity.username = 'demo_manager'
ON CONFLICT DO NOTHING;
