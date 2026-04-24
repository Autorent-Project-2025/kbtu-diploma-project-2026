import { createEmbedding } from "../embeddings";
import { sql } from "../db/sql";
import { isPartnerCarAvailableOnDates } from "../integrations/catalogClient";
import { ParsedRecommendationQuery, SearchCandidate } from "../types";
import { rerankCarsWithLlm } from "./llmReranker";
import { hasExplicitPreferredStyleIntent } from "../ai/heuristicQueryParser";
import {
  getAliasToCanonicalBrand,
  getAliasToCanonicalModel,
  getModelToBrandDictionary,
} from "../queryTaxonomy";
import { cosineSimilarity, getUserEmbedding } from "../personalization/userEmbeddings";

type RawSearchRow = {
  partnerCarId: number;
  carModelId: number;
  brand: string;
  model: string;
  year: number;
  title: string;
  imageUrl: string | null;
  detailsUrl: string;
  bookingUrl: string;
  priceHour: number | string | null;
  priceDay: number | string | null;
  rating: number | string | null;
  ratingsCount: number;
  carrierName: string | null;
  tags: string[];
  vectorDistance: number | string | null;
  lexicalScore: number | string | null;
  searchableText: string;
  transmission: string | null;
  seats: number | string | null;
};

type RankedSearchRow = Omit<
  RawSearchRow,
  "priceHour" | "priceDay" | "rating" | "vectorDistance" | "lexicalScore" | "seats"
> & {
  priceHour: number | null;
  priceDay: number | null;
  rating: number | null;
  vectorDistance: number | null;
  lexicalScore: number | null;
  seats: number | null;
};

function toVectorLiteral(values: number[]): string {
  return `[${values.map((value) => Number(value).toFixed(6)).join(",")}]`;
}

function normalizeScore(value: number | null | undefined): number {
  if (value == null || Number.isNaN(value)) {
    return 0;
  }

  return Math.max(0, Number(value));
}

function toNullableNumber(value: number | string | null | undefined): number | null {
  if (value == null || value === "") {
    return null;
  }

  const parsed = Number(value);
  return Number.isFinite(parsed) ? parsed : null;
}

function normalizeRow(row: RawSearchRow): RankedSearchRow {
  return {
    ...row,
    priceHour: toNullableNumber(row.priceHour),
    priceDay: toNullableNumber(row.priceDay),
    rating: toNullableNumber(row.rating),
    vectorDistance: toNullableNumber(row.vectorDistance),
    lexicalScore: toNullableNumber(row.lexicalScore),
    seats: toNullableNumber(row.seats),
  };
}

function buildReasons(row: RankedSearchRow, query: ParsedRecommendationQuery): string[] {
  const reasons: string[] = [];

  if (query.maxBudgetPerHour != null && row.priceHour != null) {
    if (row.priceHour <= query.maxBudgetPerHour) {
      reasons.push("укладывается в бюджет");
    } else if (row.priceHour <= query.maxBudgetPerHour * 1.25) {
      reasons.push("чуть выше бюджета, но ближе всего к запросу");
    }
  }

  if (query.preferredStyles.some((style) => row.tags.includes(style))) {
    reasons.push("совпадает по стилю");
  }

  if (query.minRating != null && row.rating != null && row.rating >= query.minRating) {
    reasons.push("подходит по рейтингу");
  }

  if (query.passengers != null && row.seats != null && row.seats >= query.passengers) {
    reasons.push("подходит по количеству мест");
  }

  if (
    (query.minYear != null && row.year >= query.minYear) ||
    (query.maxYear != null && row.year <= query.maxYear)
  ) {
    reasons.push("подходит по году выпуска");
  }

  if (row.rating != null && row.rating >= 4.5) {
    reasons.push("высокий рейтинг");
  }

  if (reasons.length === 0) {
    reasons.push("хорошее смысловое совпадение с запросом");
  }

  return reasons.slice(0, 3);
}

