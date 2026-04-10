-- Add missing CRUD permissions for internal panel tables
INSERT INTO permissions (id, name, description)
VALUES
    (gen_random_uuid(), 'Booking.View',   'View all bookings'),
    (gen_random_uuid(), 'Booking.Update', 'Update any booking'),
    (gen_random_uuid(), 'Booking.Delete', 'Delete any booking'),
    (gen_random_uuid(), 'PartnerCar.View','View all partner cars')
ON CONFLICT (name) DO NOTHING;

-- Create the data-manager role
INSERT INTO roles (id, name)
VALUES (gen_random_uuid(), 'data-manager')
ON CONFLICT (name) DO NOTHING;

-- Assign client, partner-car, and booking CRUD permissions
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
JOIN permissions p ON p.name IN (
    'Client.View',
    'Client.Update',
    'Client.Delete',
    'PartnerCar.View',
    'PartnerCar.Update',
    'PartnerCar.Delete',
    'Booking.View',
    'Booking.Update',
    'Booking.Delete'
)
WHERE r.name = 'data-manager'
ON CONFLICT DO NOTHING;

-- supermanager inherits data-manager
INSERT INTO role_inheritance (child_role_id, parent_role_id)
SELECT child.id, parent.id
FROM roles child
JOIN roles parent ON parent.name = 'data-manager'
WHERE child.name = 'supermanager'
ON CONFLICT DO NOTHING;
