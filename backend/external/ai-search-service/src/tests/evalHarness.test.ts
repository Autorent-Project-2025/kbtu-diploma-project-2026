/**
 * Offline evaluation harness for the recommendation service.
 *
 * Unlike the assertion-based suite in llmRecommendations.test.ts, this
 * script measures *quality* with ranking metrics (recall@k, precision@k,
 * MRR). Use it to compare models/retrieval changes: run before and after
 * a change, compare the overall scores.
 *
 * Golden set format: each query declares which partner_car_ids (or
 * brand+model pairs, resolved via catalog) SHOULD appear in the top-k.
 *
 * Usage:
 *   docker compose up -d ai-search-service
 *   npx tsc && node dist/tests/evalHarness.test.js
 *
 * Env:
 *   AI_TEST_BASE_URL  — default http://localhost:9186
 *   EVAL_K            — top-k to evaluate (default 5)
 */

type RecommendationCar = {
  partnerCarId: number;
  brand: string;
  model: string;
};

type RecommendationResponse = {
  cars: RecommendationCar[];
  appliedFilters: Record<string, unknown>;
};

type GoldenQuery = {
  id: string;
  prompt: string;
  relevantBrandModels: Array<{ brand: string; model?: string }>;
  mustAppearFirst?: { brand: string; model?: string };
  tags?: string[];
};

const BASE_URL = process.env.AI_TEST_BASE_URL ?? "http://localhost:9186";
const K = Number(process.env.EVAL_K ?? 5);
const TIMEOUT_MS = 60000;

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
    if (!res.ok) throw new Error(`HTTP ${res.status}`);
    return (await res.json()) as RecommendationResponse;
  } finally {
    clearTimeout(timer);
  }
}

function matches(car: RecommendationCar, target: { brand: string; model?: string }): boolean {
  if (car.brand.toLowerCase() !== target.brand.toLowerCase()) return false;
  if (target.model && car.model.toLowerCase() !== target.model.toLowerCase()) return false;
  return true;
}

function computeRecallAtK(cars: RecommendationCar[], relevant: GoldenQuery["relevantBrandModels"], k: number): number {
  if (relevant.length === 0) return 1;
  const topK = cars.slice(0, k);
  const hits = relevant.filter((target) => topK.some((car) => matches(car, target))).length;
  return hits / relevant.length;
}

function computePrecisionAtK(cars: RecommendationCar[], relevant: GoldenQuery["relevantBrandModels"], k: number): number {
  const topK = cars.slice(0, k);
  if (topK.length === 0) return 0;
  const hits = topK.filter((car) => relevant.some((target) => matches(car, target))).length;
  return hits / topK.length;
}

function computeReciprocalRank(cars: RecommendationCar[], mustAppearFirst?: GoldenQuery["mustAppearFirst"]): number {
  if (!mustAppearFirst) return 1;
  const idx = cars.findIndex((car) => matches(car, mustAppearFirst));
  return idx >= 0 ? 1 / (idx + 1) : 0;
}

