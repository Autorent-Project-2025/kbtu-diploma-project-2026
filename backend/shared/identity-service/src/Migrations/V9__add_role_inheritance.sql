CREATE TABLE IF NOT EXISTS role_inheritance (
    child_role_id UUID NOT NULL,
    parent_role_id UUID NOT NULL,
    PRIMARY KEY (child_role_id, parent_role_id),
    CONSTRAINT fk_role_inheritance_child_role FOREIGN KEY (child_role_id) REFERENCES roles(id) ON DELETE CASCADE,
    CONSTRAINT fk_role_inheritance_parent_role FOREIGN KEY (parent_role_id) REFERENCES roles(id) ON DELETE CASCADE,
    CONSTRAINT chk_role_inheritance_not_self CHECK (child_role_id <> parent_role_id)
);

CREATE INDEX IF NOT EXISTS idx_role_inheritance_parent_role_id
    ON role_inheritance (parent_role_id);
