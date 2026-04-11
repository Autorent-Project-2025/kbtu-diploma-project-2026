-- Complaint system: separate bounded context within ticket-service deployment unit.
-- Complaints are NOT tickets. They have their own aggregate, lifecycle, and permissions.

CREATE TABLE IF NOT EXISTS complaints (
    id                      UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    booking_id              INT NOT NULL,
    charge_id               BIGINT NULL,

    reporter_actor_type     INT NOT NULL,
    target_type             INT NOT NULL,
    category                INT NOT NULL,
    status                  INT NOT NULL DEFAULT 1,
    priority                INT NOT NULL DEFAULT 1,

    created_by_user_id      UUID NOT NULL,
    subject                 VARCHAR(200) NOT NULL,
    description             VARCHAR(4000) NOT NULL,

    assigned_to_manager_id  UUID NULL,

    info_request_text       VARCHAR(4000) NULL,
    info_request_at         TIMESTAMPTZ NULL,
    info_request_by         UUID NULL,
    info_response_text      VARCHAR(4000) NULL,
    info_response_at        TIMESTAMPTZ NULL,

    manager_note            VARCHAR(4000) NULL,
    manager_note_at         TIMESTAMPTZ NULL,
    manager_note_by         UUID NULL,

    resolution_type         INT NULL,
    resolution_note         VARCHAR(4000) NULL,
    resolved_at             TIMESTAMPTZ NULL,
    resolved_by             UUID NULL,

    rejection_reason        VARCHAR(4000) NULL,
    rejected_at             TIMESTAMPTZ NULL,
    rejected_by             UUID NULL,

    snapshot_data           JSONB NOT NULL DEFAULT '{}',

    created_at              TIMESTAMPTZ NOT NULL DEFAULT now(),
    updated_at              TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT chk_complaint_reporter_actor_type CHECK (reporter_actor_type IN (1, 2)),
    CONSTRAINT chk_complaint_target_type CHECK (target_type IN (1, 2)),
    CONSTRAINT chk_complaint_category CHECK (category IN (1, 2, 3, 4, 5, 99)),
    CONSTRAINT chk_complaint_status CHECK (status IN (1, 2, 3, 4, 5)),
    CONSTRAINT chk_complaint_priority CHECK (priority IN (1, 2, 3))
);

CREATE INDEX idx_complaints_status ON complaints (status);
CREATE INDEX idx_complaints_booking_id ON complaints (booking_id);
CREATE INDEX idx_complaints_created_by ON complaints (created_by_user_id);
CREATE INDEX idx_complaints_assigned_to ON complaints (assigned_to_manager_id) WHERE assigned_to_manager_id IS NOT NULL;
CREATE INDEX idx_complaints_created_at ON complaints (created_at);
CREATE INDEX idx_complaints_priority_status ON complaints (priority DESC, created_at ASC) WHERE status IN (1, 2, 3);

-- One active complaint per booking per reporter
CREATE UNIQUE INDEX uq_complaint_active_per_booking_reporter
    ON complaints (booking_id, created_by_user_id)
    WHERE status NOT IN (4, 5);

CREATE TABLE IF NOT EXISTS complaint_attachments (
    id                      UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    complaint_id            UUID NOT NULL REFERENCES complaints(id) ON DELETE CASCADE,
    file_name               VARCHAR(255) NOT NULL,
    original_file_name      VARCHAR(255) NOT NULL,
    file_type               VARCHAR(100) NOT NULL,
    uploaded_by_user_id     UUID NOT NULL,
    attachment_phase        INT NOT NULL DEFAULT 1,
    created_at              TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT chk_attachment_phase CHECK (attachment_phase IN (1, 2))
);

CREATE INDEX idx_complaint_attachments_complaint_id ON complaint_attachments (complaint_id);
