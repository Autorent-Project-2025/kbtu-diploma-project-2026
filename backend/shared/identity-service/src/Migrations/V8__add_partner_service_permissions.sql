INSERT INTO permissions (id, name, description)
VALUES
    (gen_random_uuid(), 'Partner.View', 'Allows viewing partners in partner service'),
    (gen_random_uuid(), 'Partner.Create', 'Allows creating partners in partner service'),
    (gen_random_uuid(), 'Partner.Update', 'Allows updating partners in partner service'),
    (gen_random_uuid(), 'Partner.Delete', 'Allows deleting partners in partner service')
ON CONFLICT (name) DO NOTHING;

INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
JOIN permissions p ON p.name IN ('Partner.View', 'Partner.Create', 'Partner.Update', 'Partner.Delete')
WHERE r.name = 'admin'
ON CONFLICT DO NOTHING;

INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
JOIN permissions p ON p.name IN ('Partner.View', 'Partner.Create', 'Partner.Update', 'Partner.Delete')
WHERE r.name = 'superadmin'
ON CONFLICT DO NOTHING;
