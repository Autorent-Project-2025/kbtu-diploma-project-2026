INSERT INTO permissions (id, name, description)
VALUES
    (gen_random_uuid(), 'Ticket.ViewAll', 'Allows viewing all tickets regardless of status')
ON CONFLICT (name) DO NOTHING;

INSERT INTO roles (id, name)
VALUES (gen_random_uuid(), 'supermanager')
ON CONFLICT (name) DO NOTHING;

-- supermanager gets all manager permissions plus Ticket.ViewAll and User.View
INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
JOIN permissions p ON p.name IN (
    'Ticket.View',
    'Ticket.Approve',
    'Ticket.Reject',
    'Ticket.ViewAll',
    'User.View'
)
WHERE r.name = 'supermanager'
ON CONFLICT DO NOTHING;

-- supermanager inherits manager
INSERT INTO role_inheritance (child_role_id, parent_role_id)
SELECT child.id, parent.id
FROM roles child
JOIN roles parent ON parent.name = 'manager'
WHERE child.name = 'supermanager'
ON CONFLICT DO NOTHING;
