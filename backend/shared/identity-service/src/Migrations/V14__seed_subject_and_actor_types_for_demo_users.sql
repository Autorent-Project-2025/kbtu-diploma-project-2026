DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM information_schema.tables
        WHERE table_schema = 'public'
          AND table_name = 'subject_types')
       AND EXISTS (
        SELECT 1
        FROM information_schema.tables
        WHERE table_schema = 'public'
          AND table_name = 'actor_types')
    THEN
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
    END IF;

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
        SET subject_type = 'user',
            actor_type = 'admin'
        WHERE email = 'superadmin@local'
           OR username = 'superadmin';

        UPDATE users
        SET subject_type = 'user',
            actor_type = 'client'
        WHERE id = '11111111-1111-1111-1111-111111111111'::uuid
           OR email = 'user@autorent.local'
           OR username = 'demo_user';

        UPDATE users
        SET subject_type = 'user',
            actor_type = 'partner'
        WHERE id = '22222222-2222-2222-2222-222222222222'::uuid
           OR email = 'partner@autorent.local'
           OR username = 'demo_partner';

        UPDATE users
        SET subject_type = 'user',
            actor_type = 'internal'
        WHERE id = '33333333-3333-3333-3333-333333333333'::uuid
           OR email = 'manager@autorent.local'
           OR username = 'demo_manager';
    END IF;
END $$;
