from __future__ import annotations

import re

CYRILLIC_TO_LATIN = str.maketrans(
    {
        "а": "a",
        "ә": "a",
        "б": "b",
        "в": "v",
        "г": "g",
        "ғ": "g",
        "д": "d",
        "е": "e",
        "ё": "e",
        "ж": "zh",
        "з": "z",
        "и": "i",
        "й": "i",
        "к": "k",
        "қ": "k",
        "л": "l",
        "м": "m",
        "н": "n",
        "ң": "n",
        "о": "o",
        "ө": "o",
        "п": "p",
        "р": "r",
        "с": "s",
        "т": "t",
        "у": "u",
        "ұ": "u",
        "ү": "u",
        "ф": "f",
        "х": "h",
        "һ": "h",
        "ц": "ts",
        "ч": "ch",
        "ш": "sh",
        "щ": "shch",
        "ы": "y",
        "і": "i",
        "э": "e",
        "ю": "yu",
        "я": "ya",
        "ь": "",
        "ъ": "",
    }
)

SLUG_SANITIZE_PATTERN = re.compile(r"[^a-z0-9]+")
SPACE_PATTERN = re.compile(r"\s+")


def normalize_text(value: str) -> str:
    return SPACE_PATTERN.sub(" ", value.strip())


def slugify(value: str) -> str:
    normalized = normalize_text(value).lower().translate(CYRILLIC_TO_LATIN)
    normalized = normalized.replace("&", " and ").replace("'", "").replace('"', "")
    normalized = SLUG_SANITIZE_PATTERN.sub("-", normalized)
    return normalized.strip("-")
