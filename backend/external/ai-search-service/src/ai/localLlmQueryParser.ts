import { config } from "../config/env";
import { ParsedRecommendationQuery } from "../types";
import { postToOllama } from "../llm/ollamaClient";
import {
  canonicalizeStyleLabel,
  canonicalizeTransmissionLabel,
} from "./heuristicQueryParser";

const systemPrompt = `
You extract structured filters for car recommendation search.
Return only valid JSON.
Schema:
{
  "maxBudgetPerHour": number | null,
  "passengers": number | null,
  "transmission": string | null,
  "preferredStyles": string[],
  "preferredBrands": string[],
  "minYear": number | null,
  "startTime": string | null,
  "endTime": string | null,
  "requiresAvailableOnDates": boolean
}
Allowed style labels: sport, business, family, city, luxury.
Allowed transmission labels: automatic, manual.
If a value is not explicitly or reasonably inferable, return null or [].
Do not invent budget, passenger count, transmission, or dates when they are not explicitly present in the user request.
`;

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
  return {
    prompt,
    maxBudgetPerHour:
      typeof parsed.maxBudgetPerHour === "number" ? parsed.maxBudgetPerHour : null,
    passengers: typeof parsed.passengers === "number" ? parsed.passengers : null,
    transmission:
      typeof parsed.transmission === "string"
        ? canonicalizeTransmissionLabel(parsed.transmission)
        : null,
    preferredStyles: Array.isArray(parsed.preferredStyles)
      ? parsed.preferredStyles
          .map((item) => canonicalizeStyleLabel(String(item)))
          .filter((item): item is string => Boolean(item))
      : [],
    preferredBrands: Array.isArray(parsed.preferredBrands)
      ? parsed.preferredBrands
          .map((item) => String(item).trim().toLowerCase())
          .filter(Boolean)
      : [],
    minYear: typeof parsed.minYear === "number" ? parsed.minYear : null,
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

export async function parseQueryWithLocalLlm(
  prompt: string,
): Promise<ParsedRecommendationQuery> {
  if (!config.localLlmBaseUrl) {
    throw new Error("LOCAL_LLM_BASE_URL is not configured.");
  }

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
        content: systemPrompt.trim(),
      },
      {
        role: "user",
        content: prompt,
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