// Golden set: 30 queries covering common intents against the current catalog
// (Audi A6, Chevrolet Cobalt, Kia K5, Mazda RX7, Mercedes S500, Nissan
// Skyline, Toyota Camry/Corolla/Supra).
const GOLDEN: GoldenQuery[] = [
  // --- Exact model queries (single relevant) ---
  { id: "exact-cobalt-latin", prompt: "cobalt", relevantBrandModels: [{ brand: "chevrolet", model: "cobalt" }], mustAppearFirst: { brand: "chevrolet", model: "cobalt" } },
  { id: "exact-cobalt-cyrillic", prompt: "кобальт", relevantBrandModels: [{ brand: "chevrolet", model: "cobalt" }], mustAppearFirst: { brand: "chevrolet", model: "cobalt" } },
  { id: "exact-camry-latin", prompt: "camry", relevantBrandModels: [{ brand: "toyota", model: "camry" }], mustAppearFirst: { brand: "toyota", model: "camry" } },
  { id: "exact-camry-cyrillic", prompt: "нужна камри", relevantBrandModels: [{ brand: "toyota", model: "camry" }], mustAppearFirst: { brand: "toyota", model: "camry" } },
  { id: "exact-supra-latin", prompt: "I need supra", relevantBrandModels: [{ brand: "toyota", model: "supra" }], mustAppearFirst: { brand: "toyota", model: "supra" } },
  { id: "exact-supra-cyrillic", prompt: "хочу супру", relevantBrandModels: [{ brand: "toyota", model: "supra" }], mustAppearFirst: { brand: "toyota", model: "supra" } },
  { id: "exact-skyline", prompt: "nissan skyline", relevantBrandModels: [{ brand: "nissan", model: "skyline" }], mustAppearFirst: { brand: "nissan", model: "skyline" } },
  { id: "exact-mercedes", prompt: "мерседес", relevantBrandModels: [{ brand: "mercedes-benz" }], mustAppearFirst: { brand: "mercedes-benz" } },
  { id: "exact-audi-a6", prompt: "audi a6", relevantBrandModels: [{ brand: "audi", model: "a6" }], mustAppearFirst: { brand: "audi", model: "a6" } },
  { id: "exact-kia-k5", prompt: "kia k5", relevantBrandModels: [{ brand: "kia", model: "k5" }], mustAppearFirst: { brand: "kia", model: "k5" } },

  // --- Brand-only queries (multi-relevant) ---
  { id: "brand-toyota", prompt: "toyota", relevantBrandModels: [
    { brand: "toyota", model: "camry" }, { brand: "toyota", model: "corolla" }, { brand: "toyota", model: "supra" },
  ] },
  { id: "brand-chevrolet", prompt: "шевроле", relevantBrandModels: [{ brand: "chevrolet", model: "cobalt" }] },

  // --- Style queries (tag-based relevance) ---
  { id: "style-sport", prompt: "спортивную машину", relevantBrandModels: [
    { brand: "toyota", model: "supra" }, { brand: "mazda", model: "rx7" }, { brand: "nissan", model: "skyline" },
  ] },
  { id: "style-luxury", prompt: "люксовую", relevantBrandModels: [
    { brand: "mercedes-benz", model: "s 500" }, { brand: "audi", model: "a6" },
  ] },
  { id: "style-business", prompt: "бизнес класс", relevantBrandModels: [
    { brand: "audi", model: "a6" }, { brand: "mercedes-benz", model: "s 500" },
    { brand: "kia", model: "k5" }, { brand: "toyota", model: "camry" },
  ] },
  { id: "style-family", prompt: "семейную машину", relevantBrandModels: [
    { brand: "chevrolet", model: "cobalt" }, { brand: "toyota", model: "corolla" }, { brand: "toyota", model: "camry" },
  ] },
  { id: "style-city", prompt: "городскую машину", relevantBrandModels: [
    { brand: "chevrolet", model: "cobalt" }, { brand: "toyota", model: "corolla" }, { brand: "kia", model: "k5" },
  ] },

  // --- Filter-based queries ---
  { id: "budget-500", prompt: "до 500 в час", relevantBrandModels: [
    { brand: "chevrolet", model: "cobalt" }, { brand: "nissan", model: "skyline" },
  ] },
  { id: "budget-cheap", prompt: "самая дешёвая", relevantBrandModels: [
    { brand: "chevrolet", model: "cobalt" }, { brand: "nissan", model: "skyline" }, { brand: "toyota", model: "camry" },
  ] },
  { id: "year-2020plus", prompt: "от 2020 года", relevantBrandModels: [
    { brand: "chevrolet", model: "cobalt" }, { brand: "kia", model: "k5" },
  ] },
  { id: "year-vintage", prompt: "до 2000 года", relevantBrandModels: [
    { brand: "mazda", model: "rx7" }, { brand: "toyota", model: "supra" },
  ] },
  { id: "transmission-manual", prompt: "на механике", relevantBrandModels: [
    { brand: "chevrolet", model: "cobalt" }, { brand: "toyota", model: "supra" }, { brand: "mazda", model: "rx7" },
  ] },

  // --- Combined filters ---
  { id: "combo-cheap-automatic", prompt: "дешёвый на автомате", relevantBrandModels: [
    { brand: "nissan", model: "skyline" }, { brand: "toyota", model: "camry" }, { brand: "toyota", model: "corolla" },
  ] },
  { id: "combo-toyota-automatic", prompt: "toyota автомат", relevantBrandModels: [
    { brand: "toyota", model: "camry" }, { brand: "toyota", model: "corolla" },
  ] },
  { id: "combo-sport-manual", prompt: "спорткар на механике", relevantBrandModels: [
    { brand: "toyota", model: "supra" }, { brand: "mazda", model: "rx7" },
  ] },

  // --- Conversational / greeting (accept any reasonable response) ---
  { id: "greet-ru", prompt: "привет", relevantBrandModels: [], mustAppearFirst: undefined },
  { id: "greet-en", prompt: "hello", relevantBrandModels: [], mustAppearFirst: undefined },

  // --- Multi-model brand filter ---
  { id: "multi-camry-or-corolla", prompt: "toyota седан", relevantBrandModels: [
    { brand: "toyota", model: "camry" }, { brand: "toyota", model: "corolla" },
  ] },

  // --- Edge: non-existent ---
  { id: "nonexistent-ferrari", prompt: "ferrari", relevantBrandModels: [] },
  { id: "nonexistent-gibberish", prompt: "лфжыдафлд", relevantBrandModels: [] },
];

