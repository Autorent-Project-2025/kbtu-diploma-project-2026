import { canonicalizeStyleLabel, canonicalizeTransmissionLabel } from "../ai/heuristicQueryParser";
import { AiChatMessage, ParsedRecommendationQuery, SearchCandidate } from "../types";

const MAX_MESSAGE_LENGTH = 4000;
const MAX_REASONS = 3;
const MAX_TAGS = 12;
const MAX_CHAT_MESSAGES = 40;
const MAX_STYLE_TAGS = 8;
const MAX_BRANDS = 8;

function toFiniteNumber(value: unknown): number | null {
  if (typeof value === "number" && Number.isFinite(value)) {
    return value;
  }

  if (typeof value === "string" && value.trim()) {
    const parsed = Number(value);
    return Number.isFinite(parsed) ? parsed : null;
  }

  return null;
}

function toStringValue(value: unknown, maxLength = 255): string | null {
  if (typeof value !== "string") {
    return null;
  }

  const normalized = value.trim();
  if (!normalized) {
    return null;
  }

  return normalized.slice(0, maxLength);
}

function toStringArray(value: unknown, maxItems: number, maxLength = 128): string[] {
  if (!Array.isArray(value)) {
    return [];
  }

  return value
    .map((item) => toStringValue(item, maxLength))
    .filter((item): item is string => Boolean(item))
    .slice(0, maxItems);
}

function unique(values: string[]): string[] {
  return [...new Set(values.filter(Boolean))];
}

function normalizeRating(value: unknown): number | null {
  const parsed = toFiniteNumber(value);
  if (parsed == null) {
    return null;
  }

  return parsed >= 0 && parsed <= 5 ? parsed : null;
}

export function normalizeParsedRecommendationQuery(
  value: unknown,
  fallbackPrompt = "",
): ParsedRecommendationQuery | null {
  if (!value || typeof value !== "object") {
    return null;
  }

  const query = value as Record<string, unknown>;
  const excludedStyles = unique(
    toStringArray(query.excludedStyles, MAX_STYLE_TAGS)
      .map((item) => canonicalizeStyleLabel(item))
      .filter((item): item is string => Boolean(item)),
  );

  const preferredStyles = unique(
    toStringArray(query.preferredStyles, MAX_STYLE_TAGS)
      .map((item) => canonicalizeStyleLabel(item))
      .filter((item): item is string => Boolean(item))
      .filter((item) => !excludedStyles.includes(item)),
  );

  return {
    prompt: toStringValue(query.prompt, MAX_MESSAGE_LENGTH) ?? fallbackPrompt,
    maxBudgetPerHour: toFiniteNumber(query.maxBudgetPerHour),
    passengers: toFiniteNumber(query.passengers),
    transmission:
      typeof query.transmission === "string"
        ? canonicalizeTransmissionLabel(query.transmission)
        : null,
    minRating: normalizeRating(query.minRating),
    preferredStyles,
    excludedStyles,
    preferredBrands: unique(
      toStringArray(query.preferredBrands, MAX_BRANDS)
        .map((item) => item.toLowerCase())
        .filter(Boolean),
    ),
    minYear: toFiniteNumber(query.minYear),
    maxYear: toFiniteNumber(query.maxYear),
    startTime: toStringValue(query.startTime, 128),
    endTime: toStringValue(query.endTime, 128),
    requiresAvailableOnDates: Boolean(query.requiresAvailableOnDates),
  };
}

export function normalizeSearchCandidate(value: unknown): SearchCandidate | null {
  if (!value || typeof value !== "object") {
    return null;
  }

  const candidate = value as Record<string, unknown>;
  const partnerCarId = toFiniteNumber(candidate.partnerCarId);
  const carModelId = toFiniteNumber(candidate.carModelId);
  const year = toFiniteNumber(candidate.year);
  const title = toStringValue(candidate.title, 255);
  const detailsUrl = toStringValue(candidate.detailsUrl, 255);
  const bookingUrl = toStringValue(candidate.bookingUrl, 255);
  const brand = toStringValue(candidate.brand, 128);
  const model = toStringValue(candidate.model, 128);

  if (
    partnerCarId == null ||
    carModelId == null ||
    year == null ||
    !title ||
    !detailsUrl ||
    !bookingUrl ||
    !brand ||
    !model
  ) {
    return null;
  }

  return {
    partnerCarId,
    carModelId,
    brand,
    model,
    year,
    title,
    imageUrl: toStringValue(candidate.imageUrl, 1024),
    detailsUrl,
    bookingUrl,
    priceHour: toFiniteNumber(candidate.priceHour),
    priceDay: toFiniteNumber(candidate.priceDay),
    rating: toFiniteNumber(candidate.rating),
    ratingsCount: toFiniteNumber(candidate.ratingsCount) ?? 0,
    carrierName: toStringValue(candidate.carrierName, 255),
    tags: toStringArray(candidate.tags, MAX_TAGS),
    lexicalScore: toFiniteNumber(candidate.lexicalScore) ?? 0,
    vectorScore: toFiniteNumber(candidate.vectorScore) ?? 0,
    businessScore: toFiniteNumber(candidate.businessScore) ?? 0,
    finalScore: toFiniteNumber(candidate.finalScore) ?? 0,
    reasons: toStringArray(candidate.reasons, MAX_REASONS),
  };
}

export function normalizeChatMessage(value: unknown, fallbackId: number): AiChatMessage | null {
  if (!value || typeof value !== "object") {
    return null;
  }

  const message = value as Record<string, unknown>;
  const role = message.role === "assistant" || message.role === "user" ? message.role : null;
  const content = toStringValue(message.content, MAX_MESSAGE_LENGTH);

  if (!role || !content) {
    return null;
  }

  const id = toFiniteNumber(message.id) ?? fallbackId;
  const cars = Array.isArray(message.cars)
    ? message.cars
        .map((candidate) => normalizeSearchCandidate(candidate))
        .filter((candidate): candidate is SearchCandidate => Boolean(candidate))
    : [];

  return {
    id,
    role,
    content,
    cars,
    appliedFilters: normalizeParsedRecommendationQuery(message.appliedFilters, content),
  };
}

export function normalizeChatMessages(messages: unknown): AiChatMessage[] {
  if (!Array.isArray(messages)) {
    return [];
  }

  return messages
    .slice(-MAX_CHAT_MESSAGES)
    .map((message, index) => normalizeChatMessage(message, index + 1))
    .filter((message): message is AiChatMessage => Boolean(message));
}
