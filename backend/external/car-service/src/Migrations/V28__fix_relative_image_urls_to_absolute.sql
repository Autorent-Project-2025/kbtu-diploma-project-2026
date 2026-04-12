-- Convert relative image URLs to absolute by prepending the API gateway base URL.
-- This fixes image display on external sites that cannot resolve relative /internal/... paths.

UPDATE car_model_images
SET image_url = 'http://localhost:9186' || image_url
WHERE image_url LIKE '/internal/%';

UPDATE partner_car_images
SET image_url = 'http://localhost:9186' || image_url
WHERE image_url LIKE '/internal/%';