type QueryResult = {
  id: string;
  prompt: string;
  recallK: number;
  precisionK: number;
  rr: number;
  latencyMs: number;
  top: string[];
  error?: string;
};

async function runEval() {
  const results: QueryResult[] = [];

  for (const golden of GOLDEN) {
    const startedAt = Date.now();
    let recallK = 0;
    let precisionK = 0;
    let rr = 0;
    let top: string[] = [];
    let error: string | undefined;

    try {
      const response = await callRecommendations(golden.prompt);
      const cars = response.cars ?? [];
      top = cars.slice(0, K).map((c) => `${c.brand} ${c.model}`);
      recallK = computeRecallAtK(cars, golden.relevantBrandModels, K);
      precisionK = golden.relevantBrandModels.length > 0
        ? computePrecisionAtK(cars, golden.relevantBrandModels, K)
        : (cars.length === 0 ? 1 : 0);
      rr = computeReciprocalRank(cars, golden.mustAppearFirst);
    } catch (e) {
      error = e instanceof Error ? e.message : String(e);
    }

    const latencyMs = Date.now() - startedAt;
    results.push({ id: golden.id, prompt: golden.prompt, recallK, precisionK, rr, latencyMs, top, error });
  }

  const report = (label: string, values: number[]) => {
    const avg = values.reduce((a, b) => a + b, 0) / (values.length || 1);
    console.log(`  ${label}: ${avg.toFixed(3)}`);
  };

  console.log(`\n=== Eval results (k=${K}, n=${results.length}) ===`);
  report(`recall@${K}`, results.map((r) => r.recallK));
  report(`precision@${K}`, results.map((r) => r.precisionK));
  report("MRR", results.map((r) => r.rr));
  console.log(`  avg latency: ${Math.round(results.reduce((a, b) => a + b.latencyMs, 0) / results.length)}ms`);
  console.log(`  p95 latency: ${Math.round([...results].map((r) => r.latencyMs).sort((a, b) => a - b)[Math.floor(results.length * 0.95)])}ms`);

  console.log("\n=== Per-query detail (sorted by recall) ===");
  const sorted = [...results].sort((a, b) => a.recallK - b.recallK);
  for (const r of sorted) {
    const flag = r.error ? "ERR" : r.recallK === 1 ? "OK " : r.recallK > 0 ? "PAR" : "MIS";
    console.log(`[${flag}] ${r.id.padEnd(28)} r=${r.recallK.toFixed(2)} p=${r.precisionK.toFixed(2)} rr=${r.rr.toFixed(2)} "${r.prompt}" -> ${r.top.slice(0, 3).join(", ")}`);
    if (r.error) console.log(`    error: ${r.error}`);
  }

  // Output a single JSON summary for scripting comparisons.
  const avg = (vs: number[]) => vs.reduce((a, b) => a + b, 0) / (vs.length || 1);
  const summary = {
    recallAtK: avg(results.map((r) => r.recallK)),
    precisionAtK: avg(results.map((r) => r.precisionK)),
    mrr: avg(results.map((r) => r.rr)),
    avgLatencyMs: Math.round(avg(results.map((r) => r.latencyMs))),
    total: results.length,
    failures: results
      .filter((r) => {
        const golden = GOLDEN.find((g) => g.id === r.id);
        return r.recallK === 0 && (golden?.relevantBrandModels?.length ?? 0) > 0;
      })
      .map((r) => r.id),
  };
  console.log("\n=== JSON summary ===");
  console.log(JSON.stringify(summary, null, 2));
}

runEval().catch((e) => {
  console.error(e);
  process.exit(1);
});
