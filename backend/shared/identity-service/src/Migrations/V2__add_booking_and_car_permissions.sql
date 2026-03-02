INSERT INTO permissions (id, name, description)
VALUES
    (gen_random_uuid(), 'Booking.Create', 'Allows creating bookings'),
    (gen_random_uuid(), 'Car.Create', 'Allows creating cars'),
    (gen_random_uuid(), 'Car.Update', 'Allows updating cars'),
    (gen_random_uuid(), 'Car.Delete', 'Allows deleting cars'),
    (gen_random_uuid(), 'Car.Image.Create', 'Allows uploading car images')
ON CONFLICT (name) DO NOTHING;

INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
JOIN permissions p ON p.name = 'Booking.Create'
WHERE r.name IN ('user', 'admin')
ON CONFLICT DO NOTHING;

INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
JOIN permissions p ON p.name IN ('Car.Create', 'Car.Update', 'Car.Delete', 'Car.Image.Create')
WHERE r.name = 'admin'
ON CONFLICT DO NOTHING;
