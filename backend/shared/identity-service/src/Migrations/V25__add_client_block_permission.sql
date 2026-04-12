-- Add Client.Block permission for blocking/unblocking client booking access
INSERT INTO permissions (id, name, description)
VALUES (gen_random_uuid(), 'Client.Block', 'Block or unblock client booking access')
ON CONFLICT (name) DO NOTHING;

-- Assign Client.Block to manager, data-manager, and admin roles.
-- supermanager inherits from manager automatically via role_inheritance.
-- superadmin gets all permissions via CROSS JOIN.
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
JOIN permissions p ON p.name = 'Client.Block'
WHERE r.name IN ('manager', 'data-manager', 'admin')
ON CONFLICT DO NOTHING;
