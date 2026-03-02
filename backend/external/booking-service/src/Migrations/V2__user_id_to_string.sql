ALTER TABLE bookings
    ALTER COLUMN user_id TYPE VARCHAR(64) USING user_id::text;
