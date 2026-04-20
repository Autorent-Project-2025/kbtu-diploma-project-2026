"""
Generates a fine-tuning dataset for AutoRent car recommendation parser.

The model learns TWO skills:
1. TRANSLITERATION: "кобальт"→"cobalt", "камри"→"camry" (pattern, not memorization)
2. STRUCTURED PARSING: extract filters from natural language, distinguish year from price

RAG (catalog knowledge) is handled at inference time, NOT here.
The model does NOT memorize specific cars — it learns the FORMAT and LOGIC.

Usage:
    python generate_dataset.py --db-url "postgresql://postgres:postgres@localhost:1836/postgres_db"
"""

import argparse
import json
import random
import subprocess
import sys
from dataclasses import dataclass

try:
    import psycopg2
    HAS_PSYCOPG2 = True
except ImportError:
    HAS_PSYCOPG2 = False

SYSTEM_PROMPT = """You extract structured filters for car recommendation search.
Return only valid JSON.
Schema:
{
  "maxBudgetPerHour": number | null,
  "passengers": number | null,
  "transmission": string | null,
  "minRating": number | null,
  "preferredStyles": string[],
  "excludedStyles": string[],
  "preferredBrands": string[],
  "minYear": number | null,
  "maxYear": number | null,
  "startTime": string | null,
  "endTime": string | null,
  "requiresAvailableOnDates": boolean
}
Allowed style labels: sport, business, family, city, luxury.
Allowed transmission labels: automatic, manual.
If the user writes a car brand or model in Cyrillic, transliterate it to Latin for preferredBrands.
If a value is not explicitly or reasonably inferable, return null or [].
Do not invent values. Do not put year values into maxBudgetPerHour."""

# ---------------------------------------------------------------------------
# Transliteration pairs — this is the PATTERN the model learns.
# Not a lookup table for inference — the model generalizes from these examples.
# ---------------------------------------------------------------------------
TRANSLITERATION_PAIRS = [
    # Real car brands
    ("шевроле", "chevrolet"), ("тойота", "toyota"), ("ниссан", "nissan"),
    ("мазда", "mazda"), ("хонда", "honda"), ("форд", "ford"),
    ("бмв", "bmw"), ("мерседес", "mercedes"), ("ауди", "audi"),
    ("лексус", "lexus"), ("киа", "kia"), ("хёндай", "hyundai"),
    ("хендай", "hyundai"), ("хундай", "hyundai"),
    ("фольксваген", "volkswagen"), ("порше", "porsche"),
    ("субару", "subaru"), ("митсубиси", "mitsubishi"),
    ("сузуки", "suzuki"), ("рено", "renault"), ("пежо", "peugeot"),
    ("ситроен", "citroen"), ("вольво", "volvo"), ("джип", "jeep"),
    ("ленд ровер", "land rover"), ("рейндж ровер", "range rover"),
    # Real car models
    ("кобальт", "cobalt"), ("камри", "camry"), ("королла", "corolla"),
    ("супра", "supra"), ("скайлайн", "skyline"), ("гольф", "golf"),
    ("поло", "polo"), ("тигуан", "tiguan"), ("туксон", "tucson"),
    ("тусон", "tucson"), ("аккорд", "accord"), ("сивик", "civic"),
    ("фокус", "focus"), ("мустанг", "mustang"), ("импреза", "impreza"),
    ("лансер", "lancer"), ("аутлендер", "outlander"), ("каптур", "captur"),
    ("дастер", "duster"), ("логан", "logan"), ("сандеро", "sandero"),
    ("октавия", "octavia"), ("рапид", "rapid"), ("солярис", "solaris"),
    ("крета", "creta"), ("туарег", "touareg"), ("пассат", "passat"),
    ("джетта", "jetta"), ("элантра", "elantra"), ("соната", "sonata"),
    ("оптима", "optima"), ("спортейдж", "sportage"), ("сид", "ceed"),
    ("рио", "rio"), ("селтос", "seltos"), ("аутбэк", "outback"),
    ("форестер", "forester"), ("прадо", "prado"), ("ленд крузер", "land cruiser"),
    ("хайлендер", "highlander"), ("рав4", "rav4"),
]


@dataclass
class Car:
    brand: str
    model: str
    year: int
    price_hour: float | None
    transmission: str | None
    seats: int | None
    tags: list[str]


