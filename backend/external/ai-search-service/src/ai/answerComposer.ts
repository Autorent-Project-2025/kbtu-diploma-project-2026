import { config } from "../config/env";
import { postToOllama } from "../llm/ollamaClient";
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

type OllamaChatResponse = {
  message?: {
    content?: string | null;
  };
  response?: string | null;
};

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
    query.requiresAvailableOnDates
  );
}

export function shouldAskClarifyingQuestion(
  query: ParsedRecommendationQuery,
): boolean {
  if (hasSearchSignals(query)) {
    return false;
  }

  const normalizedPrompt = normalizePrompt(query.prompt);
  if (!normalizedPrompt) {
    return true;
  }

  if (greetingPhrases.includes(normalizedPrompt)) {
    return true;
  }

  if (normalizedPrompt.includes("как дела")) {
    return true;
  }

  const words = normalizedPrompt.split(" ").filter(Boolean);
  const hasCarIntentKeyword = carIntentKeywords.some((keyword) =>
    normalizedPrompt.includes(keyword),
  );

  return words.length <= 3 && !hasCarIntentKeyword;
}

function composeDeterministicRecommendationText(
  query: ParsedRecommendationQuery,
  cars: SearchCandidate[],
): string {
  if (cars.length === 0) {
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

async function generateWithLocalLlm(
  kind: "clarification" | "recommendation",
  query: ParsedRecommendationQuery,
  cars: SearchCandidate[],
): Promise<string | null> {
  if (!config.localLlmBaseUrl) {
    return null;
  }

  const systemPrompt = kind === "clarification"
    ? `
You are AutoRent AI assistant.
Reply in Russian.
Write exactly one short natural reply to the user's latest message.
The user has not yet provided enough constraints for a car search.
Politely guide them to provide criteria like budget, type of car, transmission, seats, dates, or rating threshold.
Ask only about supported criteria.
Do not ask about city, location, brand availability, or anything outside the provided criteria.
Keep it concise, practical, and conversational.
Maximum 22 words.
Do not use bullet points or markdown.
Do not mention internal filters or JSON.
Vary phrasing naturally and react to the user's exact wording.
`.trim()
    : `
You are AutoRent AI assistant.
Reply in Russian.
Write one or two concise natural sentences.
You already have a deterministic ranked list of cars.
Summarize the result without inventing facts.
You may mention up to two car names from the provided list.
Do not use bullet points or markdown.
Do not mention vector search, embeddings, internal filters, or technical details.
`.trim();

  const userPrompt = kind === "clarification"
    ? `
User message: ${query.prompt}

Known extracted filters:
${JSON.stringify({
  maxBudgetPerHour: query.maxBudgetPerHour,
  passengers: query.passengers,
        transmission: query.transmission,
        minRating: query.minRating,
        preferredStyles: query.preferredStyles,
        excludedStyles: query.excludedStyles,
        preferredBrands: query.preferredBrands,
        minYear: query.minYear,
        startTime: query.startTime,
        endTime: query.endTime,
}, null, 2)}

Write a short reply that asks for missing car-selection criteria.
`.trim()
    : `
User message: ${query.prompt}

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
        startTime: query.startTime,
        endTime: query.endTime,
}, null, 2)}

Top cars:
${formatCarsForPrompt(cars)}

Write a short result summary based only on this data.
`.trim();

  try {
    const payload = await postToOllama<OllamaChatResponse>("/api/chat", {
      model: config.localLlmChatModel,
      stream: false,
      options: {
        temperature: kind === "clarification" ? 0.7 : 0.45,
        num_predict: kind === "clarification" ? 60 : 120,
      },
      messages: [
        {
          role: "system",
          content: systemPrompt,
        },
        {
          role: "user",
          content: userPrompt,
        },
      ],
    });

    const content = normalizeAssistantText(
      payload.message?.content?.trim() || payload.response?.trim() || "",
    );

    if (!content) {
      throw new Error("Local LLM answer generation returned empty content.");
    }

    observabilityLogger.info("local_llm_answer_generation_succeeded", {
      kind,
      model: config.localLlmChatModel,
      carsCount: cars.length,
    });

    return content;
  } catch (error) {
    observabilityLogger.warn("local_llm_answer_generation_failed_fallback_to_template", {
      kind,
      errorMessage: error instanceof Error ? error.message : String(error),
      model: config.localLlmChatModel,
    });
    return null;
  }
}

export async function composeRecommendationResponse(
  query: ParsedRecommendationQuery,
  cars: SearchCandidate[],
): Promise<AiRecommendationResponse> {
  const assistantText = composeDeterministicRecommendationText(query, cars);

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
  const assistantText =
    (await generateWithLocalLlm("clarification", query, [])) ??
    composeDeterministicClarificationText();

  return {
    assistantText,
    appliedFilters: query,
    totalCandidates: 0,
    cars: [],
  };
}
