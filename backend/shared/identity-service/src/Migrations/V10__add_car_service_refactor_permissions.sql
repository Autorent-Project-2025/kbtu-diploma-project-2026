INSERT INTO permissions (id, name, description)
VALUES
    (gen_random_uuid(), 'CarModel.Create', 'Allows creating car models'),
    (gen_random_uuid(), 'CarModel.Update', 'Allows updating car models'),
    (gen_random_uuid(), 'CarModel.Delete', 'Allows deleting car models'),
    (gen_random_uuid(), 'PartnerCar.Create', 'Allows creating partner-owned cars'),
    (gen_random_uuid(), 'PartnerCar.Update', 'Allows updating partner-owned cars'),
    (gen_random_uuid(), 'PartnerCar.Delete', 'Allows deleting partner-owned cars'),
    (gen_random_uuid(), 'PartnerCar.ViewOwn', 'Allows viewing own partner-owned cars'),
    (gen_random_uuid(), 'CarComment.Create', 'Allows creating car comments'),
    (gen_random_uuid(), 'CarComment.Update', 'Allows updating own car comments'),
    (gen_random_uuid(), 'CarComment.Delete', 'Allows deleting own car comments'),
    (gen_random_uuid(), 'CarImage.Create', 'Allows creating partner car images through car service'),
    (gen_random_uuid(), 'CarImage.Update', 'Allows updating partner car images through car service'),
    (gen_random_uuid(), 'CarImage.Delete', 'Allows deleting partner car images through car service')
ON CONFLICT (name) DO NOTHING;

INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
JOIN permissions p ON p.name IN (
    'CarModel.Create',
    'CarModel.Update',
    'CarModel.Delete',
    'PartnerCar.Create',
    'PartnerCar.Update',
    'PartnerCar.Delete',
    'PartnerCar.ViewOwn',
    'CarComment.Create',
    'CarComment.Update',
    'CarComment.Delete',
    'CarImage.Create',
    'CarImage.Update',
    'CarImage.Delete')
WHERE r.name = 'admin'
ON CONFLICT DO NOTHING;

INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
JOIN permissions p ON p.name IN (
    'CarModel.Create',
    'CarModel.Update',
    'CarModel.Delete',
    'PartnerCar.Create',
    'PartnerCar.Update',
    'PartnerCar.Delete',
    'PartnerCar.ViewOwn',
    'CarComment.Create',
    'CarComment.Update',
    'CarComment.Delete',
    'CarImage.Create',
    'CarImage.Update',
    'CarImage.Delete')
WHERE r.name = 'superadmin'
ON CONFLICT DO NOTHING;

INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
JOIN permissions p ON p.name IN (
    'PartnerCar.Create',
    'PartnerCar.Update',
    'PartnerCar.Delete',
    'PartnerCar.ViewOwn',
    'CarComment.Create',
    'CarComment.Update',
    'CarComment.Delete',
    'CarImage.Create',
    'CarImage.Update',
    'CarImage.Delete',
    'Image.Create',
    'Image.Delete')
WHERE r.name = 'user'
ON CONFLICT DO NOTHING;
