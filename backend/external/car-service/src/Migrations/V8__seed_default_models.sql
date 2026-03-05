INSERT INTO public.models (brand_id, name)
SELECT brand.id, seed.model_name
FROM (
    VALUES
        ('Toyota', 'Camry'),
        ('Toyota', 'Corolla'),
        ('Toyota', 'RAV4'),
        ('Toyota', 'Land Cruiser'),
        ('Toyota', 'Prius'),

        ('Nissan', 'Skyline'),
        ('Nissan', 'Altima'),
        ('Nissan', 'X-Trail'),
        ('Nissan', 'Patrol'),
        ('Nissan', 'Qashqai'),

        ('Geely', 'Emgrand'),
        ('Geely', 'Coolray'),
        ('Geely', 'Atlas'),
        ('Geely', 'Tugella'),
        ('Geely', 'Monjaro'),

        ('Honda', 'Civic'),
        ('Honda', 'Accord'),
        ('Honda', 'CR-V'),

        ('Hyundai', 'Elantra'),
        ('Hyundai', 'Sonata'),
        ('Hyundai', 'Tucson'),

        ('Kia', 'Rio'),
        ('Kia', 'K5'),
        ('Kia', 'Sportage'),

        ('Chevrolet', 'Cobalt'),
        ('Chevrolet', 'Malibu'),
        ('Chevrolet', 'Tahoe'),

        ('Ford', 'Focus'),
        ('Ford', 'Mondeo'),
        ('Ford', 'Explorer'),

        ('BMW', '3 Series'),
        ('BMW', '5 Series'),
        ('BMW', 'X5'),

        ('Mercedes-Benz', 'C-Class'),
        ('Mercedes-Benz', 'E-Class'),
        ('Mercedes-Benz', 'GLE'),

        ('Audi', 'A4'),
        ('Audi', 'A6'),
        ('Audi', 'Q7'),

        ('Volkswagen', 'Passat'),
        ('Volkswagen', 'Golf'),
        ('Volkswagen', 'Tiguan'),

        ('Lexus', 'ES'),
        ('Lexus', 'RX'),
        ('Lexus', 'LX'),

        ('Subaru', 'Impreza'),
        ('Subaru', 'Forester'),
        ('Subaru', 'Outback'),

        ('Mazda', 'Mazda3'),
        ('Mazda', 'Mazda6'),
        ('Mazda', 'CX-5'),

        ('Renault', 'Logan'),
        ('Renault', 'Duster'),
        ('Renault', 'Megane'),

        ('Skoda', 'Octavia'),
        ('Skoda', 'Superb'),
        ('Skoda', 'Kodiaq'),

        ('Chery', 'Tiggo 4'),
        ('Chery', 'Tiggo 7 Pro'),
        ('Chery', 'Arrizo 8'),

        ('Haval', 'Jolion'),
        ('Haval', 'F7'),
        ('Haval', 'H6'),

        ('BYD', 'Qin'),
        ('BYD', 'Han'),
        ('BYD', 'Song Plus'),
        ('BYD', 'Dolphin')
) AS seed(brand_name, model_name)
JOIN public.brands brand
    ON LOWER(brand.name) = LOWER(seed.brand_name)
WHERE NOT EXISTS (
    SELECT 1
    FROM public.models existing
    WHERE existing.brand_id = brand.id
      AND LOWER(existing.name) = LOWER(seed.model_name)
);
