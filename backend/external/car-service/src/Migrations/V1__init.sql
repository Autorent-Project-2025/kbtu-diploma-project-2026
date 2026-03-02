CREATE TABLE IF NOT EXISTS cars (
    id SERIAL PRIMARY KEY,
    brand VARCHAR(255) NOT NULL,
    model VARCHAR(255) NOT NULL,
    year INT NOT NULL,
    image_url TEXT,
    price_hour NUMERIC(10, 2),
    price_day NUMERIC(10, 2)
);

ALTER TABLE cars
    ADD COLUMN IF NOT EXISTS engine VARCHAR(100),
    ADD COLUMN IF NOT EXISTS transmission VARCHAR(100),
    ADD COLUMN IF NOT EXISTS seats INT CHECK (seats > 0 AND seats <= 20),
    ADD COLUMN IF NOT EXISTS fuel_type VARCHAR(50),
    ADD COLUMN IF NOT EXISTS color VARCHAR(50),
    ADD COLUMN IF NOT EXISTS doors INT CHECK (doors > 0 AND doors <= 6),
    ADD COLUMN IF NOT EXISTS description VARCHAR(585),
    ADD COLUMN IF NOT EXISTS rating NUMERIC(2, 1) CHECK (rating >= 1 AND rating <= 5); 


CREATE TABLE IF NOT EXISTS car_images (
    id SERIAL PRIMARY KEY,
    car_id INTEGER NOT NULL,
    image_url TEXT NOT NULL,
    image_type INTEGER NOT NULL, -- 0=Exterior, 1=Interior, 2=Detail (enum in code)
    display_order INTEGER NOT NULL,
    FOREIGN KEY (car_id) REFERENCES cars(id) ON DELETE CASCADE
);

CREATE INDEX idx_car_image_car_id ON car_images(car_id);
CREATE INDEX idx_car_image_display_order ON car_images(car_id, display_order);

CREATE TABLE IF NOT EXISTS features (
    id SERIAL PRIMARY KEY,
    name TEXT NOT NULL UNIQUE,
    created_on TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE INDEX idx_feature_name ON features(name);

CREATE TABLE IF NOT EXISTS car_features (
    id SERIAL PRIMARY KEY,
    car_id INTEGER NOT NULL,
    feature_id INTEGER NOT NULL,
    FOREIGN KEY (car_id) REFERENCES cars(id) ON DELETE CASCADE,
    FOREIGN KEY (feature_id) REFERENCES features(id) ON DELETE CASCADE,
    UNIQUE(car_id, feature_id)
);

CREATE INDEX idx_car_feature_car_id ON car_features(car_id);
CREATE INDEX idx_car_feature_feature_id ON car_features(feature_id);

CREATE TABLE IF NOT EXISTS car_comments (
    id SERIAL PRIMARY KEY,
    user_id INTEGER NOT NULL,
    user_name VARCHAR(255) NOT NULL,
    car_id INTEGER NOT NULL,
    content TEXT NOT NULL,
    rating INTEGER NOT NULL CHECK (rating >= 1 AND rating <= 5),
    created_on TIMESTAMP NOT NULL DEFAULT NOW(),
    FOREIGN KEY (car_id) REFERENCES cars(id) ON DELETE CASCADE
);

CREATE INDEX idx_car_comment_car_id ON car_comments(car_id);
CREATE INDEX idx_car_comment_user_id ON car_comments(user_id);
CREATE INDEX idx_car_comment_created_on ON car_comments(created_on DESC);