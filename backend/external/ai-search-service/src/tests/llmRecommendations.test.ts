/**
 * Integration tests for the AI recommendation endpoint.
 * Runs against a live ai-search-service (through the api-gateway by default).
 *
 * Usage:
 *   docker compose up -d ai-search-service api-gateway ollama
 *   npx ts-node src/tests/llmRecommendations.test.ts
 *
 * Env:
 *   AI_TEST_BASE_URL  — defaults to http://localhost:9186
 *   AI_TEST_TIMEOUT   — per-request timeout ms (default 60000)
 */

type AppliedFilters = {
  transmission: string | null;
  minRating: number | null;
  maxBudgetPerHour: number | null;
  passengers: number | null;
  minYear: number | null;
  maxYear: number | null;
  preferredBrands: string[];
  preferredStyles: string[];
  excludedStyles: string[];
};

type RecommendationCar = {
  brand: string;
  model: string;
  year: number;
  priceHour: number | null;
  tags: string[];
};

type RecommendationResponse = {
  assistantText: string;
  appliedFilters: AppliedFilters;
  totalCandidates: number;
  cars: RecommendationCar[];
};

const BASE_URL = process.env.AI_TEST_BASE_URL ?? "http://localhost:9186";
const TIMEOUT_MS = Number(process.env.AI_TEST_TIMEOUT ?? 60000);

async function callRecommendations(prompt: string): Promise<RecommendationResponse> {
  const controller = new AbortController();
  const timer = setTimeout(() => controller.abort(), TIMEOUT_MS);
  try {
    const res = await fetch(`${BASE_URL}/ai/recommendations`, {
      method: "POST",
      headers: { "Content-Type": "application/json" },
      body: JSON.stringify({ prompt }),
      signal: controller.signal,
    });
    if (!res.ok) {
      throw new Error(`HTTP ${res.status} — ${await res.text()}`);
    }
    return (await res.json()) as RecommendationResponse;
  } finally {
    clearTimeout(timer);
  }
}

type Assertion = (r: RecommendationResponse) => string | null;

const expect = {
  brandsInclude(...brands: string[]): Assertion {
    return (r) => {
      const found = r.appliedFilters.preferredBrands.map((b) => b.toLowerCase());
      const missing = brands.filter((b) => !found.includes(b.toLowerCase()));
      return missing.length === 0
        ? null
        : `expected brands to include [${missing.join(", ")}], got [${found.join(", ")}]`;
    };
  },
  brandsExclude(...brands: string[]): Assertion {
    return (r) => {
      const found = r.appliedFilters.preferredBrands.map((b) => b.toLowerCase());
      const present = brands.filter((b) => found.includes(b.toLowerCase()));
      return present.length === 0
        ? null
        : `expected brands NOT to include [${present.join(", ")}]`;
    };
  },
  onlyBrand(brand: string): Assertion {
    return (r) => {
      const wrong = r.cars.filter((c) => c.brand.toLowerCase() !== brand.toLowerCase());
      return wrong.length === 0
        ? null
        : `expected only ${brand}, found ${wrong.map((c) => c.brand).join(", ")}`;
    };
  },
  hasCarWithModel(model: string): Assertion {
    return (r) => {
      const hit = r.cars.find((c) => c.model.toLowerCase() === model.toLowerCase());
      return hit ? null : `expected result to contain model "${model}"`;
    };
  },
  allPriceHourLe(limit: number): Assertion {
    return (r) => {
      const over = r.cars.filter((c) => c.priceHour != null && c.priceHour > limit);
      return over.length === 0
        ? null
        : `expected all priceHour <= ${limit}, violations: ${over
            .map((c) => `${c.brand} ${c.model}=${c.priceHour}`)
            .join(", ")}`;
    };
  },
  allTransmission(expected: string): Assertion {
    return (r) => {
      const bad = r.cars.filter(
        (c) => !c.tags.some((t) => t.toLowerCase() === expected.toLowerCase()),
      );
      return bad.length === 0
        ? null
        : `expected all cars to have transmission "${expected}", violations: ${bad
            .map((c) => `${c.brand} ${c.model}`)
            .join(", ")}`;
    };
  },
  allHaveTag(tag: string): Assertion {
    return (r) => {
      const bad = r.cars.filter(
        (c) => !c.tags.some((t) => t.toLowerCase() === tag.toLowerCase()),
      );
      return bad.length === 0
        ? null
        : `expected all cars to have tag "${tag}", violations: ${bad
            .map((c) => `${c.brand} ${c.model}`)
            .join(", ")}`;
    };
  },
  minYearGe(year: number): Assertion {
    return (r) => {
      const bad = r.cars.filter((c) => c.year < year);
      return bad.length === 0
        ? null
        : `expected year >= ${year}, violations: ${bad
            .map((c) => `${c.brand} ${c.model} ${c.year}`)
            .join(", ")}`;
    };
  },
  filterEquals<K extends keyof AppliedFilters>(key: K, expected: AppliedFilters[K]): Assertion {
    return (r) => {
      const actual = r.appliedFilters[key];
      return JSON.stringify(actual) === JSON.stringify(expected)
        ? null
        : `expected appliedFilters.${String(key)}=${JSON.stringify(expected)}, got ${JSON.stringify(actual)}`;
    };
  },
  filterIsNull<K extends keyof AppliedFilters>(key: K): Assertion {
    return (r) => {
      const actual = r.appliedFilters[key];
      if (actual === null) return null;
      if (Array.isArray(actual) && actual.length === 0) return null;
      return `expected appliedFilters.${String(key)} to be null/empty, got ${JSON.stringify(actual)}`;
    };
  },
  noCars(): Assertion {
    return (r) =>
      r.cars.length === 0 ? null : `expected no cars, got ${r.cars.length}`;
  },
  hasCars(): Assertion {
    return (r) => (r.cars.length > 0 ? null : "expected at least one car");
  },
};

