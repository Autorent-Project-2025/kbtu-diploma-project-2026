INSERT INTO public.brands (name)
SELECT seed.name
FROM (
    VALUES
        ('Toyota'),
        ('Nissan'),
        ('Geely'),
        ('Honda'),
        ('Hyundai'),
        ('Kia'),
        ('Chevrolet'),
        ('Ford'),
        ('BMW'),
        ('Mercedes-Benz'),
        ('Audi'),
        ('Volkswagen'),
        ('Lexus'),
        ('Subaru'),
        ('Mazda'),
        ('Renault'),
        ('Skoda'),
        ('Chery'),
        ('Haval'),
        ('BYD')
) AS seed(name)
WHERE NOT EXISTS (
    SELECT 1
    FROM public.brands existing
    WHERE LOWER(existing.name) = LOWER(seed.name)
);
