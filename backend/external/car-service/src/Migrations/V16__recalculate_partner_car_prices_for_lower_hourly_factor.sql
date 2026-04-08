UPDATE partner_cars AS partner_car
SET
    price_hour = CASE
        WHEN car_model.market_value_kzt IS NULL OR car_model.market_value_kzt <= 0 THEN NULL
        ELSE ROUND(
            car_model.market_value_kzt *
            0.0001 *
            (1 + ((COALESCE(partner_car.rating, car_model.rating, 3.0) - 3.0) * 0.05)),
            2)
    END,
    price_day = CASE
        WHEN car_model.market_value_kzt IS NULL OR car_model.market_value_kzt <= 0 THEN NULL
        ELSE ROUND(
            ROUND(
                car_model.market_value_kzt *
                0.0001 *
                (1 + ((COALESCE(partner_car.rating, car_model.rating, 3.0) - 3.0) * 0.05)),
                2) * 24 * 0.90,
            2)
    END
FROM car_models AS car_model
WHERE partner_car.car_model_id = car_model.id;
