import { config } from "../config/env";
import { observabilityLogger } from "../observability/logger";
import { AiChatMessage, ParsedRecommendationQuery } from "../types";
import { hasExplicitYearIntent, parseQueryHeuristically } from "./heuristicQueryParser";
import { parseQueryWithLocalLlm } from "./localLlmQueryParser";
import { parseQueryWithOpenAi } from "./openAiQueryParser";

function unique(values: string[]): string[] {
  return [...new Set(values.filter(Boolean))];
}

function normalizePrompt(prompt: string): string {
  return prompt.trim().toLowerCase().replace(/\s+/g, " ");
}

function countWords(normalizedPrompt: string): number {
  return normalizedPrompt.split(" ").filter(Boolean).length;
}

function isShortPrompt(normalizedPrompt: string): boolean {
  return countWords(normalizedPrompt) <= 4;
}

function hasContinuationMarker(normalizedPrompt: string): boolean {
  return /^(не|без|а|но|можно|а можно|теперь|тогда|ещ[её]|подешевле|дешевле|дороже|подороже)(?:\s|$)/u.test(
    normalizedPrompt,
  );
}

function isFilterOnlyFollowUpPrompt(
  normalizedPrompt: string,
  currentQuery: ParsedRecommendationQuery,
): boolean {
  if (
    currentQuery.transmission &&
    /^(?:с\s+)?(?:автомат|акпп|automatic|механик|мкпп|manual)(?:\s|$)/u.test(normalizedPrompt)
  ) {
    return true;
  }

  if (
    currentQuery.maxBudgetPerHour != null &&
    /^(?:до|от)\s*\d/u.test(normalizedPrompt) &&
    !hasExplicitYearIntent(normalizedPrompt)
  ) {
    return true;
  }

  if (
    currentQuery.passengers != null &&
    /^\d+\s*(?:чел|человек|people|passenger|пассаж|мест|места)(?:\s|$)/u.test(normalizedPrompt)
  ) {
    return true;
  }

  if (
    currentQuery.minRating != null &&
    /^(?:рейтинг|rating)(?:\s|$)/u.test(normalizedPrompt)
  ) {
    return true;
  }

  if (
    currentQuery.minYear != null &&
    /^(?:(?:от|с|после|не старше|не ниже|начиная с)\s*)?(?:19\d{2}|20\d{2})(?:\s*(?:г|г\.|год|года|year|\+))?$/u.test(
      normalizedPrompt,
    )
  ) {
    return true;
  }

  if (
    currentQuery.maxYear != null &&
    /^(?:(?:до|по|не новее|не позже|не позднее)\s*)?(?:19\d{2}|20\d{2})(?:\s*(?:г|г\.|год|года|year))?$/u.test(
      normalizedPrompt,
    )
  ) {
    return true;
  }

  return false;
}

function isShortOrFollowUpPrompt(
  normalizedPrompt: string,
  currentQuery: ParsedRecommendationQuery,
): boolean {
  return (
    isShortPrompt(normalizedPrompt) ||
    hasContinuationMarker(normalizedPrompt) ||
    isFilterOnlyFollowUpPrompt(normalizedPrompt, currentQuery)
  );
}

function getLatestAppliedFilters(history: AiChatMessage[]): ParsedRecommendationQuery | null {
  for (let index = history.length - 1; index >= 0; index -= 1) {
    const message = history[index];
    if (message.role === "assistant" && message.appliedFilters) {
      return message.appliedFilters;
    }
  }

  return null;
}

