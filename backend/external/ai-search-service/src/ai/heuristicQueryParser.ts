import { ParsedRecommendationQuery } from "../types";
import {
  getBrandDictionary,
  getModelToBrandDictionary,
  STYLE_DICTIONARY,
  TRANSMISSION_DICTIONARY,
} from "../queryTaxonomy";

function normalizePrompt(prompt: string): string {
  return prompt.toLowerCase().replace(/\s+/g, " ").trim();
}

function padDateTimePart(value: number): string {
  return String(value).padStart(2, "0");
}

function isValidDateTime(
  year: number,
  month: number,
  day: number,
  hour: number,
  minute: number,
  second: number,
): boolean {
  const candidate = new Date(Date.UTC(year, month - 1, day, hour, minute, second));

  return (
    candidate.getUTCFullYear() === year &&
    candidate.getUTCMonth() === month - 1 &&
    candidate.getUTCDate() === day &&
    candidate.getUTCHours() === hour &&
    candidate.getUTCMinutes() === minute &&
    candidate.getUTCSeconds() === second
  );
}

function toLocalIsoDateTime(
  year: number,
  month: number,
  day: number,
  hour: number,
  minute: number,
  second: number,
): string {
  return `${year}-${padDateTimePart(month)}-${padDateTimePart(day)}T${padDateTimePart(hour)}:${padDateTimePart(minute)}:${padDateTimePart(second)}`;
}

function isChronologicalRange(startTime: string, endTime: string): boolean {
  const startTimestamp = Date.parse(startTime);
  const endTimestamp = Date.parse(endTime);

  return Number.isFinite(startTimestamp) && Number.isFinite(endTimestamp) && startTimestamp < endTimestamp;
}

function parseExplicitDateTimeToken(rawValue: string): string | null {
  const value = rawValue.trim().replace(/\s+/g, " ");

  const isoLikeMatch = value.match(
    /^(\d{4})[-/.](\d{2})[-/.](\d{2})[ t](\d{2}):(\d{2})(?::(\d{2}))?(z|[+-]\d{2}:\d{2})?$/i,
  );
  if (isoLikeMatch) {
    const year = Number(isoLikeMatch[1]);
    const month = Number(isoLikeMatch[2]);
    const day = Number(isoLikeMatch[3]);
    const hour = Number(isoLikeMatch[4]);
    const minute = Number(isoLikeMatch[5]);
    const second = Number(isoLikeMatch[6] ?? "00");
    const offset = isoLikeMatch[7]?.toUpperCase() ?? "";

    if (!isValidDateTime(year, month, day, hour, minute, second)) {
      return null;
    }

    return `${toLocalIsoDateTime(year, month, day, hour, minute, second)}${offset}`;
  }

  const localizedMatch = value.match(
    /^(\d{2})\.(\d{2})\.(\d{4})\s+(\d{2}):(\d{2})(?::(\d{2}))?$/,
  );
  if (!localizedMatch) {
    return null;
  }

  const day = Number(localizedMatch[1]);
  const month = Number(localizedMatch[2]);
  const year = Number(localizedMatch[3]);
  const hour = Number(localizedMatch[4]);
  const minute = Number(localizedMatch[5]);
  const second = Number(localizedMatch[6] ?? "00");

  if (!isValidDateTime(year, month, day, hour, minute, second)) {
    return null;
  }

  return toLocalIsoDateTime(year, month, day, hour, minute, second);
}

function parseAvailabilityRange(prompt: string): {
  startTime: string;
  endTime: string;
  requiresAvailableOnDates: boolean;
} | null {
  const dateTimeTokenPattern =
    "(?:\\d{4}[-/.]\\d{2}[-/.]\\d{2}[ T]\\d{2}:\\d{2}(?::\\d{2})?(?:Z|[+-]\\d{2}:\\d{2})?|\\d{2}\\.\\d{2}\\.\\d{4}\\s+\\d{2}:\\d{2}(?::\\d{2})?)";
  const patterns = [
    new RegExp(
      `(?:\\bс\\b|\\bfrom\\b)\\s*(${dateTimeTokenPattern})\\s*(?:\\bпо\\b|\\bдо\\b|\\bto\\b|[-–—])\\s*(${dateTimeTokenPattern})`,
      "iu",
    ),
    new RegExp(
      `(${dateTimeTokenPattern})\\s*(?:\\bпо\\b|\\bдо\\b|\\bto\\b|[-–—])\\s*(${dateTimeTokenPattern})`,
      "iu",
    ),
  ];

  for (const pattern of patterns) {
    const match = prompt.match(pattern);
    if (!match) {
      continue;
    }

    const startTime = parseExplicitDateTimeToken(match[1]);
    const endTime = parseExplicitDateTimeToken(match[2]);
    if (!startTime || !endTime || !isChronologicalRange(startTime, endTime)) {
      continue;
    }

    return {
      startTime,
      endTime,
      requiresAvailableOnDates: true,
    };
  }

  return null;
}