function computeBusinessScore(row: RankedSearchRow, query: ParsedRecommendationQuery): number {
  let score = 0;

  if (query.maxBudgetPerHour != null && row.priceHour != null) {
    if (row.priceHour <= query.maxBudgetPerHour) {
      score += 0.4;
    } else if (row.priceHour <= query.maxBudgetPerHour * 1.15) {
      score += 0.15;
    }
  }

  if (query.passengers != null && row.seats != null && row.seats >= query.passengers) {
    score += 0.2;
  }

  if (query.transmission && row.transmission?.toLowerCase() === query.transmission) {
    score += 0.15;
  }

  if (query.minRating != null && row.rating != null && row.rating >= query.minRating) {
    score += 0.2;
  }

  if (query.preferredStyles.some((style) => row.tags.includes(style))) {
    score += 0.2;
  }

  if (query.minYear != null && row.year >= query.minYear) {
    score += 0.1;
  }

  if (query.maxYear != null && row.year <= query.maxYear) {
    score += 0.1;
  }

  if (row.rating != null) {
    score += Math.min(row.rating / 5, 1) * 0.15;
  }

  return Math.min(score, 1);
}

function computeQueryExpansions(prompt: string): string[] {
  const expansions: string[] = [];
  const normalizedPrompt = prompt.toLowerCase();

  const aliasToModel = getAliasToCanonicalModel();
  for (const [alias, canonicalModel] of Object.entries(aliasToModel)) {
    if (normalizedPrompt.includes(alias)) {
      const brand = getModelToBrandDictionary()[alias];
      if (brand) {
        expansions.push(`${brand} ${canonicalModel}`);
      } else {
        expansions.push(canonicalModel);
      }
    }
  }

  const aliasToBrand = getAliasToCanonicalBrand();
  for (const [alias, canonicalBrand] of Object.entries(aliasToBrand)) {
    if (!(alias in aliasToModel) && normalizedPrompt.includes(alias)) {
      expansions.push(canonicalBrand);
    }
  }

  for (const [model, brand] of Object.entries(getModelToBrandDictionary())) {
    if (model in aliasToModel) continue;
    if (normalizedPrompt.includes(model)) {
      expansions.push(`${brand} ${model}`);
    }
  }

  return expansions;
}

export function buildBaseRetrievalPrompt(prompt: string): string {
  // Embedding-facing retrieval prompt: raw user text plus alias/model
  // expansions so cyrillic queries ("нужна камри") also match english
  // documents ("toyota camry"). Used for the vector channel of hybrid search.
  const expansions = computeQueryExpansions(prompt);
  const parts = [prompt.trim(), ...expansions].filter((p) => p && p.trim());
  return parts.join(" ").trim() || "car";
}

function buildLexicalQueryTokens(prompt: string, query: ParsedRecommendationQuery): string[] {
  // BM25-facing tokens: a distilled set of high-signal terms. Strips the
  // user's filler words (articles, verbs) that torpedo AND-based
  // websearch_to_tsquery recall. We feed this to to_tsquery with OR
  // semantics so any single matching token still activates the lexical
  // channel of hybrid search.
  const tokens = new Set<string>();

  for (const expansion of computeQueryExpansions(prompt)) {
    for (const word of expansion.split(/\s+/)) {
      const clean = word.trim().toLowerCase();
      if (clean && /^[\p{L}\p{N}]+$/u.test(clean)) tokens.add(clean);
    }
  }

  for (const brand of query.preferredBrands) {
    const clean = brand.trim().toLowerCase();
    if (clean) tokens.add(clean);
  }
  for (const style of query.preferredStyles) {
    const clean = style.trim().toLowerCase();
    if (clean) tokens.add(clean);
  }
  if (query.transmission) {
    tokens.add(query.transmission.toLowerCase());
  }

  // As a safety net — if no expansions/filters matched, fall back to the
  // longest alphabetic tokens from the raw prompt so lexical isn't empty.
  if (tokens.size === 0) {
    const words = prompt.toLowerCase().match(/[\p{L}\p{N}]{3,}/gu) ?? [];
    for (const w of words.slice(0, 5)) tokens.add(w);
  }

  return [...tokens];
}

