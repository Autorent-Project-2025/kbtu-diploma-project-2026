INSERT INTO permissions (id, name, description)
VALUES
    (gen_random_uuid(), 'Client.View', 'Allows viewing clients in client service'),
    (gen_random_uuid(), 'Client.Create', 'Allows creating clients in client service'),
    (gen_random_uuid(), 'Client.Update', 'Allows updating clients in client service'),
    (gen_random_uuid(), 'Client.Delete', 'Allows deleting clients in client service')
ON CONFLICT (name) DO NOTHING;

INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
JOIN permissions p ON p.name IN ('Client.View', 'Client.Create', 'Client.Update', 'Client.Delete')
WHERE r.name = 'admin'
ON CONFLICT DO NOTHING;

INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
JOIN permissions p ON p.name IN ('Client.View', 'Client.Create', 'Client.Update', 'Client.Delete')
WHERE r.name = 'superadmin'
ON CONFLICT DO NOTHING;