def load_catalog_psycopg2(db_url: str) -> list[Car]:
    conn = psycopg2.connect(db_url)
    cur = conn.cursor()
    cur.execute("""
        SELECT DISTINCT brand, model, year, price_hour, transmission, seats, tags
        FROM ai_car_documents WHERE brand IS NOT NULL AND model IS NOT NULL
    """)
    cars = []
    for row in cur.fetchall():
        tags = row[6] if isinstance(row[6], list) else json.loads(row[6]) if row[6] else []
        cars.append(Car(brand=row[0], model=row[1], year=row[2],
                        price_hour=float(row[3]) if row[3] else None,
                        transmission=row[4], seats=row[5], tags=tags))
    cur.close()
    conn.close()
    return cars


def load_catalog_docker(container: str) -> list[Car]:
    """Load catalog via docker exec — works when DB port is not exposed to host."""
    query = """SELECT json_agg(row_to_json(t)) FROM (
        SELECT DISTINCT brand, model, year, price_hour, transmission, seats, tags::text
        FROM ai_car_documents WHERE brand IS NOT NULL AND model IS NOT NULL
    ) t"""
    result = subprocess.run(
        ["docker", "exec", container, "psql", "-U", "postgres", "-d", "postgres_db",
         "-t", "-A", "-c", query],
        capture_output=True, text=True,
    )
    if result.returncode != 0:
        raise RuntimeError(f"docker exec failed: {result.stderr.strip()}")

    raw = result.stdout.strip()
    if not raw or raw == "null":
        return []

    rows = json.loads(raw)
    cars = []
    for row in rows:
        tags = json.loads(row["tags"]) if isinstance(row["tags"], str) else (row["tags"] or [])
        cars.append(Car(
            brand=row["brand"], model=row["model"], year=row["year"],
            price_hour=float(row["price_hour"]) if row["price_hour"] else None,
            transmission=row.get("transmission"), seats=row.get("seats"), tags=tags,
        ))
    return cars


def load_catalog(db_url: str | None, container: str | None) -> list[Car]:
    if db_url and HAS_PSYCOPG2:
        return load_catalog_psycopg2(db_url)
    if container:
        return load_catalog_docker(container)
    if db_url and not HAS_PSYCOPG2:
        print("psycopg2 not installed, trying docker exec...", file=sys.stderr)
    # Auto-detect container name
    result = subprocess.run(
        ["docker", "ps", "--filter", "name=ai-search-db", "--format", "{{.Names}}"],
        capture_output=True, text=True,
    )
    name = result.stdout.strip().split("\n")[0]
    if name:
        print(f"Auto-detected container: {name}")
        return load_catalog_docker(name)
    raise RuntimeError("Cannot connect to DB. Use --db-url or --container.")


def resp(**kw) -> str:
    base = {"maxBudgetPerHour": None, "passengers": None, "transmission": None,
            "minRating": None, "preferredStyles": [], "excludedStyles": [],
            "preferredBrands": [], "minYear": None, "maxYear": None,
            "startTime": None, "endTime": None, "requiresAvailableOnDates": False}
    base.update(kw)
    return json.dumps(base, ensure_ascii=False)


def ex(user: str, assistant: str) -> dict:
    return {"messages": [
        {"role": "system", "content": SYSTEM_PROMPT},
        {"role": "user", "content": user},
        {"role": "assistant", "content": assistant},
    ]}


# ---------------------------------------------------------------------------
# 1. TRANSLITERATION examples — the core skill
# ---------------------------------------------------------------------------
def gen_transliteration() -> list[dict]:
    out = []
    for cyrillic, latin in TRANSLITERATION_PAIRS:
        brand = latin  # simplified — brand = transliterated name
        for tmpl in [
            f"Хочу {cyrillic}", f"Покажи {cyrillic}", f"Есть {cyrillic}?",
            f"Нужен {cyrillic}", f"Ищу {cyrillic}", f"Дай {cyrillic}",
            f"хочу {cyrillic} плиз", f"а {cyrillic} есть?",
            f"подбери {cyrillic}", f"мне нужен {cyrillic}",
        ]:
            out.append(ex(tmpl, resp(preferredBrands=[brand])))

        # with filters
        out.append(ex(f"{cyrillic} от 2020 года", resp(preferredBrands=[brand], minYear=2020)))
        out.append(ex(f"{cyrillic} до 5000 тенге", resp(preferredBrands=[brand], maxBudgetPerHour=5000)))
        out.append(ex(f"{cyrillic} на автомате", resp(preferredBrands=[brand], transmission="automatic")))

    return out


