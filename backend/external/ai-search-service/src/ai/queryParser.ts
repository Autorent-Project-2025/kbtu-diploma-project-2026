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

function isShortOrFollowUpPrompt(normalizedPrompt: string): boolean {
  const wordsCount = normalizedPrompt.split(" ").filter(Boolean).length;
  return (
    wordsCount <= 4 ||
    /^(не|без|а|но|можно|а можно|теперь|тогда|ещ[её]|подешевле|дешевле|дороже|подороже)(?:\s|$)/u.test(
      normalizedPrompt,
    ) ||
    /^(с автомат|автомат|с механик|механик|до \d|от \d)/u.test(normalizedPrompt)
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
    /^(привет|здравствуй|здравствуйте|hello|hi|hey)\b/u.test(normalizedPrompt) ||
    /^(новый запрос|сначала|забудь|ignore previous|reset)\b/u.test(normalizedPrompt)
  ) {
    return false;
  }

  const wordsCount = normalizedPrompt.split(" ").filter(Boolean).length;
  const standaloneSignalCount = [
    currentQuery.maxBudgetPerHour != null,
    currentQuery.passengers != null,
    currentQuery.transmission != null,
    currentQuery.minRating != null,
    currentQuery.preferredStyles.length > 0,
    currentQuery.preferredBrands.length > 0,
    currentQuery.minYear != null,
    currentQuery.requiresAvailableOnDates,
  ].filter(Boolean).length;

  const hasFollowUpMarker =
    isShortOrFollowUpPrompt(normalizedPrompt);

  if (currentQuery.excludedStyles.length > 0) {
    return true;
  }

  if (hasFollowUpMarker) {
    return true;
  }

  return wordsCount <= 4 && standaloneSignalCount <= 1;
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
    minYear: currentQuery.minYear ?? previousQuery.minYear,
    startTime: currentQuery.startTime ?? previousQuery.startTime,
    endTime: currentQuery.endTime ?? previousQuery.endTime,
    requiresAvailableOnDates:
      currentQuery.requiresAvailableOnDates || previousQuery.requiresAvailableOnDates,
  };
}

function reconcileWithHeuristics(
  modelQuery: ParsedRecommendationQuery,
  heuristicQuery: ParsedRecommendationQuery,
): ParsedRecommendationQuery {
  const hasYearIntent = hasExplicitYearIntent(modelQuery.prompt);
  const normalizedPrompt = normalizePrompt(modelQuery.prompt);
  const canExpandWithModel = !isShortOrFollowUpPrompt(normalizedPrompt);
  const modelPreferredStyles =
    canExpandWithModel &&
    heuristicQuery.preferredStyles.length === 0 &&
    heuristicQuery.excludedStyles.length === 0
      ? modelQuery.preferredStyles
      : [];
  const modelExcludedStyles =
    canExpandWithModel && heuristicQuery.excludedStyles.length === 0
      ? modelQuery.excludedStyles
      : [];
  const modelPreferredBrands =
    canExpandWithModel && heuristicQuery.preferredBrands.length === 0
      ? modelQuery.preferredBrands
      : [];
  const excludedStyles = unique([
    ...heuristicQuery.excludedStyles,
    ...modelExcludedStyles,
  ]);

  return {
    prompt: modelQuery.prompt,
    maxBudgetPerHour:
      heuristicQuery.maxBudgetPerHour ?? modelQuery.maxBudgetPerHour,
    passengers: heuristicQuery.passengers ?? modelQuery.passengers,
    transmission: heuristicQuery.transmission,
    minRating: heuristicQuery.minRating,
    preferredStyles: unique([
      ...heuristicQuery.preferredStyles,
      ...modelPreferredStyles,
    ]).filter((style) => !excludedStyles.includes(style)),
    excludedStyles,
    preferredBrands: unique([
      ...heuristicQuery.preferredBrands,
      ...modelPreferredBrands,
    ]),
    minYear: hasYearIntent ? heuristicQuery.minYear ?? modelQuery.minYear : null,
    startTime: modelQuery.startTime,
    endTime: modelQuery.endTime,
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
  });

  return mergedQuery;
}
