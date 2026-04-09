import { ParsedRecommendationQuery } from "../types";

const styleDictionary: Array<{ label: string; variants: string[] }> = [
  {
    label: "sport",
    variants: ["sport", "sports", "спортив", "спорт", "спорткар", "купе", "coupe"],
  },
  { label: "business", variants: ["business", "бизнес", "делов", "meeting", "airport", "аэропорт"] },
  { label: "family", variants: ["family", "сем", "дет", "багаж", "children"] },
  { label: "city", variants: ["city", "город", "urban", "ежеднев", "парковк"] },
  { label: "luxury", variants: ["luxury", "premium", "люкс", "премиум"] },
];

const transmissionDictionary: Array<{ label: string; variants: string[] }> = [
  { label: "automatic", variants: ["automatic", "автомат", "акпп"] },
  { label: "manual", variants: ["manual", "механик", "мкпп"] },
];

const brandDictionary = [
  "toyota",
  "nissan",
  "kia",
  "mazda",
  "bmw",
  "mercedes",
  "lexus",
  "audi",
];

function normalizePrompt(prompt: string): string {
  return prompt.toLowerCase().replace(/\s+/g, " ").trim();
}

export function canonicalizeStyleLabel(value: string): string | null {
  const normalized = normalizePrompt(value);
  if (!normalized) {
    return null;
  }

  return (
    styleDictionary.find(
      (item) =>
        item.label === normalized ||
        item.variants.some((variant) => normalized.includes(variant)),
    )?.label ?? null
  );
}

export function canonicalizeTransmissionLabel(value: string): string | null {
  const normalized = normalizePrompt(value);
  if (!normalized) {
    return null;
  }

  return (
    transmissionDictionary.find(
      (item) =>
        item.label === normalized ||
        item.variants.some((variant) => normalized.includes(variant)),
    )?.label ?? null
  );
}

function parseBudget(prompt: string): number | null {
  const normalized = normalizePrompt(prompt);
  const compact = normalized.replace(/\s+/g, "");
  const thousandMatch = compact.match(/до(\d+(?:[.,]\d+)?)тыс/);
  if (thousandMatch) {
    return Math.round(Number(thousandMatch[1].replace(",", ".")) * 1000);
  }

  const currencyMatch = compact.match(/до(\d{3,6})/);
  if (currencyMatch) {
    return Number(currencyMatch[1]);
  }

  return null;
}

function parsePassengers(prompt: string): number | null {
  const normalized = normalizePrompt(prompt);
  const match = normalized.match(/(\d+)\s*(чел|человек|people|passenger|пассаж)/);
  return match ? Number(match[1]) : null;
}

export function hasExplicitYearIntent(prompt: string): boolean {
  const normalized = normalizePrompt(prompt);
  return (
    /(?:\b(19\d{2}|20\d{2})\s*(?:г|г\.|год|года|year)\b)/.test(normalized) ||
    /(?:\b(?:от|с|после|не старше|не ниже|начиная с)\b[^\d]{0,12}(19\d{2}|20\d{2}))/u.test(
      normalized,
    ) ||
    /\b(19\d{2}|20\d{2})\s*\+\b/.test(normalized)
  );
}

function parseMinYear(prompt: string): number | null {
  const normalized = normalizePrompt(prompt);
  if (!hasExplicitYearIntent(normalized)) {
    return null;
  }

  const patterns = [
    /\b(19\d{2}|20\d{2})\s*(?:г|г\.|год|года|year)\b/,
    /\b(?:от|с|после|не старше|не ниже|начиная с)\b[^\d]{0,12}(19\d{2}|20\d{2})/u,
    /\b(19\d{2}|20\d{2})\s*\+\b/,
  ];

  for (const pattern of patterns) {
    const match = normalized.match(pattern);
    if (match) {
      return Number(match[1]);
    }
  }

  return null;
}

function parsePreferredStyles(prompt: string): string[] {
  const normalized = normalizePrompt(prompt);
  return [...new Set(
    styleDictionary
      .map((item) => canonicalizeStyleLabel(item.variants.find((variant) => normalized.includes(variant)) ?? ""))
      .filter((item): item is string => Boolean(item)),
  )];
}

function parseTransmission(prompt: string): string | null {
  const normalized = normalizePrompt(prompt);
  return canonicalizeTransmissionLabel(normalized);
}

function parsePreferredBrands(prompt: string): string[] {
  const normalized = normalizePrompt(prompt);
  return brandDictionary.filter((brand) => normalized.includes(brand));
}

export function parseQueryHeuristically(prompt: string): ParsedRecommendationQuery {
  const normalized = normalizePrompt(prompt);

  return {
    prompt,
    maxBudgetPerHour: parseBudget(normalized),
    passengers: parsePassengers(normalized),
    transmission: parseTransmission(normalized),
    preferredStyles: parsePreferredStyles(normalized),
    preferredBrands: parsePreferredBrands(normalized),
    minYear: parseMinYear(normalized),
    startTime: null,
    endTime: null,
    requiresAvailableOnDates: false,
  };
}