# ---------------------------------------------------------------------------
# 2. Latin brand/model from catalog (direct, no transliteration needed)
# ---------------------------------------------------------------------------
def gen_catalog_direct(cars: list[Car]) -> list[dict]:
    out = []
    seen = set()
    for car in cars:
        key = f"{car.brand}|{car.model}"
        if key in seen:
            continue
        seen.add(key)
        b = car.brand.lower()

        for tmpl in [
            f"I need {car.model}", f"Show me {car.brand} {car.model}",
            f"Want {car.model}", f"{car.model} please",
            f"Хочу {car.brand} {car.model}", f"Покажи {car.brand} {car.model}",
            f"{car.model.lower()}", f"{car.brand.lower()} {car.model.lower()}",
        ]:
            out.append(ex(tmpl, resp(preferredBrands=[b])))

        # year
        out.append(ex(f"{car.model} от {car.year} года", resp(preferredBrands=[b], minYear=car.year)))
        out.append(ex(f"{car.model} {car.year}+", resp(preferredBrands=[b], minYear=car.year)))

        # budget
        if car.price_hour:
            budget = int(car.price_hour * 1.2)
            out.append(ex(f"{car.model} до {budget} тенге", resp(preferredBrands=[b], maxBudgetPerHour=budget)))

    return out


# ---------------------------------------------------------------------------
# 3. Year vs Budget disambiguation (CRITICAL)
# ---------------------------------------------------------------------------
def gen_year() -> list[dict]:
    out = []
    # minYear
    for q, y in [
        ("машина с 2020 года", 2020), ("от 2018 года", 2018),
        ("не старше 2019", 2019), ("2020+", 2020),
        ("начиная с 2017", 2017), ("год выпуска от 2020", 2020),
        ("свежее 2021 года", 2021), ("car from 2020", 2020),
        ("2019 or newer", 2019), ("машина 2020 года", 2020),
        ("авто с 2020", 2020), ("хочу 2020 года выпуска", 2020),
        ("2022 год", 2022), ("не старше 2018 года", 2018),
        ("минимум 2020 года", 2020), ("выпуск после 2019", 2019),
    ]:
        out.append(ex(q, resp(minYear=y)))

    # maxYear
    for q, y in [
        ("до 2015 года выпуска", 2015), ("не новее 2022", 2022),
        ("по 2020 год", 2020), ("до 2018 года", 2018),
    ]:
        out.append(ex(q, resp(maxYear=y)))

    # range
    for q, mn, mx in [
        ("между 2018 и 2022", 2018, 2022), ("с 2019 по 2023", 2019, 2023),
        ("от 2020 до 2024 года", 2020, 2024),
    ]:
        out.append(ex(q, resp(minYear=mn, maxYear=mx)))

    return out


def gen_budget() -> list[dict]:
    out = []
    for q, b in [
        ("до 5000 в час", 5000), ("бюджет 3000 тенге", 3000),
        ("не дороже 8000 ₸/час", 8000), ("до 10000 тг в час", 10000),
        ("максимум 6000", 6000), ("в пределах 5000", 5000),
        ("хочу что-нибудь до 3000 в час", 3000), ("не больше 9000", 9000),
        ("under 7000 per hour", 7000), ("budget 4000", 4000),
        ("max 10000", 10000), ("до двух тысяч", 2000),
    ]:
        out.append(ex(q, resp(maxBudgetPerHour=b)))
    return out


def gen_styles() -> list[dict]:
    out = []
    for q, p, e in [
        ("спортивную машину", ["sport"], []),
        ("спорткар", ["sport"], []),
        ("купе хочу", ["sport"], []),
        ("семейную машину", ["family"], []),
        ("для семьи с детьми", ["family"], []),
        ("минивэн для семьи", ["family"], []),
        ("бизнес класс", ["business"], []),
        ("для деловой встречи", ["business"], []),
        ("городскую машину", ["city"], []),
        ("для города на каждый день", ["city"], []),
        ("премиум авто", ["luxury"], []),
        ("люксовую", ["luxury"], []),
        ("спортивную но не бизнес", ["sport"], ["business"]),
        ("не спортивную", [], ["sport"]),
        ("без спорткаров", [], ["sport"]),
        ("sporty car", ["sport"], []),
        ("family car", ["family"], []),
        ("luxury sedan", ["luxury"], []),
    ]:
        out.append(ex(q, resp(preferredStyles=p, excludedStyles=e)))
    return out


