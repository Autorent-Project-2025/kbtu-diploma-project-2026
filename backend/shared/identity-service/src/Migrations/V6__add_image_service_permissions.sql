INSERT INTO permissions (id, name, description)
VALUES
    (gen_random_uuid(), 'Image.Create', 'Allows uploading images to image service'),
    (gen_random_uuid(), 'Image.Delete', 'Allows deleting images from image service')
ON CONFLICT (name) DO NOTHING;

INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
JOIN permissions p ON p.name IN ('Image.Create', 'Image.Delete')
WHERE r.name = 'admin'
ON CONFLICT DO NOTHING;

INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
JOIN permissions p ON p.name IN ('Image.Create', 'Image.Delete')
WHERE r.name = 'superadmin'
ON CONFLICT DO NOTHING;