function buildRetrievalPrompt(prompt: string, query: ParsedRecommendationQuery): string {
  const parts = [
    buildBaseRetrievalPrompt(prompt),
    ...query.preferredStyles,
    ...query.preferredBrands,
    query.transmission,
    query.minRating != null ? `${query.minRating} star rating or higher` : null,
    query.passengers != null ? `${query.passengers} seats` : null,
    query.maxBudgetPerHour != null ? `${query.maxBudgetPerHour} kzt per hour` : null,
    query.minYear != null ? `year from ${query.minYear}` : null,
    query.maxYear != null ? `year up to ${query.maxYear}` : null,
    ...query.excludedStyles.map((style) => `not ${style}`),
  ].filter((part): part is string => Boolean(part && part.trim()));

  return parts.join(" ").trim() || prompt.trim() || "car";
}

async function fetchCandidates(
  prompt: string,
  embedding: number[],
  query: ParsedRecommendationQuery,
): Promise<RawSearchRow[]> {
  const lexicalTokens = buildLexicalQueryTokens(prompt, query);
  // to_tsquery('simple', 'toyota | camry | sedan') — OR semantics so any
  // of the high-signal tokens activates the lexical channel.
  const lexicalQuery = lexicalTokens.length > 0 ? lexicalTokens.join(" | ") : "car";
  const vectorLiteral = toVectorLiteral(embedding);
  const brandsArray = sql.array(query.preferredBrands, 25);
  const RRF_K = 60;
  const CANDIDATE_POOL = 60;
  const FINAL_LIMIT = 24;

  // Hybrid search via Reciprocal Rank Fusion: each document receives
  // rank-based scores from both the vector neighborhood (by cosine distance)
  // and the BM25-style lexical match (ts_rank_cd). Fused score is
  // sum(1 / (k + rank)). This is resilient to weak signals on either side —
  // a document that ranks well in lexical-only ("cobalt" exact token match)
  // survives even when the embedding is noisy, and vice versa.
  const rows = await sql<RawSearchRow[]>`
    with filtered as (
      select
        partner_car_id,
        car_model_id,
        brand,
        model,
        year,
        title,
        image_url,
        details_url,
        booking_url,
        price_hour,
        price_day,
        rating,
        ratings_count,
        carrier_name,
        tags,
        searchable_text,
        transmission,
        seats,
        vector_embedding,
        lexical_document
      from ai_car_documents
      where (${query.maxBudgetPerHour ?? null}::numeric is null or price_hour is null or price_hour <= ${query.maxBudgetPerHour ?? null} * 1.25)
        and (${query.passengers ?? null}::int is null or seats is null or seats >= ${query.passengers ?? null})
        and (${query.minYear ?? null}::int is null or year >= ${query.minYear ?? null})
        and (${query.maxYear ?? null}::int is null or year <= ${query.maxYear ?? null})
        and (${query.transmission ?? null}::text is null or lower(coalesce(transmission, '')) = ${query.transmission ?? null})
        and (${query.minRating ?? null}::numeric is null or (rating is not null and rating >= ${query.minRating ?? null}))
        and (
          cardinality(${brandsArray}) = 0
          or lower(brand) = any(${brandsArray})
        )
    ),
    vector_ranked as (
      select
        partner_car_id,
        (vector_embedding <=> ${vectorLiteral}::vector) as vector_distance,
        row_number() over (order by vector_embedding <=> ${vectorLiteral}::vector) as vec_rank
      from filtered
      order by vector_embedding <=> ${vectorLiteral}::vector
      limit ${CANDIDATE_POOL}
    ),
    lexical_ranked as (
      select
        partner_car_id,
        ts_rank_cd(lexical_document, to_tsquery('simple', ${lexicalQuery})) as lexical_score,
        row_number() over (order by ts_rank_cd(lexical_document, to_tsquery('simple', ${lexicalQuery})) desc) as lex_rank
      from filtered
      where lexical_document @@ to_tsquery('simple', ${lexicalQuery})
      order by ts_rank_cd(lexical_document, to_tsquery('simple', ${lexicalQuery})) desc
      limit ${CANDIDATE_POOL}
    ),
    fused as (
      select
        partner_car_id,
        sum(score) as rrf_score,
        max(vector_distance) as vector_distance,
        max(lexical_score) as lexical_score
      from (
        -- Vector channel (weight 0.4): semantic similarity.
        select partner_car_id, 0.4 / (${RRF_K} + vec_rank) as score, vector_distance, null::real as lexical_score
        from vector_ranked
        union all
        -- Lexical channel (weight 0.6): exact/stem token match. Weighted
        -- higher because named-entity queries ("camry", "cobalt") are
        -- discriminated far better by BM25 than by weak multilingual
        -- embeddings that tend to blur siblings of the same brand.
        select partner_car_id, 0.6 / (${RRF_K} + lex_rank) as score, null::float8 as vector_distance, lexical_score
        from lexical_ranked
      ) ranks
      group by partner_car_id
    )
    select
      f.partner_car_id as "partnerCarId",
      f.car_model_id as "carModelId",
      f.brand,
      f.model,
      f.year,
      f.title,
      f.image_url as "imageUrl",
      f.details_url as "detailsUrl",
      f.booking_url as "bookingUrl",
      f.price_hour as "priceHour",
      f.price_day as "priceDay",
      f.rating,
      f.ratings_count as "ratingsCount",
      f.carrier_name as "carrierName",
      array(select jsonb_array_elements_text(f.tags)) as tags,
      f.searchable_text as "searchableText",
      f.transmission,
      f.seats,
      fused.vector_distance as "vectorDistance",
      coalesce(fused.lexical_score, 0) as "lexicalScore"
    from fused
    join filtered f on f.partner_car_id = fused.partner_car_id
    order by fused.rrf_score desc
    limit ${FINAL_LIMIT}
  `;

  return rows;
}