def gen_other_filters() -> list[dict]:
    out = []
    # transmission
    for q, t in [
        ("автомат", "automatic"), ("на автомате", "automatic"),
        ("акпп", "automatic"), ("механика", "manual"),
        ("на механике", "manual"), ("мкпп", "manual"),
        ("automatic", "automatic"), ("manual", "manual"),
    ]:
        out.append(ex(q, resp(transmission=t)))

    # passengers
    for q, p in [
        ("на 5 человек", 5), ("на 4 места", 4), ("для двоих", 2),
        ("7 мест", 7), ("нас будет 5", 5), ("5 seats", 5),
    ]:
        out.append(ex(q, resp(passengers=p)))

    # rating
    for q, r in [
        ("рейтинг от 4", 4.0), ("рейтинг больше 4.5", 4.5),
        ("с хорошим рейтингом", 4.0), ("rating above 4", 4.0),
    ]:
        out.append(ex(q, resp(minRating=r)))

    return out


# ---------------------------------------------------------------------------
# 4. Combined multi-filter
# ---------------------------------------------------------------------------
def gen_combined() -> list[dict]:
    out = []
    examples = [
        ("спортивную до 8000 на автомате", resp(preferredStyles=["sport"], maxBudgetPerHour=8000, transmission="automatic")),
        ("семейную от 2020 года на 5 мест", resp(preferredStyles=["family"], minYear=2020, passengers=5)),
        ("тойота до 7000 от 2018 года", resp(preferredBrands=["toyota"], maxBudgetPerHour=7000, minYear=2018)),
        ("бизнес тойота от 2020 года на автомате", resp(preferredStyles=["business"], preferredBrands=["toyota"], minYear=2020, transmission="automatic")),
        ("городскую до 5000 с рейтингом от 4.5", resp(preferredStyles=["city"], maxBudgetPerHour=5000, minRating=4.5)),
        ("кобальт до 5000 от 2020 на автомате", resp(preferredBrands=["cobalt"], maxBudgetPerHour=5000, minYear=2020, transmission="automatic")),
        ("камри на механике с рейтингом от 4", resp(preferredBrands=["camry"], transmission="manual", minRating=4.0)),
        ("luxury under 10000, 2020+, automatic", resp(preferredStyles=["luxury"], maxBudgetPerHour=10000, minYear=2020, transmission="automatic")),
        ("cheap family car for 5 people", resp(preferredStyles=["family"], maxBudgetPerHour=5000, passengers=5)),
        ("ниссан скайлайн от 2000 года", resp(preferredBrands=["nissan"], minYear=2000)),
        ("шевроле кобальт до 4000 тенге", resp(preferredBrands=["chevrolet"], maxBudgetPerHour=4000)),
    ]
    for q, r in examples:
        out.append(ex(q, r))
    return out


# ---------------------------------------------------------------------------
# 5. Negative — empty filters
# ---------------------------------------------------------------------------
def gen_negative() -> list[dict]:
    out = []
    for q in [
        "привет", "здравствуйте", "hello", "hi", "добрый день",
        "аолдфа", "dfghj", "йцукен", "asdfgh", "кпрнгш", "qqqqq",
        "как дела", "что умеешь?", "ты кто?", "расскажи о себе",
        "какая погода", "what can you do", "how are you",
        "ну", "ок", "хм", "да", "нет", "ok", "yes", "no",
        "как арендовать", "какие документы нужны", "где вернуть",
        "что есть?", "какие машины?", "что можете предложить?",
        "покажи все", "все варианты",
    ]:
        out.append(ex(q, resp()))
    return out


