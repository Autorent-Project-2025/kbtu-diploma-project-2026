-- Add complaint action permissions for Phase 1 manager actions within complaints.

INSERT INTO permissions (id, name, description)
VALUES
    (gen_random_uuid(), 'Complaint.Action.CancelBooking', 'Cancel the linked booking from a complaint'),
    (gen_random_uuid(), 'Complaint.Action.WaiveCharge', 'Waive a pending charge linked to a complaint booking'),
    (gen_random_uuid(), 'Complaint.Action.Escalate', 'Escalate a complaint to supermanager')
ON CONFLICT (name) DO NOTHING;

-- manager and admin get all complaint action permissions.
-- supermanager inherits from manager automatically via role_inheritance.
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
JOIN permissions p ON p.name IN (
    'Complaint.Action.CancelBooking',
    'Complaint.Action.WaiveCharge',
    'Complaint.Action.Escalate'
)
WHERE r.name IN ('manager', 'admin')
ON CONFLICT DO NOTHING;
