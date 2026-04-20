import { config } from "../config/env";
import { ParsedRecommendationQuery } from "../types";
import { postToOllama } from "../llm/ollamaClient";
import {
  canonicalizeStyleLabel,
  canonicalizeTransmissionLabel,
} from "./heuristicQueryParser";
import { STYLE_LABELS_TEXT, TRANSMISSION_LABELS_TEXT, getCatalogSummary } from "../queryTaxonomy";
import { sql } from "../db/sql";

function buildSystemPrompt(): string {
  const catalog = getCatalogSummary();
  return `
You extract structured filters for car recommendation search.
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
Allowed style labels: ${STYLE_LABELS_TEXT}.
Allowed transmission labels: ${TRANSMISSION_LABELS_TEXT}.
${catalog ? catalog + "\n" : ""}If the user mentions a car model name (e.g. "cobalt", "кобальт", "camry", "камри"), put the corresponding brand into "preferredBrands".
CRITICAL: If a value is not EXPLICITLY stated in the user message, return null or []. Never guess or infer default values.
If the user message is a greeting (привет, hello, etc.), off-topic, or gibberish, return ALL fields as null or [].
Do not invent budget, passenger count, transmission, rating, or dates when they are not explicitly present in the user request.
If the user asks for rating threshold like "рейтинг больше 4.5", put it into "minRating". Otherwise minRating must be null.
Use "minYear" for requests like "от 2020 года", "2020+" or "не старше 2020".
Use "maxYear" for requests like "до 2020 года", "по 2020 год" or "не новее 2020".
Do not put year values into "maxBudgetPerHour".
`.trim();
}

type OllamaChatResponse = {
  message?: {
    content?: string | null;
  };
  response?: string | null;
};

function normalizeQuery(
  prompt: string,
  parsed: Partial<Omit<ParsedRecommendationQuery, "prompt">>,
): ParsedRecommendationQuery {
  const excludedStyles = Array.isArray(parsed.excludedStyles)
    ? parsed.excludedStyles
        .map((item) => canonicalizeStyleLabel(String(item)))
        .filter((item): item is string => Boolean(item))
    : [];

  return {
    prompt,
    maxBudgetPerHour:
      typeof parsed.maxBudgetPerHour === "number" ? parsed.maxBudgetPerHour : null,
    passengers: typeof parsed.passengers === "number" ? parsed.passengers : null,
    transmission:
      typeof parsed.transmission === "string"
        ? canonicalizeTransmissionLabel(parsed.transmission)
        : null,
    minRating:
      typeof parsed.minRating === "number" && parsed.minRating >= 0 && parsed.minRating <= 5
        ? parsed.minRating
        : null,
    excludedStyles,
    preferredStyles: Array.isArray(parsed.preferredStyles)
      ? parsed.preferredStyles
          .map((item) => canonicalizeStyleLabel(String(item)))
          .filter((item): item is string => Boolean(item))
          .filter((item) => !excludedStyles.includes(item))
      : [],
    preferredBrands: Array.isArray(parsed.preferredBrands)
      ? parsed.preferredBrands
          .map((item) => String(item).trim().toLowerCase())
          .filter(Boolean)
      : [],
    minYear: typeof parsed.minYear === "number" ? parsed.minYear : null,
    maxYear: typeof parsed.maxYear === "number" ? parsed.maxYear : null,
    startTime:
      typeof parsed.startTime === "string" && parsed.startTime.trim()
        ? parsed.startTime.trim()
        : null,
    endTime:
      typeof parsed.endTime === "string" && parsed.endTime.trim()
        ? parsed.endTime.trim()
        : null,
    requiresAvailableOnDates: Boolean(parsed.requiresAvailableOnDates),
  };
}

function unwrapJson(content: string): string {
  const trimmed = content.trim();
  if (!trimmed.startsWith("```")) {
    return trimmed;
  }

  return trimmed
    .replace(/^```(?:json)?/i, "")
    .replace(/```$/i, "")
    .trim();
}

async function retrieveRagContext(prompt: string): Promise<string> {
  try {
    const words = prompt.toLowerCase().replace(/[^\p{L}\p{N}\s]/gu, "").split(/\s+/).filter(w => w.length >= 2);
    if (words.length === 0) return "";

    const parts: string[] = [];

    // Search each word against brand_model_aliases
    for (const word of words) {
      const aliasRows = await sql<{ alias: string; canonical_brand: string; canonical_model: string | null }[]>`
        select alias, canonical_brand, canonical_model from brand_model_aliases
        where lower(alias) = ${word}
        limit 3
      `;
      for (const r of aliasRows) {
        parts.push(`"${r.alias}" means ${r.canonical_brand}${r.canonical_model ? " " + r.canonical_model : ""}`);
      }
    }

    // Search combined term against car documents
    const searchTerm = `%${words.join("%")}%`;
    const rows = await sql<{ brand: string; model: string; year: number }[]>`
      select distinct brand, model, year
      from ai_car_documents
      where lower(brand || ' ' || model || ' ' || coalesce(tags::text, '')) like ${searchTerm}
      limit 10
    `;

    if (rows.length === 0 && words.length > 1) {
      // Try each word individually against car documents
      for (const word of words) {
        const wordRows = await sql<{ brand: string; model: string; year: number }[]>`
          select distinct brand, model, year
          from ai_car_documents
          where lower(brand || ' ' || model) like ${"%" + word + "%"}
          limit 5
        `;
        rows.push(...wordRows);
      }
    }

    if (rows.length > 0) {
      const unique = [...new Map(rows.map(r => [`${r.brand} ${r.model}`, r])).values()];
      parts.push("Cars in catalog: " + unique.map(r => `${r.brand} ${r.model} (${r.year})`).join(", "));
    }

    return parts.join("\n");
  } catch {
    return "";
  }
}

export async function parseQueryWithLocalLlm(
  prompt: string,
): Promise<ParsedRecommendationQuery> {
  if (!config.localLlmBaseUrl) {
    throw new Error("LOCAL_LLM_BASE_URL is not configured.");
  }

  const ragContext = await retrieveRagContext(prompt);

  const userMessage = ragContext
    ? `${prompt}\n\n[Context from catalog]\n${ragContext}`
    : prompt;

  const payload = await postToOllama<OllamaChatResponse>("/api/chat", {
    model: config.localLlmChatModel,
    stream: false,
    format: "json",
    options: {
      temperature: 0,
    },
    messages: [
      {
        role: "system",
        content: buildSystemPrompt(),
      },
      {
        role: "user",
        content: userMessage,
      },
    ],
  });

  const content =
    payload.message?.content?.trim() || payload.response?.trim() || "";
  if (!content) {
    throw new Error("Local LLM parser response was empty.");
  }

  const parsed = JSON.parse(
    unwrapJson(content),
  ) as Partial<Omit<ParsedRecommendationQuery, "prompt">>;

  return normalizeQuery(prompt, parsed);
}
