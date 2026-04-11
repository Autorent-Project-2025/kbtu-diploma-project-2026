-- Add complaint permissions for the complaint bounded context within ticket-service.

INSERT INTO permissions (id, name, description)
VALUES
    (gen_random_uuid(), 'Complaint.View', 'View complaints queue and details'),
    (gen_random_uuid(), 'Complaint.Review', 'Take, request info, and add notes to complaints'),
    (gen_random_uuid(), 'Complaint.Resolve', 'Resolve or reject complaints')
ON CONFLICT (name) DO NOTHING;

-- manager gets all complaint permissions (View + Review + Resolve).
-- supermanager inherits from manager automatically via role_inheritance.
-- superadmin gets all permissions via CROSS JOIN.
-- data-manager gets View only (read-only for analytics).
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
JOIN permissions p ON p.name IN ('Complaint.View', 'Complaint.Review', 'Complaint.Resolve')
WHERE r.name IN ('manager', 'admin')
ON CONFLICT DO NOTHING;

INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
JOIN permissions p ON p.name = 'Complaint.View'
WHERE r.name = 'data-manager'
ON CONFLICT DO NOTHING;