function isVariantNegated(normalizedPrompt: string, variant: string): boolean {
  return (
    normalizedPrompt.includes(`не ${variant}`) ||
    normalizedPrompt.includes(`без ${variant}`) ||
    normalizedPrompt.includes(`кроме ${variant}`)
  );
}

export function hasExplicitPreferredStyleIntent(
  prompt: string,
  preferredStyles: string[],
): boolean {
  const normalizedPrompt = normalizePrompt(prompt);

  return preferredStyles.some((style) => {
    const styleEntry = STYLE_DICTIONARY.find((item) => item.label === style);
    if (!styleEntry) {
      return false;
    }

    return styleEntry.variants.some(
      (variant) =>
        normalizedPrompt.includes(variant) && !isVariantNegated(normalizedPrompt, variant),
    );
  });
}

export function canonicalizeStyleLabel(value: string): string | null {
  const normalized = normalizePrompt(value);
  if (!normalized) {
    return null;
  }

  return (
    STYLE_DICTIONARY.find(
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
    TRANSMISSION_DICTIONARY.find(
      (item) =>
        item.label === normalized ||
        item.variants.some((variant) => normalized.includes(variant)),
    )?.label ?? null
  );
}

function parseBudget(prompt: string): number | null {
  const normalized = normalizePrompt(prompt);
  const compact = normalized.replace(/\s+/g, "");
  const thousandMatches = compact.matchAll(/до(\d+(?:[.,]\d+)?)тыс/gu);
  for (const match of thousandMatches) {
    return Math.round(Number(match[1].replace(",", ".")) * 1000);
  }

  const currencyMatches = compact.matchAll(/до(\d{3,6})(?:₸|тенге|kzt|тг|tg)?/gu);
  for (const match of currencyMatches) {
    const matchStart = match.index ?? 0;
    const matchEnd = matchStart + match[0].length;
    const trailingText = compact.slice(matchEnd);
    if (/^(?:г|г\.|год|года|year)/u.test(trailingText)) {
      continue;
    }

    return Number(match[1]);
  }

  return null;
}

function parsePassengers(prompt: string): number | null {
  const normalized = normalizePrompt(prompt);
  const match = normalized.match(/(\d+)\s*(чел|человек|people|passenger|пассаж|мест|места)/);
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
    /(?:\b(?:от|с|после|не старше|не ниже|начиная с|до|по|не новее|не позже|не позднее)\b[^\d]{0,12}(19\d{2}|20\d{2}))/u.test(
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
    /\b(?:от|с|после|не старше|не ниже|начиная с)\b[^\d]{0,12}(19\d{2}|20\d{2})(?:\s*(?:г|г\.|год|года|year))?\b/u,
    /\b(19\d{2}|20\d{2})\s*\+\b/,
    /\b(19\d{2}|20\d{2})(?:\s*(?:г|г\.|год|года|year))?\s*(?:и\s*)?(?:новее|newer)\b/u,
  ];

  for (const pattern of patterns) {
    const match = normalized.match(pattern);
    if (match) {
      return Number(match[1]);
    }
  }

  return null;
}

function parseMaxYear(prompt: string): number | null {
  const normalized = normalizePrompt(prompt);
  if (!hasExplicitYearIntent(normalized)) {
    return null;
  }

  const patterns = [
    /\b(?:до|по|не новее|не позже|не позднее)\b[^\d]{0,12}(19\d{2}|20\d{2})(?:\s*(?:г|г\.|год|года|year))?\b/u,
    /\b(19\d{2}|20\d{2})(?:\s*(?:г|г\.|год|года|year))?\s*(?:или\s*)?(?:старше|older|earlier)\b/u,
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
    STYLE_DICTIONARY
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
    STYLE_DICTIONARY
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
  const brands = new Set<string>();

  for (const brand of getBrandDictionary()) {
    if (normalized.includes(brand)) {
      brands.add(brand);
    }
  }

  for (const [model, brand] of Object.entries(getModelToBrandDictionary())) {
    if (normalized.includes(model)) {
      brands.add(brand);
    }
  }

  return [...brands];
}

export function parseQueryHeuristically(prompt: string): ParsedRecommendationQuery {
  const normalized = normalizePrompt(prompt);
  const excludedStyles = parseExcludedStyles(normalized);
  const preferredStyles = parsePreferredStyles(normalized).filter(
    (style) => !excludedStyles.includes(style),
  );
  const availabilityRange = parseAvailabilityRange(prompt);

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
    maxYear: parseMaxYear(normalized),
    startTime: availabilityRange?.startTime ?? null,
    endTime: availabilityRange?.endTime ?? null,
    requiresAvailableOnDates: availabilityRange?.requiresAvailableOnDates ?? false,
  };
}
