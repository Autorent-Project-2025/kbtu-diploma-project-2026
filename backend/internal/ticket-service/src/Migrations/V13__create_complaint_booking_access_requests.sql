-- Complaint-scoped booking access requests.
-- Allows managers to request time-limited, read-only access to a specific booking
-- linked to a complaint they are reviewing. Supermanagers approve/reject.

CREATE TABLE IF NOT EXISTS complaint_booking_access_requests (
    id                          UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    complaint_id                UUID NOT NULL REFERENCES complaints(id) ON DELETE CASCADE,
    booking_id                  INT NOT NULL,
    requested_by_manager_id     UUID NOT NULL,
    status                      INT NOT NULL DEFAULT 1,
    reason                      VARCHAR(2000) NOT NULL,
    requested_at                TIMESTAMPTZ NOT NULL DEFAULT now(),

    reviewed_by_supermanager_id UUID NULL,
    reviewed_at                 TIMESTAMPTZ NULL,
    decision_note               VARCHAR(2000) NULL,
    expires_at                  TIMESTAMPTZ NULL,

    CONSTRAINT chk_access_request_status CHECK (status IN (1, 2, 3, 4, 5))
);

CREATE INDEX idx_access_requests_complaint_id ON complaint_booking_access_requests (complaint_id);
CREATE INDEX idx_access_requests_booking_id ON complaint_booking_access_requests (booking_id);
CREATE INDEX idx_access_requests_manager_id ON complaint_booking_access_requests (requested_by_manager_id);
CREATE INDEX idx_access_requests_status ON complaint_booking_access_requests (status);
CREATE INDEX idx_access_requests_manager_booking_status
    ON complaint_booking_access_requests (requested_by_manager_id, booking_id, status);

-- Prevent duplicate pending requests for the same complaint+booking+manager
CREATE UNIQUE INDEX uq_access_request_pending_per_complaint_manager
    ON complaint_booking_access_requests (complaint_id, booking_id, requested_by_manager_id)
    WHERE status = 1;
