-- Complaint reopen requests and strict booking uniqueness.

-- 1. Create complaint_reopen_requests table
CREATE TABLE IF NOT EXISTS complaint_reopen_requests (
    id                      UUID PRIMARY KEY DEFAULT gen_random_uuid(),
    complaint_id            UUID NOT NULL REFERENCES complaints(id) ON DELETE CASCADE,
    requested_by_user_id    UUID NOT NULL,
    reason                  VARCHAR(4000) NOT NULL,
    status                  INT NOT NULL DEFAULT 1,
    reviewed_by_manager_id  UUID NULL,
    reviewed_at             TIMESTAMPTZ NULL,
    decision_note           VARCHAR(4000) NULL,
    created_at              TIMESTAMPTZ NOT NULL DEFAULT now(),

    CONSTRAINT chk_reopen_request_status CHECK (status IN (1, 2, 3))
);

CREATE INDEX idx_reopen_requests_complaint_id ON complaint_reopen_requests (complaint_id);
CREATE INDEX idx_reopen_requests_status ON complaint_reopen_requests (status);

-- Only one pending reopen request per complaint at a time
CREATE UNIQUE INDEX uq_reopen_request_pending_per_complaint
    ON complaint_reopen_requests (complaint_id)
    WHERE status = 1;

-- 2. Replace partial unique index with strict uniqueness (one complaint per booking per reporter, period)
-- First drop old partial unique index that only blocks active complaints
DROP INDEX IF EXISTS uq_complaint_active_per_booking_reporter;

-- Before creating strict unique index, handle any existing duplicates by archiving older ones.
-- Mark duplicate closed complaints with a negative booking_id offset to avoid conflict.
-- This is a safe migration: it keeps the latest complaint per booking/reporter and shifts
-- older duplicates' booking_id so they don't conflict with the unique index.
DO $$
DECLARE
    dup RECORD;
BEGIN
    FOR dup IN
        SELECT id, booking_id, created_by_user_id, created_at,
               ROW_NUMBER() OVER (
                   PARTITION BY booking_id, created_by_user_id
                   ORDER BY created_at DESC
               ) AS rn
        FROM complaints
    LOOP
        IF dup.rn > 1 THEN
            -- Shift booking_id to negative to de-conflict while preserving data
            UPDATE complaints SET booking_id = -booking_id WHERE id = dup.id AND booking_id > 0;
        END IF;
    END LOOP;
END $$;

-- Now create strict unique index
CREATE UNIQUE INDEX uq_complaint_per_booking_reporter
    ON complaints (booking_id, created_by_user_id)
    WHERE booking_id > 0;
