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
${catalog ? catalog + "\n" : ""}
ABSOLUTE RULE: Only fill a field if the user's message EXPLICITLY contains that information. When in doubt, use null or []. Do not infer, guess, or add "reasonable defaults". An empty result is always better than a hallucinated one.

The "[Context from catalog]" block attached to the user message is reference data about available cars — IGNORE years, tags, or any other values from it. Never copy years, budgets, or transmission from catalog context into the output. Only the user's original text determines filters.

If the user mentions a car model name (e.g. "cobalt", "кобальт", "camry", "камри"), put the corresponding brand into "preferredBrands" — but DO NOT add transmission, year, style, or budget based on that model.

Greetings (привет, hello), off-topic text, or gibberish → ALL fields null/[].

"minRating" only when user says "рейтинг", "звёзд", "rating". Otherwise null.
"minYear" only for "от 2020", "2020+", "не старше 2020". "maxYear" only for "до 2020", "по 2020 год", "не новее 2020". Never put year into "maxBudgetPerHour".
"transmission" only when user literally says "автомат", "механика", "manual", "automatic", "акпп", "мкпп".

EXAMPLES (study carefully):

User: "есть cobalt"
Output: {"maxBudgetPerHour":null,"passengers":null,"transmission":null,"minRating":null,"preferredStyles":[],"excludedStyles":[],"preferredBrands":["chevrolet"],"minYear":null,"maxYear":null,"startTime":null,"endTime":null,"requiresAvailableOnDates":false}

User: "нужна camry"
Output: {"maxBudgetPerHour":null,"passengers":null,"transmission":null,"minRating":null,"preferredStyles":[],"excludedStyles":[],"preferredBrands":["toyota"],"minYear":null,"maxYear":null,"startTime":null,"endTime":null,"requiresAvailableOnDates":false}

User: "привет"
Output: {"maxBudgetPerHour":null,"passengers":null,"transmission":null,"minRating":null,"preferredStyles":[],"excludedStyles":[],"preferredBrands":[],"minYear":null,"maxYear":null,"startTime":null,"endTime":null,"requiresAvailableOnDates":false}

User: "спортивную до 10000 в час"
Output: {"maxBudgetPerHour":10000,"passengers":null,"transmission":null,"minRating":null,"preferredStyles":["sport"],"excludedStyles":[],"preferredBrands":[],"minYear":null,"maxYear":null,"startTime":null,"endTime":null,"requiresAvailableOnDates":false}

User: "camry автомат от 2020"
Output: {"maxBudgetPerHour":null,"passengers":null,"transmission":"automatic","minRating":null,"preferredStyles":[],"excludedStyles":[],"preferredBrands":["toyota"],"minYear":2020,"maxYear":null,"startTime":null,"endTime":null,"requiresAvailableOnDates":false}
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
      parts.push("Cars in catalog: " + unique.map(r => `${r.brand} ${r.model}`).join(", "));
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
