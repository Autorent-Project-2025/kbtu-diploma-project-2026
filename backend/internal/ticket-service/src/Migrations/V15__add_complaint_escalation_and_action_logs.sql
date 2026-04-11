-- Add escalation fields to complaints table.
ALTER TABLE complaints ADD COLUMN IF NOT EXISTS is_escalated BOOLEAN NOT NULL DEFAULT FALSE;
ALTER TABLE complaints ADD COLUMN IF NOT EXISTS escalated_at TIMESTAMPTZ NULL;
ALTER TABLE complaints ADD COLUMN IF NOT EXISTS escalated_by UUID NULL;
ALTER TABLE complaints ADD COLUMN IF NOT EXISTS escalation_reason VARCHAR(4000) NULL;

CREATE INDEX IF NOT EXISTS idx_complaints_is_escalated
    ON complaints (is_escalated) WHERE is_escalated = TRUE;

-- Complaint action log: append-only audit trail for manager actions on complaints.
CREATE TABLE IF NOT EXISTS complaint_action_logs (
    id                  UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    complaint_id        UUID NOT NULL REFERENCES complaints(id) ON DELETE CASCADE,
    action_type         VARCHAR(100) NOT NULL,
    performed_by        UUID NOT NULL,
    comment             VARCHAR(4000) NULL,
    target_entity_type  VARCHAR(100) NULL,
    target_entity_id    VARCHAR(200) NULL,
    created_at          TIMESTAMPTZ NOT NULL DEFAULT now()
);

CREATE INDEX idx_complaint_action_logs_complaint_id ON complaint_action_logs (complaint_id);
CREATE INDEX idx_complaint_action_logs_created_at ON complaint_action_logs (created_at);