function shouldInheritContext(
  prompt: string,
  currentQuery: ParsedRecommendationQuery,
  previousQuery: ParsedRecommendationQuery | null,
): boolean {
  if (!previousQuery) {
    return false;
  }

  const normalizedPrompt = normalizePrompt(prompt);
  if (!normalizedPrompt) {
    return false;
  }

  if (
    /^(привет|здравствуй|здравствуйте|hello|hi|hey)(?:\s|$)/u.test(normalizedPrompt) ||
    /^(новый запрос|сначала|забудь|ignore previous|reset)(?:\s|$)/u.test(normalizedPrompt)
  ) {
    return false;
  }

  if (currentQuery.excludedStyles.length > 0) {
    return true;
  }

  if (
    hasContinuationMarker(normalizedPrompt) ||
    isFilterOnlyFollowUpPrompt(normalizedPrompt, currentQuery)
  ) {
    return true;
  }

  return false;
}

function mergeWithConversationContext(
  currentQuery: ParsedRecommendationQuery,
  previousQuery: ParsedRecommendationQuery | null,
): ParsedRecommendationQuery {
  if (!previousQuery) {
    return currentQuery;
  }

  const preferredStyles = (
    currentQuery.preferredStyles.length > 0
      ? currentQuery.preferredStyles
      : previousQuery.preferredStyles
  ).filter((style) => !currentQuery.excludedStyles.includes(style));

  const excludedStyles = unique([
    ...previousQuery.excludedStyles,
    ...currentQuery.excludedStyles,
  ]).filter((style) => !preferredStyles.includes(style));
  const hasCurrentYearIntent = hasExplicitYearIntent(currentQuery.prompt);

  return {
    prompt: currentQuery.prompt,
    maxBudgetPerHour: currentQuery.maxBudgetPerHour ?? previousQuery.maxBudgetPerHour,
    passengers: currentQuery.passengers ?? previousQuery.passengers,
    transmission: currentQuery.transmission ?? previousQuery.transmission,
    minRating: currentQuery.minRating ?? previousQuery.minRating,
    preferredStyles,
    excludedStyles,
    preferredBrands:
      currentQuery.preferredBrands.length > 0
        ? currentQuery.preferredBrands
        : previousQuery.preferredBrands,
    minYear: hasCurrentYearIntent
      ? currentQuery.minYear
      : currentQuery.minYear ?? previousQuery.minYear,
    maxYear: hasCurrentYearIntent
      ? currentQuery.maxYear
      : currentQuery.maxYear ?? previousQuery.maxYear,
    startTime: currentQuery.startTime ?? previousQuery.startTime,
    endTime: currentQuery.endTime ?? previousQuery.endTime,
    requiresAvailableOnDates:
      currentQuery.requiresAvailableOnDates || previousQuery.requiresAvailableOnDates,
  };
}

// Defensive anti-hallucination guard. The small local LLM sometimes fills
// in transmission/rating when the user never mentioned them (e.g. "нужна
// камри" → transmission=manual). We only accept the LLM's value if the
// raw prompt actually contains a related keyword.
const TRANSMISSION_KEYWORDS = [
  "автомат", "акпп", "автомате", "automatic", "auto",
  "механик", "мкпп", "механике", "manual", "stick",
  "коробк", "gearbox",
];
const RATING_KEYWORDS = [
  "рейтинг", "ratings", "rating", "звёзд", "звезд", "stars", "оцен",
];

function promptMentionsAny(prompt: string, keywords: string[]): boolean {
  const normalized = prompt.toLowerCase();
  return keywords.some((kw) => normalized.includes(kw));
}

