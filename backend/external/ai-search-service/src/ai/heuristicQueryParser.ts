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

function isVariantNegated(normalizedPrompt: string, variant: string): boolean {
  return (
    normalizedPrompt.includes(`не ${variant}`) ||
    normalizedPrompt.includes(`без ${variant}`) ||
    normalizedPrompt.includes(`кроме ${variant}`)
  );
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

function parseMinRating(prompt: string): number | null {
  const normalized = normalizePrompt(prompt).replace(/,/g, ".");
  const hasRatingKeyword = /(рейтинг|rating|оценк|зв[её]зд|stars?)/u.test(normalized);
  if (!hasRatingKeyword) {
    return null;
  }

  const comparisonMatch = normalized.match(
    /(?:от|выше(?: чем)?|больше(?: чем)?|не меньше|свыше|above|over|at least|>=?)\s*(\d(?:\.\d+)?)/u,
  );
  if (comparisonMatch) {
    const parsed = Number(comparisonMatch[1]);
    return parsed >= 0 && parsed <= 5 ? parsed : null;
  }

  const plusMatch = normalized.match(/(\d(?:\.\d+)?)\s*\+/u);
  if (plusMatch) {
    const parsed = Number(plusMatch[1]);
    return parsed >= 0 && parsed <= 5 ? parsed : null;
  }

  const bareMatch = normalized.match(/(\d(?:\.\d+)?)/u);
  if (bareMatch) {
    const parsed = Number(bareMatch[1]);
    return parsed >= 0 && parsed <= 5 ? parsed : null;
  }

  return null;
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
      .filter((item) =>
        item.variants.some(
          (variant) => normalized.includes(variant) && !isVariantNegated(normalized, variant),
        ),
      )
      .map((item) => item.label),
  )];
}

function parseExcludedStyles(prompt: string): string[] {
  const normalized = normalizePrompt(prompt);
  return [...new Set(
    styleDictionary
      .filter((item) => item.variants.some((variant) => isVariantNegated(normalized, variant)))
      .map((item) => item.label),
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
  const excludedStyles = parseExcludedStyles(normalized);
  const preferredStyles = parsePreferredStyles(normalized).filter(
    (style) => !excludedStyles.includes(style),
  );

  return {
    prompt,
    maxBudgetPerHour: parseBudget(normalized),
    passengers: parsePassengers(normalized),
    transmission: parseTransmission(normalized),
    minRating: parseMinRating(normalized),
    preferredStyles,
    excludedStyles,
    preferredBrands: parsePreferredBrands(normalized),
    minYear: parseMinYear(normalized),
    startTime: null,
    endTime: null,
    requiresAvailableOnDates: false,
  };
}
