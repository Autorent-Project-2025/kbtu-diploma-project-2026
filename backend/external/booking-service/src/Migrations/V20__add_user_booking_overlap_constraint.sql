-- V20: companion to V3's `prevent_overlapping_bookings` (per-car) — adds
-- a per-user range exclusion so a single user cannot hold two active bookings
-- whose time ranges overlap. Mirrors the application-level
-- HasOverlappingUserBookings check in BookingService.CreateBooking.
--
-- With both DB-side EXCLUDE constraints in place, application code can drop
-- the read-then-insert overlap pattern and run booking creation under
-- ReadCommitted instead of Serializable. That removes the false-positive
-- 40001 serialization_failure retries that surface under high concurrency,
-- because the DB itself enforces correctness via exclusion violations
-- (23P01) which only fire on actual conflicts.

DO $$
BEGIN
    IF EXISTS (
        SELECT 1
        FROM public.bookings b1
        JOIN public.bookings b2
          ON b1.id < b2.id
         AND b1.user_id = b2.user_id
         AND b1.booking_range && b2.booking_range
         AND lower(coalesce(b1.status, '')) IN ('pending', 'confirmed', 'active')
         AND lower(coalesce(b2.status, '')) IN ('pending', 'confirmed', 'active')
    ) THEN
        RAISE EXCEPTION
            'Cannot add prevent_overlapping_user_bookings: overlapping active user bookings already exist. Resolve conflicts first.';
    END IF;
END $$;

DO $$
BEGIN
    IF NOT EXISTS (
        SELECT 1
        FROM pg_constraint
        WHERE conrelid = 'public.bookings'::regclass
          AND conname = 'prevent_overlapping_user_bookings'
    ) THEN
        ALTER TABLE public.bookings
            ADD CONSTRAINT prevent_overlapping_user_bookings
            EXCLUDE USING gist (
                user_id WITH =,
                booking_range WITH &&
            )
            WHERE (status IN ('pending', 'confirmed', 'active'));
    END IF;
END $$;
