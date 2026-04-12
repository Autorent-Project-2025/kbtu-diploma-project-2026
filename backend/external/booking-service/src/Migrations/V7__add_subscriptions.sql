CREATE TABLE subscription_plans (
    id                 SERIAL PRIMARY KEY,
    name               VARCHAR(100) NOT NULL,
    plan_type          VARCHAR(20) NOT NULL,
    price              NUMERIC(18,2) NOT NULL,
    included_bookings  INT NOT NULL,
    is_active          BOOLEAN NOT NULL DEFAULT TRUE,
    created_at         TIMESTAMPTZ NOT NULL DEFAULT NOW()
);

CREATE TABLE subscriptions (
    id                   SERIAL PRIMARY KEY,
    user_id              UUID NOT NULL,
    subscription_plan_id INT NOT NULL,
    status               VARCHAR(20) NOT NULL DEFAULT 'active',
    start_date           TIMESTAMPTZ NOT NULL,
    end_date             TIMESTAMPTZ NOT NULL,
    auto_renew           BOOLEAN NOT NULL DEFAULT FALSE,
    included_bookings    INT NOT NULL,
    used_bookings        INT NOT NULL DEFAULT 0,
    created_at           TIMESTAMPTZ NOT NULL DEFAULT NOW(),

    CONSTRAINT fk_subscriptions_plan
        FOREIGN KEY (subscription_plan_id)
        REFERENCES subscription_plans(id)
        ON DELETE RESTRICT
);

ALTER TABLE bookings
    ADD COLUMN subscription_id INT NULL,
    ADD COLUMN used_subscription BOOLEAN NOT NULL DEFAULT FALSE;

ALTER TABLE bookings
    ADD CONSTRAINT fk_bookings_subscription
        FOREIGN KEY (subscription_id)
        REFERENCES subscriptions(id)
        ON DELETE SET NULL;

CREATE INDEX ix_subscriptions_user_id ON subscriptions(user_id);
CREATE INDEX ix_subscriptions_status ON subscriptions(status);
CREATE INDEX ix_bookings_subscription_id ON bookings(subscription_id);

INSERT INTO subscription_plans (name, plan_type, price, included_bookings, is_active)
VALUES
    ('Weekly Basic', 'weekly', 20000.00, 3, TRUE),
    ('Monthly Standard', 'monthly', 70000.00, 10, TRUE),
    ('Monthly Premium', 'monthly', 120000.00, 20, TRUE);