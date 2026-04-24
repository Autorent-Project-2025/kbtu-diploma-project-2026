import { completeWithPreferredLlm, describeConfiguredLlm } from "../llm/chatCompletion";
import { observabilityLogger } from "../observability/logger";
import { ParsedRecommendationQuery, SearchCandidate } from "../types";

const MAX_CANDIDATES_TO_RERANK = 10;
const LLM_RERANK_TIMEOUT_MS = 5000;

type RerankResponse = {
  rankedPartnerCarIds?: number[];
};

function buildRerankPrompt(
  query: ParsedRecommendationQuery,
  candidates: SearchCandidate[],
): { systemPrompt: string; userPrompt: string } {
  return {
    systemPrompt: `
You rerank car recommendations for AutoRent.
Return only valid JSON.
Choose the best overall order for the provided candidates based on the user query and extracted filters.
   Respect explicit constraints first: budget, seats, transmission, rating, dates, excluded styles, preferred brands, minYear, maxYear.
Then optimize for overall scenario fit and user intent.
Do not invent candidate facts.
Return this schema:
{
  "rankedPartnerCarIds": number[]
}
Include only ids from the provided list, without duplicates.
If several options are close, still produce the best order you can.
`.trim(),
    userPrompt: `
User query:
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

Candidates:
${JSON.stringify(
  candidates.map((candidate, index) => ({
    rank: index + 1,
    partnerCarId: candidate.partnerCarId,
    title: candidate.title,
    brand: candidate.brand,
    model: candidate.model,
    year: candidate.year,
    priceHour: candidate.priceHour,
    priceDay: candidate.priceDay,
    rating: candidate.rating,
    ratingsCount: candidate.ratingsCount,
    carrierName: candidate.carrierName,
    tags: candidate.tags,
    reasons: candidate.reasons,
    deterministicScores: {
      vectorScore: candidate.vectorScore,
      lexicalScore: candidate.lexicalScore,
      businessScore: candidate.businessScore,
      finalScore: candidate.finalScore,
    },
  })),
  null,
  2,
)}
`.trim(),
  };
}

function normalizeRankedIds(
  rankedPartnerCarIds: unknown,
  knownCandidates: SearchCandidate[],
): number[] {
  if (!Array.isArray(rankedPartnerCarIds)) {
    return [];
  }

  const knownIds = new Set(knownCandidates.map((candidate) => candidate.partnerCarId));
  const orderedIds: number[] = [];
  const seenIds = new Set<number>();

  for (const rawId of rankedPartnerCarIds) {
    const partnerCarId = Number(rawId);
    if (!Number.isInteger(partnerCarId) || seenIds.has(partnerCarId) || !knownIds.has(partnerCarId)) {
      continue;
    }

    seenIds.add(partnerCarId);
    orderedIds.push(partnerCarId);
  }

  return orderedIds;
}

const MIN_CANDIDATES_FOR_RERANK = 4;

export async function rerankCarsWithLlm(
  query: ParsedRecommendationQuery,
  candidates: SearchCandidate[],
): Promise<SearchCandidate[]> {
  if (candidates.length < MIN_CANDIDATES_FOR_RERANK) {
    return candidates;
  }

  const llm = describeConfiguredLlm();
  if (!llm) {
    return candidates;
  }

  const rerankWindow = candidates.slice(0, MAX_CANDIDATES_TO_RERANK);
  const { systemPrompt, userPrompt } = buildRerankPrompt(query, rerankWindow);

  try {
    const completion = await completeWithPreferredLlm({
      systemPrompt,
      userPrompt,
      responseType: "json",
      temperature: 0,
      maxOutputTokens: 200,
      timeoutMs: LLM_RERANK_TIMEOUT_MS,
    });

    if (!completion) {
      return candidates;
    }

    const payload = JSON.parse(completion.content) as RerankResponse;
    const orderedIds = normalizeRankedIds(payload.rankedPartnerCarIds, rerankWindow);
    if (orderedIds.length === 0) {
      throw new Error("LLM reranker returned no usable ids.");
    }

    const llmRankMap = new Map(orderedIds.map((partnerCarId, index) => [partnerCarId, index]));
    const rerankedWindow = [...rerankWindow].sort((left, right) => {
      const leftRank = llmRankMap.get(left.partnerCarId) ?? Number.MAX_SAFE_INTEGER;
      const rightRank = llmRankMap.get(right.partnerCarId) ?? Number.MAX_SAFE_INTEGER;
      return leftRank - rightRank || right.finalScore - left.finalScore;
    });

    observabilityLogger.info("llm_search_rerank_succeeded", {
      provider: completion.provider,
      model: completion.model,
      rerankCandidatesCount: rerankWindow.length,
      reorderedIds: orderedIds,
    });

    return [
      ...rerankedWindow,
      ...candidates.slice(MAX_CANDIDATES_TO_RERANK),
    ];
  } catch (error) {
    observabilityLogger.warn("llm_search_rerank_failed_fallback_to_deterministic", {
      provider: llm.provider,
      model: llm.model,
      errorMessage: error instanceof Error ? error.message : String(error),
      rerankCandidatesCount: rerankWindow.length,
    });
    return candidates;
  }
}
