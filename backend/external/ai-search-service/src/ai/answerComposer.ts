import { config } from "../config/env";
import { completeWithPreferredLlm, describeConfiguredLlm } from "../llm/chatCompletion";
import { observabilityLogger } from "../observability/logger";
import { AiRecommendationResponse, ParsedRecommendationQuery, SearchCandidate } from "../types";

const greetingPhrases = [
  "привет",
  "здравствуй",
  "здравствуйте",
  "добрый день",
  "добрый вечер",
  "доброе утро",
  "салам",
  "hello",
  "hi",
  "hey",
  "good morning",
  "good evening",
];

const carIntentKeywords = [
  "машин",
  "авто",
  "автомоб",
  "car",
  "cars",
  "sedan",
  "suv",
  "sport",
  "sportive",
  "кроссовер",
  "седан",
  "внедорож",
  "купе",
  "минивэн",
  "бюджет",
  "час",
  "сутк",
  "аренд",
  "прокат",
  "мест",
  "короб",
  "автомат",
  "механик",
  "партнер",
  "рейтинг",
];

function normalizePrompt(prompt: string): string {
  return prompt.trim().toLowerCase().replace(/\s+/g, " ");
}

function tokenizePrompt(normalizedPrompt: string): string[] {
  return normalizedPrompt
    .split(/[\s,.!?;:()"'`«»]+/g)
    .map((token) => token.replace(/^[^\p{L}\p{N}]+|[^\p{L}\p{N}]+$/gu, ""))
    .filter(Boolean);
}

const genericPromptTokens = new Set([
  "а",
  "и",
  "или",
  "есть",
  "ли",
  "что",
  "какие",
  "какая",
  "какой",
  "какое",
  "можно",
  "нужно",
  "мне",
  "могу",
  "хочу",
  "посоветуй",
  "посоветуешь",
  "подбери",
  "подберите",
  "покажи",
  "найди",
  "варианты",
  "вариант",
  "машина",
  "машину",
  "машины",
  "авто",
  "автомобиль",
  "автомобили",
  "car",
  "cars",
  "show",
  "find",
  "need",
  "want",
  "please",
]);

function hasSearchSignals(query: ParsedRecommendationQuery): boolean {
  return (
    query.maxBudgetPerHour != null ||
    query.passengers != null ||
    query.transmission != null ||
    query.minRating != null ||
    query.preferredStyles.length > 0 ||
    query.excludedStyles.length > 0 ||
    query.preferredBrands.length > 0 ||
    query.minYear != null ||
    query.maxYear != null ||
    query.requiresAvailableOnDates
  );
}

export function shouldAskClarifyingQuestion(
  query: ParsedRecommendationQuery,
): boolean {
  // If the query parser (heuristic + LLM) found any concrete search filter,
  // proceed to search — the intent is clear enough.
  if (hasSearchSignals(query)) {
    return false;
  }

  // No search signals detected — let LLM handle the conversation.
  // It will figure out whether the prompt is a greeting, gibberish, or
  // a vague request, and respond naturally.
  return true;
}

function composeDeterministicRecommendationText(
  query: ParsedRecommendationQuery,
  cars: SearchCandidate[],
): string {
  if (cars.length === 0) {
    const requestedStyle = query.preferredStyles[0] ?? null;
    const styleLabel = requestedStyle ? `в стиле ${requestedStyle}` : "по этому запросу";

    if (query.maxBudgetPerHour != null && requestedStyle) {
      return `Не нашлось машин ${styleLabel} до ${query.maxBudgetPerHour} ₸/час. Попробуйте увеличить бюджет или смягчить требования к стилю.`;
    }

    if (requestedStyle) {
      return `Не нашлось машин ${styleLabel}. Попробуйте расширить запрос или убрать жёсткое ограничение по стилю.`;
    }

    if (query.maxBudgetPerHour != null) {
      return `Не нашлось машин до ${query.maxBudgetPerHour} ₸/час. Попробуйте немного увеличить бюджет или убрать часть ограничений.`;
    }

    return "Подходящих машин по этому запросу сейчас не нашлось. Попробуйте смягчить бюджет, убрать жёсткие ограничения или уточнить сценарий поездки.";
  }

  const summaryParts: string[] = [];
  const onlyOverBudgetMatches =
    query.maxBudgetPerHour != null &&
    cars.length > 0 &&
    cars.every((car) => car.priceHour != null && car.priceHour > query.maxBudgetPerHour!);

  if (query.preferredStyles.length > 0) {
    summaryParts.push(`ориентировался на стиль ${query.preferredStyles.join(", ")}`);
  }

  if (query.maxBudgetPerHour != null) {
    summaryParts.push(
      onlyOverBudgetMatches
        ? `точных вариантов до ${query.maxBudgetPerHour} ₸/час не нашлось, поэтому показал ближайшие чуть дороже`
        : `учёл лимит до ${query.maxBudgetPerHour} ₸/час`,
    );
  }

  if (query.transmission) {
    summaryParts.push(`отфильтровал по коробке ${query.transmission}`);
  }

  if (query.minRating != null) {
    summaryParts.push(`учёл рейтинг от ${query.minRating.toFixed(1)}`);
  }

  if (query.passengers != null) {
    summaryParts.push(`проверил вместимость от ${query.passengers} мест`);
  }

  if (query.minYear != null && query.maxYear != null) {
    summaryParts.push(`учёл год выпуска от ${query.minYear} до ${query.maxYear}`);
  } else if (query.minYear != null) {
    summaryParts.push(`учёл год выпуска от ${query.minYear}`);
  } else if (query.maxYear != null) {
    summaryParts.push(`учёл год выпуска до ${query.maxYear}`);
  }

  const summary = summaryParts.length > 0 ? `${summaryParts.join(", ")}. ` : "";
  return `Нашёл ${cars.length} наиболее подходящих машин. ${summary}Ниже варианты с лучшим совпадением по смыслу запроса, бюджету и рейтингу.`;
}

function composeDeterministicClarificationText(): string {
  return 'Могу подобрать машину, если скажете хотя бы часть критериев: бюджет, тип машины, коробку или даты. Например: "хочу спортивную машину до 10000 ₸/час".';
}

function formatCarsForPrompt(cars: SearchCandidate[]): string {
  return cars
    .slice(0, 3)
    .map((car, index) => {
      const facts = [
        `#${index + 1}`,
        car.title,
        car.priceHour != null ? `${car.priceHour} ₸/час` : "цена не указана",
        car.rating != null ? `рейтинг ${car.rating}` : "без рейтинга",
        car.carrierName ? `партнёр ${car.carrierName}` : null,
        car.reasons.length > 0 ? `причины: ${car.reasons.join(", ")}` : null,
      ].filter(Boolean);

      return facts.join(" | ");
    })
    .join("\n");
}

function normalizeAssistantText(content: string): string {
  return content.replace(/\s+/g, " ").trim();
}

function countMatches(text: string, pattern: RegExp): number {
  return text.match(pattern)?.length ?? 0;
}

function looksRussianEnough(content: string): boolean {
  const cyrillicCount = countMatches(content, /[А-Яа-яЁё]/g);
  const latinCount = countMatches(content, /[A-Za-z]/g);

  if (cyrillicCount < 6) {
    return false;
  }

  return cyrillicCount >= latinCount;
}

function hasInvalidRecommendationArtifacts(content: string): boolean {
  return (
    /\$/.test(content) ||
    /\bper hour\b/i.test(content) ||
    /\bbased on\b/i.test(content) ||
    /\bprovided filters\b/i.test(content) ||
    /\bmeet the criteria\b/i.test(content) ||
    /^\s*[-*•]/m.test(content)
  );
}

function countSentences(content: string): number {
  const matches = content.match(/[.!?]+/g);
  return matches?.length ?? 1;
}

function assertRecommendationSummaryIsAcceptable(content: string) {
  if (!looksRussianEnough(content)) {
    throw new Error("LLM recommendation summary is not Russian enough.");
  }

  if (hasInvalidRecommendationArtifacts(content)) {
    throw new Error("LLM recommendation summary contains invalid artifacts.");
  }

  if (countSentences(content) > 3) {
    throw new Error("LLM recommendation summary is too long.");
  }
}

type RecommendationSummaryPayload = {
  assistantText?: string;
  referencedPartnerCarIds?: number[];
};

function normalizeReferencedPartnerCarIds(
  referencedPartnerCarIds: unknown,
  cars: SearchCandidate[],
): number[] {
  if (!Array.isArray(referencedPartnerCarIds)) {
    return [];
  }

  const knownIds = new Set(cars.map((car) => car.partnerCarId));
  const result: number[] = [];
  const seenIds = new Set<number>();

  for (const rawId of referencedPartnerCarIds) {
    const partnerCarId = Number(rawId);
    if (!Number.isInteger(partnerCarId) || seenIds.has(partnerCarId) || !knownIds.has(partnerCarId)) {
      continue;
    }

    seenIds.add(partnerCarId);
    result.push(partnerCarId);
  }

  return result;
}

async function generateRecommendationSummaryWithLlm(
  query: ParsedRecommendationQuery,
  cars: SearchCandidate[],
): Promise<string | null> {
  if (!config.llmRecommendationSummaryEnabled || cars.length === 0) {
    return null;
  }

  const llm = describeConfiguredLlm();
  if (!llm) {
    return null;
  }

  const systemPrompt = `
You are AutoRent AI assistant helping users find rental cars.
Return only valid JSON.
Reply in Russian and use Cyrillic in assistantText.
Summarize the recommendation in up to three concise natural sentences.
Use the provided car data (tags, description, features) to give a helpful, grounded answer.
You may mention at most three car titles from the provided list.
Do not invent facts, brands, availability, missing data, prices, or counts.
Do not use bullet points, markdown, or English explanatory phrases.
If a price is mentioned, keep the provided format unchanged.
Schema:
{
  "assistantText": string,
  "referencedPartnerCarIds": number[]
}
`.trim();

  const userPrompt = `
User message:
${query.prompt}

Extracted filters:
${JSON.stringify({
  maxBudgetPerHour: query.maxBudgetPerHour,
  passengers: query.passengers,
  transmission: query.transmission,
  minRating: query.minRating,
  preferredStyles: query.preferredStyles,
  excludedStyles: query.excludedStyles,
  preferredBrands: query.preferredBrands,
  minYear: query.minYear,
  maxYear: query.maxYear,
  startTime: query.startTime,
  endTime: query.endTime,
  requiresAvailableOnDates: query.requiresAvailableOnDates,
}, null, 2)}

Top cars:
${JSON.stringify(
  cars.slice(0, 6).map((car, index) => ({
    rank: index + 1,
    partnerCarId: car.partnerCarId,
    title: car.title,
    priceHourLabel: car.priceHour != null ? `${car.priceHour} ₸/час` : "цена не указана",
    ratingLabel: car.rating != null ? `рейтинг ${car.rating}` : "без рейтинга",
    carrierName: car.carrierName,
    tags: car.tags ?? [],
    reasons: car.reasons,
  })),
  null,
  2,
)}
`.trim();

  try {
    const completion = await completeWithPreferredLlm({
      systemPrompt,
      userPrompt,
      responseType: "json",
      temperature: 0.2,
      maxOutputTokens: 300,
      timeoutMs: config.llmRecommendationSummaryTimeoutMs,
    });

    if (!completion) {
      return null;
    }

    const payload = JSON.parse(completion.content) as RecommendationSummaryPayload;
    const assistantText = normalizeAssistantText(String(payload.assistantText ?? ""));
    if (!assistantText) {
      throw new Error("LLM recommendation summary payload is empty.");
    }

    assertRecommendationSummaryIsAcceptable(assistantText);

    const referencedPartnerCarIds = normalizeReferencedPartnerCarIds(
      payload.referencedPartnerCarIds,
      cars.slice(0, 6),
    );

    observabilityLogger.info("llm_recommendation_summary_succeeded", {
      provider: completion.provider,
      model: completion.model,
      carsCount: cars.length,
      referencedPartnerCarIds,
    });

    return assistantText;
  } catch (error) {
    observabilityLogger.warn("llm_recommendation_summary_failed_fallback_to_deterministic", {
      provider: llm.provider,
      model: llm.model,
      errorMessage: error instanceof Error ? error.message : String(error),
      carsCount: cars.length,
    });
    return null;
  }
}

export async function composeRecommendationResponse(
  query: ParsedRecommendationQuery,
  cars: SearchCandidate[],
): Promise<AiRecommendationResponse> {
  const assistantText =
    (await generateRecommendationSummaryWithLlm(query, cars)) ??
    composeDeterministicRecommendationText(query, cars);

  return {
    assistantText,
    appliedFilters: query,
    totalCandidates: cars.length,
    cars,
  };
}

export async function composeClarificationResponse(
  query: ParsedRecommendationQuery,
): Promise<AiRecommendationResponse> {
  let assistantText = await generateClarificationWithLlm(query.prompt);
  if (!assistantText) {
    assistantText = composeDeterministicClarificationText();
  }

  return {
    assistantText,
    appliedFilters: query,
    totalCandidates: 0,
    cars: [],
  };
}

async function generateClarificationWithLlm(userPrompt: string): Promise<string | null> {
  const llm = describeConfiguredLlm();
  if (!llm) return null;

  try {
    const completion = await completeWithPreferredLlm({
      systemPrompt: `Ты — AI-ассистент сервиса аренды автомобилей AutoRent.
Отвечай на русском, дружелюбно и кратко (1-2 предложения).
Твоя задача — помочь пользователю подобрать машину.
Если пользователь просто здоровается — поприветствуй и предложи помочь с подбором.
Если сообщение непонятное или бессмысленное — вежливо попроси уточнить запрос.
Всегда направляй разговор к подбору машины: спроси про бюджет, тип машины, даты или предпочтения.
Не выдумывай наличие машин. Не используй markdown.`,
      userPrompt,
      responseType: "text",
      temperature: 0.5,
      maxOutputTokens: 150,
      timeoutMs: 6000,
    });

    if (!completion?.content) return null;

    const text = completion.content.trim();
    if (text.length < 5) return null;

    return text;
  } catch {
    return null;
  }
}
