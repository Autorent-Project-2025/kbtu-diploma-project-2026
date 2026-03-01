INSERT INTO roles (name) VALUES ('partner')
ON CONFLICT (name) DO NOTHING;