import { config } from "../config/env";
import { observabilityLogger } from "../observability/logger";
import { ParsedRecommendationQuery } from "../types";
import { hasExplicitYearIntent, parseQueryHeuristically } from "./heuristicQueryParser";
import { parseQueryWithLocalLlm } from "./localLlmQueryParser";
import { parseQueryWithOpenAi } from "./openAiQueryParser";

function unique(values: string[]): string[] {
  return [...new Set(values.filter(Boolean))];
}

function reconcileWithHeuristics(
  modelQuery: ParsedRecommendationQuery,
  heuristicQuery: ParsedRecommendationQuery,
): ParsedRecommendationQuery {
  const hasYearIntent = hasExplicitYearIntent(modelQuery.prompt);

  return {
    prompt: modelQuery.prompt,
    maxBudgetPerHour:
      heuristicQuery.maxBudgetPerHour ?? modelQuery.maxBudgetPerHour,
    passengers: heuristicQuery.passengers ?? modelQuery.passengers,
    transmission: heuristicQuery.transmission,
    preferredStyles: unique([
      ...heuristicQuery.preferredStyles,
      ...modelQuery.preferredStyles,
    ]),
    preferredBrands: unique([
      ...heuristicQuery.preferredBrands,
      ...modelQuery.preferredBrands,
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
): Promise<ParsedRecommendationQuery> {
  const heuristicQuery = parseQueryHeuristically(prompt);

  if (config.localLlmBaseUrl) {
    try {
      const parsed = await parseQueryWithLocalLlm(prompt);
      const reconciled = reconcileWithHeuristics(parsed, heuristicQuery);
      observabilityLogger.info("local_llm_query_parser_succeeded", {
        model: config.localLlmChatModel,
        maxBudgetPerHour: reconciled.maxBudgetPerHour,
        transmission: reconciled.transmission,
        preferredStyles: reconciled.preferredStyles,
      });
      return reconciled;
    } catch (error) {
      observabilityLogger.warn("local_llm_query_parser_failed_fallback_to_heuristic", {
        errorMessage: error instanceof Error ? error.message : String(error),
        model: config.localLlmChatModel,
      });
      return heuristicQuery;
    }
  }

  if (!config.openAiApiKey) {
    return heuristicQuery;
  }

  try {
    return reconcileWithHeuristics(
      await parseQueryWithOpenAi(prompt),
      heuristicQuery,
    );
  } catch (error) {
    observabilityLogger.warn("openai_query_parser_failed_fallback_to_heuristic", {
      errorMessage: error instanceof Error ? error.message : String(error),
    });
    return heuristicQuery;
  }
}
