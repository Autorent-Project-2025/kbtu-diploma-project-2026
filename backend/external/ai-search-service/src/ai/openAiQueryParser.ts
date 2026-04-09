import { config } from "../config/env";
import { ParsedRecommendationQuery } from "../types";
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
  "minRating": number | null,
  "preferredStyles": string[],
  "excludedStyles": string[],
  "preferredBrands": string[],
  "minYear": number | null,
  "startTime": string | null,
  "endTime": string | null,
  "requiresAvailableOnDates": boolean
}
Allowed style labels: sport, business, family, city, luxury.
Allowed transmission labels: automatic, manual.
If a value is not explicitly or reasonably inferable, return null or [].
`;

export async function parseQueryWithOpenAi(prompt: string): Promise<ParsedRecommendationQuery> {
  if (!config.openAiApiKey) {
    throw new Error("OPENAI_API_KEY is not configured.");
  }

  const response = await fetch(`${config.openAiBaseUrl.replace(/\/$/, "")}/chat/completions`, {
    method: "POST",
    headers: {
      "Content-Type": "application/json",
      Authorization: `Bearer ${config.openAiApiKey}`,
    },
    body: JSON.stringify({
      model: config.openAiChatModel,
      temperature: 0,
      response_format: { type: "json_object" },
      messages: [
        { role: "system", content: systemPrompt.trim() },
        { role: "user", content: prompt },
      ],
    }),
  });

  if (!response.ok) {
    throw new Error(`OpenAI chat request failed with status ${response.status}.`);
  }

  const payload = (await response.json()) as {
    choices?: Array<{ message?: { content?: string | null } }>;
  };

  const content = payload.choices?.[0]?.message?.content?.trim();
  if (!content) {
    throw new Error("OpenAI parser response was empty.");
  }

  const parsed = JSON.parse(content) as Omit<ParsedRecommendationQuery, "prompt">;
  const excludedStyles = Array.isArray(parsed.excludedStyles)
    ? parsed.excludedStyles.map((item) => String(item).trim().toLowerCase()).filter(Boolean)
        .map((item) => canonicalizeStyleLabel(item))
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
    preferredStyles: Array.isArray(parsed.preferredStyles)
      ? parsed.preferredStyles.map((item) => String(item).trim().toLowerCase()).filter(Boolean)
          .map((item) => canonicalizeStyleLabel(item))
          .filter((item): item is string => Boolean(item))
          .filter((item) => !excludedStyles.includes(item))
      : [],
    excludedStyles,
    preferredBrands: Array.isArray(parsed.preferredBrands)
      ? parsed.preferredBrands.map((item) => String(item).trim().toLowerCase()).filter(Boolean)
      : [],
    minYear: typeof parsed.minYear === "number" ? parsed.minYear : null,
    startTime: typeof parsed.startTime === "string" && parsed.startTime.trim() ? parsed.startTime : null,
    endTime: typeof parsed.endTime === "string" && parsed.endTime.trim() ? parsed.endTime : null,
    requiresAvailableOnDates: Boolean(parsed.requiresAvailableOnDates),
  };
}
