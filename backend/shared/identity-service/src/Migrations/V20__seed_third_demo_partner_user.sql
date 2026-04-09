INSERT INTO users (id, username, email, password_hash, is_active)
SELECT
    '88888888-8888-8888-8888-888888888888'::uuid,
    'demo_partner_three',
    'partner3@autorent.local',
    crypt('DemoPartnerThree123!', gen_salt('bf')),
    TRUE
WHERE NOT EXISTS (
    SELECT 1
    FROM users
    WHERE id = '88888888-8888-8888-8888-888888888888'::uuid
       OR email = 'partner3@autorent.local'
       OR username = 'demo_partner_three');

INSERT INTO user_roles (user_id, role_id)
SELECT
    user_entity.id,
    role_entity.id
FROM users user_entity
JOIN roles role_entity ON role_entity.name = 'user'
WHERE user_entity.id = '88888888-8888-8888-8888-888888888888'::uuid
   OR user_entity.email = 'partner3@autorent.local'
   OR user_entity.username = 'demo_partner_three'
ON CONFLICT DO NOTHING;

DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name = 'users'
          AND column_name = 'subject_type_id')
       AND EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name = 'users'
          AND column_name = 'actor_type_id')
    THEN
        UPDATE users
        SET subject_type_id = '00000000-0000-0000-0000-000000000101'::uuid,
            actor_type_id = '00000000-0000-0000-0000-000000000202'::uuid
        WHERE id = '88888888-8888-8888-8888-888888888888'::uuid
           OR email = 'partner3@autorent.local'
           OR username = 'demo_partner_three';
    ELSIF EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name = 'users'
          AND column_name = 'subject_type')
       AND EXISTS (
        SELECT 1
        FROM information_schema.columns
        WHERE table_schema = 'public'
          AND table_name = 'users'
          AND column_name = 'actor_type')
    THEN
        UPDATE users
        SET subject_type = 'user',
            actor_type = 'partner'
        WHERE id = '88888888-8888-8888-8888-888888888888'::uuid
           OR email = 'partner3@autorent.local'
           OR username = 'demo_partner_three';
    END IF;
END $$;
