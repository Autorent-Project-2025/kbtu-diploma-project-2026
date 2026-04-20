import cv2
import numpy as np

from src.core.config import get_settings


def check_quality(image: np.ndarray) -> tuple[bool, str]:
    settings = get_settings()
    h, w = image.shape[:2]
    if w < settings.min_width or h < settings.min_height:
        return False, f"Low resolution: {w}x{h}"
    
    gray = cv2.cvtColor(image, cv2.COLOR_BGR2GRAY)
    
    sharpness = cv2.Laplacian(gray, cv2.CV_64F).var()
    if sharpness < settings.min_sharpness:
        return False, f"Blurry image (sharpness: {sharpness:.1f})"
        
    brightness = np.mean(gray)
    if not (settings.min_brightness <= brightness <= settings.max_brightness):
        return False, f"Bad lighting (brightness: {brightness:.1f})"
        
    return True, "OK"
