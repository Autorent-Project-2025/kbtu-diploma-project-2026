INSERT INTO permissions (id, name, description)
VALUES
    (gen_random_uuid(), 'File.Create', 'Allows uploading private files'),
    (gen_random_uuid(), 'File.Read', 'Allows generating temporary links for private files'),
    (gen_random_uuid(), 'File.Delete', 'Allows deleting private files')
ON CONFLICT (name) DO NOTHING;

INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
JOIN permissions p ON p.name IN ('File.Create', 'File.Read', 'File.Delete')
WHERE r.name = 'admin'
ON CONFLICT DO NOTHING;

INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
JOIN permissions p ON p.name IN ('File.Create', 'File.Read', 'File.Delete')
WHERE r.name = 'superadmin'
ON CONFLICT DO NOTHING;
