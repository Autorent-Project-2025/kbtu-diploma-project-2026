import { sql } from "../db/sql";
import { observabilityLogger } from "../observability/logger";
import { AiChatHistoryResponse, AiChatMessage, SearchCandidate } from "../types";

const MAX_CHAT_MESSAGES = 40;
const MAX_MESSAGE_LENGTH = 4000;
const MAX_REASONS = 3;
const MAX_TAGS = 12;

type RawChatHistoryRow = {
  userId: string;
  messages: unknown;
};

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

function normalizeSearchCandidate(value: unknown): SearchCandidate | null {
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

function normalizeChatMessage(value: unknown, fallbackId: number): AiChatMessage | null {
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
  };
}

function normalizeChatMessages(messages: unknown): AiChatMessage[] {
  if (!Array.isArray(messages)) {
    return [];
  }

  return messages
    .slice(-MAX_CHAT_MESSAGES)
    .map((message, index) => normalizeChatMessage(message, index + 1))
    .filter((message): message is AiChatMessage => Boolean(message));
}

export async function getChatHistory(userId: string): Promise<AiChatHistoryResponse> {
  const rows = await sql<RawChatHistoryRow[]>`
    select
      user_id as "userId",
      messages
    from ai_chat_histories
    where user_id = ${userId}
    limit 1
  `;

  const normalizedMessages = normalizeChatMessages(rows[0]?.messages ?? []);

  observabilityLogger.info("chat_history_loaded", {
    userId,
    messagesCount: normalizedMessages.length,
  });

  return {
    messages: normalizedMessages,
  };
}

export async function saveChatHistory(
  userId: string,
  messages: unknown,
): Promise<AiChatHistoryResponse> {
  const normalizedMessages = normalizeChatMessages(messages);

  await sql`
    insert into ai_chat_histories (
      user_id,
      messages,
      updated_at
    )
    values (
      ${userId},
      ${sql.json(normalizedMessages)},
      now()
    )
    on conflict (user_id) do update
    set
      messages = excluded.messages,
      updated_at = now()
  `;

  observabilityLogger.info("chat_history_saved", {
    userId,
    messagesCount: normalizedMessages.length,
  });

  return {
    messages: normalizedMessages,
  };
}
