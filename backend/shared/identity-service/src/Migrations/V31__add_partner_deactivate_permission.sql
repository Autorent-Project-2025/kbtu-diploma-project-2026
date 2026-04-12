INSERT INTO permissions (id, name, description)
VALUES (gen_random_uuid(), 'Partner.Deactivate', 'Deactivate or reactivate a partner and their cars')
ON CONFLICT (name) DO NOTHING;

-- Grant to supermanager and admin roles.
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
JOIN permissions p ON p.name = 'Partner.Deactivate'
WHERE r.name IN ('supermanager', 'admin')
ON CONFLICT DO NOTHING;

-- Superadmin gets all permissions.
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
CROSS JOIN permissions p
WHERE r.name = 'superadmin'
ON CONFLICT DO NOTHING;