function reconcileWithHeuristics(
  modelQuery: ParsedRecommendationQuery,
  heuristicQuery: ParsedRecommendationQuery,
): ParsedRecommendationQuery {
  // LLM is the primary source — it understands context (e.g. "2020" is a year, not a price).
  // Heuristic only supplements for datetime parsing (ISO regex is more reliable than LLM).
  const prompt = modelQuery.prompt;
  const transmission =
    heuristicQuery.transmission ??
    (promptMentionsAny(prompt, TRANSMISSION_KEYWORDS) ? modelQuery.transmission : null);
  const minRating =
    promptMentionsAny(prompt, RATING_KEYWORDS) ? modelQuery.minRating : null;
  return {
    prompt: modelQuery.prompt,
    maxBudgetPerHour: modelQuery.maxBudgetPerHour,
    passengers: modelQuery.passengers,
    transmission,
    minRating,
    preferredStyles: unique([
      ...modelQuery.preferredStyles,
    ]).filter((style) => !modelQuery.excludedStyles.includes(style)),
    excludedStyles: unique([
      ...modelQuery.excludedStyles,
    ]),
    preferredBrands: unique([
      ...modelQuery.preferredBrands,
      // Supplement with heuristic brand detection (model→brand dictionary)
      ...heuristicQuery.preferredBrands.filter(
        (brand) => !modelQuery.preferredBrands.includes(brand),
      ),
    ]),
    minYear: modelQuery.minYear,
    maxYear: modelQuery.maxYear,
    // Dates: heuristic ISO parsing is more reliable than LLM
    startTime: heuristicQuery.startTime ?? modelQuery.startTime,
    endTime: heuristicQuery.endTime ?? modelQuery.endTime,
    requiresAvailableOnDates:
      heuristicQuery.requiresAvailableOnDates || modelQuery.requiresAvailableOnDates,
  };
}

export async function parseRecommendationQuery(
  prompt: string,
  history: AiChatMessage[] = [],
): Promise<ParsedRecommendationQuery> {
  const heuristicQuery = parseQueryHeuristically(prompt);
  const previousQuery = getLatestAppliedFilters(history);
  let resolvedQuery: ParsedRecommendationQuery;

  if (config.localLlmBaseUrl) {
    try {
      const parsed = await parseQueryWithLocalLlm(prompt);
      const reconciled = reconcileWithHeuristics(parsed, heuristicQuery);
      observabilityLogger.info("local_llm_query_parser_succeeded", {
        model: config.localLlmChatModel,
        maxBudgetPerHour: reconciled.maxBudgetPerHour,
        transmission: reconciled.transmission,
        minRating: reconciled.minRating,
        preferredStyles: reconciled.preferredStyles,
        excludedStyles: reconciled.excludedStyles,
        minYear: reconciled.minYear,
        maxYear: reconciled.maxYear,
      });
      resolvedQuery = reconciled;
    } catch (error) {
      observabilityLogger.warn("local_llm_query_parser_failed_fallback_to_heuristic", {
        errorMessage: error instanceof Error ? error.message : String(error),
        model: config.localLlmChatModel,
      });
      resolvedQuery = heuristicQuery;
    }
  } else if (config.openAiApiKey) {
    try {
      resolvedQuery = reconcileWithHeuristics(
        await parseQueryWithOpenAi(prompt),
        heuristicQuery,
      );
    } catch (error) {
      observabilityLogger.warn("openai_query_parser_failed_fallback_to_heuristic", {
        errorMessage: error instanceof Error ? error.message : String(error),
      });
      resolvedQuery = heuristicQuery;
    }
  } else {
    resolvedQuery = heuristicQuery;
  }

  if (!shouldInheritContext(prompt, resolvedQuery, previousQuery)) {
    return resolvedQuery;
  }

  const mergedQuery = mergeWithConversationContext(resolvedQuery, previousQuery);
  observabilityLogger.info("recommendation_query_context_merged", {
    prompt,
    inheritedFromHistory: true,
    previousStyles: previousQuery?.preferredStyles ?? [],
    currentStyles: resolvedQuery.preferredStyles,
    currentExcludedStyles: resolvedQuery.excludedStyles,
    mergedStyles: mergedQuery.preferredStyles,
    mergedExcludedStyles: mergedQuery.excludedStyles,
    maxBudgetPerHour: mergedQuery.maxBudgetPerHour,
    minRating: mergedQuery.minRating,
    minYear: mergedQuery.minYear,
    maxYear: mergedQuery.maxYear,
  });

  return mergedQuery;
}
