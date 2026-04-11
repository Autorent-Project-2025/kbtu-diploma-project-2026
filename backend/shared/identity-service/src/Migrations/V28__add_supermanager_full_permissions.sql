-- Add missing permissions needed for internal complaint workflow.

INSERT INTO permissions (id, name, description)
VALUES
    (gen_random_uuid(), 'AccessRequest.Review', 'Review, approve, reject, and revoke booking access requests'),
    (gen_random_uuid(), 'Payment.View', 'View payment and charge information'),
    (gen_random_uuid(), 'Payment.Update', 'Manage payments: refund charges, update payment status')
ON CONFLICT (name) DO NOTHING;

-- Grant supermanager the new permissions directly (not via inheritance).
-- Supermanager already inherits manager (Complaint.*, Complaint.Action.*) and
-- data-manager (Booking.View, Booking.Update, Booking.Delete, Client.*, PartnerCar.*).
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
JOIN permissions p ON p.name IN (
    'AccessRequest.Review',
    'Payment.View',
    'Payment.Update'
)
WHERE r.name = 'supermanager'
ON CONFLICT DO NOTHING;

-- Also grant admin the same permissions.
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
JOIN permissions p ON p.name IN (
    'AccessRequest.Review',
    'Payment.View',
    'Payment.Update'
)
WHERE r.name = 'admin'
ON CONFLICT DO NOTHING;
