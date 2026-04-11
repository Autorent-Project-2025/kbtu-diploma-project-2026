INSERT INTO permissions (id, name, description)
VALUES (gen_random_uuid(), 'Complaint.Action.RefundCharge', 'Refund a paid charge linked to a complaint booking')
ON CONFLICT (name) DO NOTHING;

INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
JOIN permissions p ON p.name = 'Complaint.Action.RefundCharge'
WHERE r.name IN ('manager', 'admin')
ON CONFLICT DO NOTHING;