# ---------------------------------------------------------------------------
# 6. ANTI-HALLUCINATION — model/brand mentioned alone should NOT fill
# transmission, year, budget or rating. This is the #1 source of real-world
# errors ("нужна камри" incorrectly gets transmission=manual).
# ---------------------------------------------------------------------------
def gen_anti_hallucination(cars: list[Car]) -> list[dict]:
    out = []
    # Cyrillic aliases — use only brand name in response, no other filters.
    cyrillic_models = [
        ("камри", "toyota"), ("кобальт", "chevrolet"), ("супра", "toyota"),
        ("королла", "toyota"), ("скайлайн", "nissan"), ("мазда", "mazda"),
        ("ауди", "audi"), ("мерседес", "mercedes"), ("киа", "kia"),
    ]
    templates = [
        "{m}", "нужна {m}", "есть {m}", "есть {m}?", "покажи {m}",
        "хочу {m}", "ищу {m}", "а {m} есть?", "подбери {m}",
        "есть у вас {m}", "{m} хочу", "дай {m}", "{m} плиз",
        "{m} нужна", "можно {m}?", "{m} пожалуйста",
        "привет, есть {m}?", "здравствуйте, {m} есть?",
        "добрый день, нужна {m}", "хай, {m} есть в наличии?",
    ]
    for alias, brand in cyrillic_models:
        for tmpl in templates:
            q = tmpl.format(m=alias)
            out.append(ex(q, resp(preferredBrands=[brand])))

    # Latin model names alone — no year/transmission/budget inference.
    seen_models = set()
    for car in cars:
        model_lower = car.model.lower()
        if model_lower in seen_models:
            continue
        seen_models.add(model_lower)
        brand_lower = car.brand.lower()
        for tmpl in [
            "{m}", "need {m}", "i need {m}", "want {m}", "show me {m}",
            "есть {m}?", "{m} please", "any {m}?",
            "привет, есть {m}?", "hi, do you have {m}?",
        ]:
            q = tmpl.format(m=model_lower)
            out.append(ex(q, resp(preferredBrands=[brand_lower])))

    # Brand-only mentions (no model).
    for brand in ["toyota", "chevrolet", "audi", "mercedes", "bmw", "kia",
                  "nissan", "mazda", "honda", "hyundai", "ford", "lexus"]:
        for tmpl in [
            "{b}", "нужен {b}", "есть {b}?", "хочу {b}",
            "показать {b}", "подбери {b}", "{b} plz", "need {b}",
        ]:
            out.append(ex(tmpl.format(b=brand), resp(preferredBrands=[brand])))

    return out


# ---------------------------------------------------------------------------
# 7. Colloquial & typo-ridden real queries
# ---------------------------------------------------------------------------
def gen_colloquial() -> list[dict]:
    out = []
    # Common typos/slang — brand/model intent should still extract.
    typo_cases = [
        ("хачу камри", resp(preferredBrands=["toyota"])),
        ("хочю кобальт", resp(preferredBrands=["chevrolet"])),
        ("нада тойоту", resp(preferredBrands=["toyota"])),
        ("падгани kamry", resp(preferredBrands=["toyota"])),
        ("камрик есть?", resp(preferredBrands=["toyota"])),
        ("кобалт нужен", resp(preferredBrands=["chevrolet"])),
        ("шеврик", resp(preferredBrands=["chevrolet"])),
        ("мэрс есть?", resp(preferredBrands=["mercedes"])),
        ("БМВ хочу", resp(preferredBrands=["bmw"])),
        ("AUDI", resp(preferredBrands=["audi"])),
        ("Toyota Camry", resp(preferredBrands=["toyota"])),
        ("Chevrolet  Cobalt", resp(preferredBrands=["chevrolet"])),  # extra space
    ]
    for q, r in typo_cases:
        out.append(ex(q, r))
    return out


# ---------------------------------------------------------------------------
# 8. Conversational / superlative — must stay within schema
# ---------------------------------------------------------------------------
def gen_conversational() -> list[dict]:
    out = []
    # "Cheapest / most affordable" — no numeric budget, so leave null.
    for q in ["самая дешёвая", "подешевле", "что подешевле есть?",
              "самый бюджетный вариант", "cheapest", "affordable"]:
        out.append(ex(q, resp()))

    # "Cheap" as a style keyword — budget stays null (no concrete number).
    for q in ["дешёвая машина", "бюджетная", "что-нибудь недорогое",
              "cheap car", "inexpensive"]:
        out.append(ex(q, resp()))

    # Tenure phrases — must NOT leak into budget or year.
    tenure = [
        "на выходные", "на неделю", "на день", "на месяц", "на сутки",
        "на час", "на 3 дня", "на 5 дней", "for a weekend", "for a day",
    ]
    for q in tenure:
        out.append(ex(q, resp()))

    # Combined: "камри на выходные" — only brand, NOT budget/time.
    for alias, brand in [("камри", "toyota"), ("кобальт", "chevrolet"),
                         ("супру", "toyota")]:
        for phrase in ["на выходные", "на неделю", "на день"]:
            out.append(ex(f"{alias} {phrase}", resp(preferredBrands=[brand])))

    # Reassurance/clarification — empty.
    for q in ["не знаю что", "что посоветуете?", "посоветуй",
              "что-нибудь нормальное", "любую", "всё равно какую"]:
        out.append(ex(q, resp()))

    return out


