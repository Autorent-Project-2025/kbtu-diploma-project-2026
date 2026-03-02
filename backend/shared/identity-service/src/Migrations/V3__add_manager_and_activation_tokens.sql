CREATE TABLE IF NOT EXISTS activation_tokens (
    id UUID PRIMARY KEY,
    user_id UUID NOT NULL,
    token_hash VARCHAR(128) NOT NULL UNIQUE,
    created_at_utc TIMESTAMPTZ NOT NULL,
    expires_at_utc TIMESTAMPTZ NOT NULL,
    used_at_utc TIMESTAMPTZ NULL,
    CONSTRAINT fk_activation_tokens_user FOREIGN KEY (user_id) REFERENCES users(id) ON DELETE CASCADE
);

CREATE INDEX IF NOT EXISTS idx_activation_tokens_user_id ON activation_tokens (user_id);
CREATE INDEX IF NOT EXISTS idx_activation_tokens_expires_at_utc ON activation_tokens (expires_at_utc);

INSERT INTO roles (id, name)
VALUES
    (gen_random_uuid(), 'manager')
ON CONFLICT (name) DO NOTHING;

INSERT INTO permissions (id, name, description)
VALUES
    (gen_random_uuid(), 'User.Create', 'Allows creating users'),
    (gen_random_uuid(), 'Ticket.View', 'Allows viewing registration tickets'),
    (gen_random_uuid(), 'Ticket.Approve', 'Allows approving registration tickets'),
    (gen_random_uuid(), 'Ticket.Reject', 'Allows rejecting registration tickets')
ON CONFLICT (name) DO NOTHING;

INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
JOIN permissions p ON p.name IN ('User.Create', 'Ticket.View', 'Ticket.Approve', 'Ticket.Reject')
WHERE r.name = 'admin'
ON CONFLICT DO NOTHING;

INSERT INTO role_permissions (role_id, permission_id)
SELECT r.id, p.id
FROM roles r
JOIN permissions p ON p.name IN ('Ticket.View', 'Ticket.Approve', 'Ticket.Reject')
WHERE r.name = 'manager'
ON CONFLICT DO NOTHING;
