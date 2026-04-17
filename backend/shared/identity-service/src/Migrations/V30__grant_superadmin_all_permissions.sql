-- Superadmin should have ALL permissions. V4 granted via CROSS JOIN but only
-- for permissions that existed at that time. This catches up with all permissions
-- added since then (Booking.*, Complaint.*, AccessRequest.*, Payment.*, etc.).
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
CROSS JOIN permissions p
WHERE r.name = 'superadmin'
ON CONFLICT DO NOTHING;
