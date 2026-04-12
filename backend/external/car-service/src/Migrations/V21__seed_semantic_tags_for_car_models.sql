CREATE TEMP TABLE tmp_car_model_semantic_feature_seed (
    brand_name VARCHAR(255) NOT NULL,
    model_name VARCHAR(255) NOT NULL,
    model_year INT NOT NULL,
    feature_name TEXT NOT NULL
) ON COMMIT DROP;

INSERT INTO tmp_car_model_semantic_feature_seed (
    brand_name,
    model_name,
    model_year,
    feature_name
)
VALUES
    ('Toyota', 'Corolla', 2010, 'city'),
    ('Toyota', 'Corolla', 2010, 'family'),
    ('Toyota', 'Corolla', 2010, 'sedan'),
    ('Toyota', 'Camry', 2012, 'business'),
    ('Toyota', 'Camry', 2012, 'sedan'),
    ('Nissan', 'Skyline', 2003, 'sport'),
    ('Nissan', 'Skyline', 2003, 'sedan'),
    ('Kia', 'K5', 2021, 'business'),
    ('Kia', 'K5', 2021, 'city'),
    ('Kia', 'K5', 2021, 'sedan'),
    ('Mazda', 'RX7', 1992, 'sport'),
    ('Mazda', 'RX7', 1992, 'coupe'),
    ('Toyota', 'Supra', 1996, 'sport'),
    ('Toyota', 'Supra', 1996, 'coupe');

INSERT INTO public.features (name)
SELECT DISTINCT LOWER(TRIM(seed.feature_name))
FROM tmp_car_model_semantic_feature_seed seed
WHERE NOT EXISTS (
    SELECT 1
    FROM public.features feature
    WHERE LOWER(feature.name) = LOWER(TRIM(seed.feature_name))
);

INSERT INTO public.car_features (car_id, feature_id)
SELECT
    car_model.id,
    feature.id
FROM tmp_car_model_semantic_feature_seed seed
JOIN public.brands brand
    ON LOWER(brand.name) = LOWER(seed.brand_name)
JOIN public.models model_lookup
    ON model_lookup.brand_id = brand.id
   AND LOWER(model_lookup.name) = LOWER(seed.model_name)
JOIN public.car_models car_model
    ON car_model.brand_id = brand.id
   AND car_model.model_id = model_lookup.id
   AND car_model.year = seed.model_year
JOIN public.features feature
    ON LOWER(feature.name) = LOWER(TRIM(seed.feature_name))
WHERE NOT EXISTS (
    SELECT 1
    FROM public.car_features car_feature
    WHERE car_feature.car_id = car_model.id
      AND car_feature.feature_id = feature.id
);
