INSERT INTO users (id, username, email, password_hash, is_active)
SELECT
    '77777777-7777-7777-7777-777777777777'::uuid,
    'demo_partner_two',
    'partner2@autorent.local',
    crypt('DemoPartnerTwo123!', gen_salt('bf')),
    TRUE
WHERE NOT EXISTS (
    SELECT 1
    FROM users
    WHERE id = '77777777-7777-7777-7777-777777777777'::uuid
       OR email = 'partner2@autorent.local'
       OR username = 'demo_partner_two');

INSERT INTO user_roles (user_id, role_id)
SELECT
    user_entity.id,
    role_entity.id
FROM users user_entity
JOIN roles role_entity ON role_entity.name = 'user'
WHERE user_entity.id = '77777777-7777-7777-7777-777777777777'::uuid
   OR user_entity.email = 'partner2@autorent.local'
   OR user_entity.username = 'demo_partner_two'
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
        WHERE id = '77777777-7777-7777-7777-777777777777'::uuid
           OR email = 'partner2@autorent.local'
           OR username = 'demo_partner_two';
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
        WHERE id = '77777777-7777-7777-7777-777777777777'::uuid
           OR email = 'partner2@autorent.local'
           OR username = 'demo_partner_two';
    END IF;
END $$;