async function applyAvailabilityFilter(
  rows: RawSearchRow[],
  query: ParsedRecommendationQuery,
): Promise<RawSearchRow[]> {
  if (!query.requiresAvailableOnDates || !query.startTime || !query.endTime) {
    return rows;
  }

  const checks = await Promise.all(
    rows.map(async (row) => ({
      row,
      isAvailable: await isPartnerCarAvailableOnDates(
        row.partnerCarId,
        query.startTime!,
        query.endTime!,
      ),
    })),
  );

  return checks.filter((item) => item.isAvailable).map((item) => item.row);
}

function applyPreferredStyleFilter(
  rows: RawSearchRow[],
  query: ParsedRecommendationQuery,
): RawSearchRow[] {
  if (query.preferredStyles.length === 0) {
    return rows;
  }

  const matchedRows = rows.filter((row) =>
    query.preferredStyles.some((style) => row.tags?.includes(style)),
  );

  if (matchedRows.length > 0) {
    return matchedRows;
  }

  if (hasExplicitPreferredStyleIntent(query.prompt, query.preferredStyles)) {
    return [];
  }

  return rows;
}

function applyExcludedStyleFilter(
  rows: RawSearchRow[],
  query: ParsedRecommendationQuery,
): RawSearchRow[] {
  if (query.excludedStyles.length === 0) {
    return rows;
  }

  return rows.filter(
    (row) => !query.excludedStyles.some((style) => row.tags?.includes(style)),
  );
}

function applyBudgetFilter(
  rows: RawSearchRow[],
  query: ParsedRecommendationQuery,
): RawSearchRow[] {
  if (query.maxBudgetPerHour == null) {
    return rows;
  }

  const exactBudgetRows = rows.filter((row) => {
    const priceHour = toNullableNumber(row.priceHour);
    return priceHour != null && priceHour <= query.maxBudgetPerHour!;
  });

  return exactBudgetRows.length > 0 ? exactBudgetRows : rows;
}