# ---------------------------------------------------------------------------
# 9. Mixed-language and multi-query
# ---------------------------------------------------------------------------
def gen_mixed_language() -> list[dict]:
    out = []
    cases = [
        ("нужна camry", resp(preferredBrands=["toyota"])),
        ("I need камри", resp(preferredBrands=["toyota"])),
        ("хочу Toyota Camry 2020+", resp(preferredBrands=["toyota"], minYear=2020)),
        ("Chevrolet кобальт на автомате", resp(preferredBrands=["chevrolet"], transmission="automatic")),
        ("mercedes с хорошим рейтингом", resp(preferredBrands=["mercedes"], minRating=4.0)),
        ("luxury ауди до 10000", resp(preferredBrands=["audi"], preferredStyles=["luxury"], maxBudgetPerHour=10000)),
        ("семейную toyota", resp(preferredBrands=["toyota"], preferredStyles=["family"])),
    ]
    for q, r in cases:
        out.append(ex(q, r))
    return out


# ---------------------------------------------------------------------------
# 10. Negation — "not X", "except X"
# ---------------------------------------------------------------------------
def gen_negation() -> list[dict]:
    out = []
    cases = [
        ("не хочу спортивную", resp(excludedStyles=["sport"])),
        ("без спорткаров", resp(excludedStyles=["sport"])),
        ("кроме бизнес класса", resp(excludedStyles=["business"])),
        ("any except luxury", resp(excludedStyles=["luxury"])),
        ("не люкс", resp(excludedStyles=["luxury"])),
        ("не семейную но спортивную", resp(preferredStyles=["sport"], excludedStyles=["family"])),
    ]
    for q, r in cases:
        out.append(ex(q, r))
    return out


def main():
    parser = argparse.ArgumentParser()
    parser.add_argument("--db-url", default=None, help="PostgreSQL URL (requires psycopg2)")
    parser.add_argument("--container", default=None, help="Docker container name for ai-search-db")
    parser.add_argument("--output", default="dataset.jsonl")
    args = parser.parse_args()

    print("Loading catalog...")
    cars = load_catalog(args.db_url, args.container)
    print(f"Loaded {len(cars)} cars from catalog")

    sections = {
        "transliteration": gen_transliteration(),
        "catalog_direct": gen_catalog_direct(cars),
        "year": gen_year(),
        "budget": gen_budget(),
        "styles": gen_styles(),
        "other_filters": gen_other_filters(),
        "combined": gen_combined(),
        "negative": gen_negative(),
        "anti_hallucination": gen_anti_hallucination(cars),
        "colloquial": gen_colloquial(),
        "conversational": gen_conversational(),
        "mixed_language": gen_mixed_language(),
        "negation": gen_negation(),
    }

    all_examples = []
    for name, examples in sections.items():
        print(f"  {name}: {len(examples)} examples")
        all_examples.extend(examples)

    # Duplicate critical sections for training balance. Anti-hallucination
    # is the most failure-prone skill, so 3x weight; year/negative 2x.
    all_examples.extend(sections["anti_hallucination"])
    all_examples.extend(sections["anti_hallucination"])
    all_examples.extend(sections["year"])
    all_examples.extend(sections["negative"])
    all_examples.extend(sections["transliteration"])

    random.shuffle(all_examples)

    with open(args.output, "w", encoding="utf-8") as f:
        for example in all_examples:
            f.write(json.dumps(example, ensure_ascii=False) + "\n")

    print(f"\nTotal: {len(all_examples)} examples → {args.output}")


if __name__ == "__main__":
    main()
