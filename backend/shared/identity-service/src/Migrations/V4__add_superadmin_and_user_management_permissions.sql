ALTER TABLE users
    ADD COLUMN IF NOT EXISTS is_active BOOLEAN NOT NULL DEFAULT TRUE;

INSERT INTO roles (id, name)
VALUES
    (gen_random_uuid(), 'superadmin')
ON CONFLICT (name) DO NOTHING;

INSERT INTO permissions (id, name, description)
VALUES
    (gen_random_uuid(), 'Role.View', 'Allows viewing roles'),
    (gen_random_uuid(), 'Permission.View', 'Allows viewing permissions'),
    (gen_random_uuid(), 'User.View', 'Allows viewing users'),
    (gen_random_uuid(), 'User.Update', 'Allows updating users'),
    (gen_random_uuid(), 'User.RemoveRole', 'Allows removing roles from users'),
    (gen_random_uuid(), 'User.Deactivate', 'Allows deactivating users'),
    (gen_random_uuid(), 'User.Activate', 'Allows activating users'),
    (gen_random_uuid(), 'User.Delete', 'Allows deleting users')
ON CONFLICT (name) DO NOTHING;

INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
JOIN permissions p ON p.name IN (
    'Role.View',
    'Permission.View',
    'User.View',
    'User.Update',
    'User.RemoveRole',
    'User.Deactivate',
    'User.Activate',
    'User.Delete')
WHERE r.name = 'admin'
ON CONFLICT DO NOTHING;

INSERT INTO role_permissions (role_id, permission_id)
SELECT superadmin_role.id, permission.id
FROM roles superadmin_role
CROSS JOIN permissions permission
WHERE superadmin_role.name = 'superadmin'
ON CONFLICT DO NOTHING;

INSERT INTO users (id, username, email, password_hash, is_active)
SELECT
    gen_random_uuid(),
    'superadmin',
    'superadmin@local',
    crypt('SuperAdmin123!', gen_salt('bf')),
    TRUE
WHERE NOT EXISTS (
    SELECT 1
    FROM users
    WHERE email = 'superadmin@local'
       OR username = 'superadmin');

INSERT INTO user_roles (user_id, role_id)
SELECT user_entity.id, role_entity.id
FROM users user_entity
JOIN roles role_entity ON role_entity.name = 'superadmin'
WHERE user_entity.email = 'superadmin@local'
ON CONFLICT DO NOTHING;

DO $$
BEGIN
    IF EXISTS (
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
        SET subject_type = 'user'
        WHERE (email = 'superadmin@local' OR username = 'superadmin')
          AND (subject_type IS NULL OR btrim(subject_type) = '');

        UPDATE users
        SET actor_type = 'client'
        WHERE (email = 'superadmin@local' OR username = 'superadmin')
          AND (actor_type IS NULL OR btrim(actor_type) = '');
    END IF;
END $$;