async function applyPersonalizationBoost(
  candidates: SearchCandidate[],
  userId: string | null | undefined,
): Promise<SearchCandidate[]> {
  if (!userId || candidates.length === 0) return candidates;
  const userVector = await getUserEmbedding(userId);
  if (!userVector) return candidates;

  const ids = candidates.map((c) => c.partnerCarId);
  const docs = await sql<{ partner_car_id: number; vector_embedding: number[] }[]>`
    select partner_car_id, vector_embedding::text::real[] as vector_embedding
    from ai_car_documents
    where partner_car_id = any(${sql.array(ids, 23)})
  `;
  if (docs.length === 0) return candidates;

  const vecByCar = new Map(docs.map((d) => [d.partner_car_id, d.vector_embedding]));
  const boosted = candidates.map((candidate) => {
    const docVec = vecByCar.get(candidate.partnerCarId);
    if (!docVec) return candidate;
    const similarity = cosineSimilarity(userVector, docVec);
    const personalBoost = Math.max(0, similarity) * 0.15;
    return {
      ...candidate,
      finalScore: Number((candidate.finalScore + personalBoost).toFixed(6)),
      reasons: similarity > 0.6
        ? [...candidate.reasons.slice(0, 2), "похоже на ваши прошлые выборы"].slice(0, 3)
        : candidate.reasons,
    };
  });
  return [...boosted].sort((a, b) => b.finalScore - a.finalScore);
}

export async function searchCars(
  prompt: string,
  query: ParsedRecommendationQuery,
  precomputedEmbedding?: number[] | null,
  userId?: string | null,
): Promise<SearchCandidate[]> {
  const retrievalPrompt = buildRetrievalPrompt(prompt, query);
  const embedding = precomputedEmbedding ?? (await createEmbedding(retrievalPrompt));
  const candidates = await fetchCandidates(prompt, embedding, query);
  const availableCandidates = await applyAvailabilityFilter(candidates, query);
  const styleFilteredCandidates = applyPreferredStyleFilter(availableCandidates, query);
  const excludedStyleFilteredCandidates = applyExcludedStyleFilter(styleFilteredCandidates, query);
  const filteredCandidates = applyBudgetFilter(excludedStyleFilteredCandidates, query);

  const scoredCandidates = filteredCandidates
    .map((rawRow) => {
      const row = normalizeRow(rawRow);
      const vectorScore = row.vectorDistance == null ? 0 : Math.max(0, 1 - row.vectorDistance);
      const lexicalScore = normalizeScore(row.lexicalScore);
      const businessScore = computeBusinessScore(row, query);
      const finalScore = vectorScore * 0.5 + lexicalScore * 0.2 + businessScore * 0.3;

      return {
        partnerCarId: row.partnerCarId,
        carModelId: row.carModelId,
        brand: row.brand,
        model: row.model,
        year: row.year,
        title: row.title,
        imageUrl: row.imageUrl,
        detailsUrl: row.detailsUrl,
        bookingUrl: row.bookingUrl,
        priceHour: row.priceHour,
        priceDay: row.priceDay,
        rating: row.rating,
        ratingsCount: row.ratingsCount,
        carrierName: row.carrierName,
        tags: row.tags ?? [],
        lexicalScore: Number(lexicalScore.toFixed(6)),
        vectorScore: Number(vectorScore.toFixed(6)),
        businessScore: Number(businessScore.toFixed(6)),
        finalScore: Number(finalScore.toFixed(6)),
        reasons: buildReasons(row, query),
      } satisfies SearchCandidate;
    })
    .sort((left, right) => right.finalScore - left.finalScore);

  const rerankedByLlm = await rerankCarsWithLlm(query, scoredCandidates);
  const personalized = await applyPersonalizationBoost(rerankedByLlm, userId);
  return personalized.slice(0, 6);
}