type TestCase = {
  name: string;
  prompt: string;
  assertions: Assertion[];
};

const TESTS: TestCase[] = [
  {
    name: "model name only → correct brand, no hallucinated filters",
    prompt: "есть cobalt",
    assertions: [
      expect.brandsInclude("chevrolet"),
      expect.onlyBrand("chevrolet"),
      expect.filterIsNull("transmission"),
      expect.filterIsNull("minYear"),
      expect.filterIsNull("maxBudgetPerHour"),
    ],
  },
  {
    name: "cyrillic transliteration камри → Toyota Camry",
    prompt: "нужна камри",
    assertions: [
      expect.brandsInclude("toyota"),
      expect.hasCarWithModel("camry"),
    ],
  },
  {
    name: "budget constraint до 600 → no cars above 600/hr",
    prompt: "машина до 600 в час",
    assertions: [
      expect.filterEquals("maxBudgetPerHour", 600),
      expect.allPriceHourLe(600 * 1.25),
      expect.hasCars(),
    ],
  },
  {
    name: "sport style → only sport-tagged cars (Supra/RX7/Skyline)",
    prompt: "хочу спортивную машину",
    assertions: [
      expect.filterEquals("preferredStyles", ["sport"]),
      expect.allHaveTag("sport"),
    ],
  },
  {
    name: "automatic transmission → all cars with automatic",
    prompt: "машина с автоматической коробкой",
    assertions: [
      expect.filterEquals("transmission", "automatic"),
      expect.allTransmission("automatic"),
    ],
  },
  {
    name: "year lower bound от 2020 → only 2020+",
    prompt: "машина не старше 2020 года",
    assertions: [expect.filterEquals("minYear", 2020), expect.minYearGe(2020)],
  },
  {
    name: "luxury style → luxury-tagged (Audi A6, Mercedes S 500)",
    prompt: "хочу люксовую машину",
    assertions: [
      expect.filterEquals("preferredStyles", ["luxury"]),
      expect.allHaveTag("luxury"),
    ],
  },
  {
    name: "greeting → no filters, may return no cars",
    prompt: "привет",
    assertions: [
      expect.filterIsNull("transmission"),
      expect.filterIsNull("minYear"),
      expect.filterIsNull("maxBudgetPerHour"),
      expect.filterIsNull("preferredBrands"),
      expect.filterIsNull("preferredStyles"),
    ],
  },
  {
    name: "combined: camry automatic от 2010",
    prompt: "camry автомат от 2010 года",
    assertions: [
      expect.brandsInclude("toyota"),
      expect.filterEquals("transmission", "automatic"),
      expect.filterEquals("minYear", 2010),
      expect.minYearGe(2010),
    ],
  },
  {
    name: "non-existent model — no cars returned",
    prompt: "есть ferrari",
    assertions: [
      expect.brandsInclude("ferrari"),
      expect.noCars(),
    ],
  },
  {
    name: "rating filter рейтинг больше 4.5 → only high-rated",
    prompt: "машина с рейтингом больше 4.5",
    assertions: [
      expect.filterEquals("minRating", 4.5),
    ],
  },
  {
    name: "english query 'I need supra' → Toyota Supra",
    prompt: "I need supra",
    assertions: [
      expect.brandsInclude("toyota"),
      expect.hasCarWithModel("supra"),
    ],
  },
];

async function runTests() {
  const results: { name: string; ok: boolean; errors: string[]; durationMs: number }[] = [];
  for (const test of TESTS) {
    const startedAt = Date.now();
    const errors: string[] = [];
    try {
      const response = await callRecommendations(test.prompt);
      for (const assert of test.assertions) {
        const error = assert(response);
        if (error) errors.push(error);
      }
    } catch (error) {
      errors.push(`request failed: ${error instanceof Error ? error.message : String(error)}`);
    }
    const durationMs = Date.now() - startedAt;
    const ok = errors.length === 0;
    results.push({ name: test.name, ok, errors, durationMs });
    const prefix = ok ? "PASS" : "FAIL";
    // eslint-disable-next-line no-console
    console.log(`[${prefix}] (${durationMs}ms) ${test.name}`);
    for (const err of errors) {
      // eslint-disable-next-line no-console
      console.log(`       ${err}`);
    }
  }

  const passed = results.filter((r) => r.ok).length;
  const total = results.length;
  // eslint-disable-next-line no-console
  console.log(`\n${passed}/${total} tests passed`);
  if (passed < total) {
    process.exitCode = 1;
  }
}

runTests().catch((error) => {
  // eslint-disable-next-line no-console
  console.error("Test runner crashed:", error);
  process.exit(1);
});
