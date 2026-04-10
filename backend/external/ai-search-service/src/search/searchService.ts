import { createEmbedding } from "../embeddings";
import { sql } from "../db/sql";
import { isPartnerCarAvailableOnDates } from "../integrations/catalogClient";
import { ParsedRecommendationQuery, SearchCandidate } from "../types";

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

  if (row.rating != null) {
    score += Math.min(row.rating / 5, 1) * 0.15;
  }

  return Math.min(score, 1);
}

function buildRetrievalPrompt(prompt: string, query: ParsedRecommendationQuery): string {
  const parts = [
    prompt.trim(),
    ...query.preferredStyles,
    ...query.preferredBrands,
    query.transmission,
    query.minRating != null ? `${query.minRating} star rating or higher` : null,
    query.passengers != null ? `${query.passengers} seats` : null,
    query.maxBudgetPerHour != null ? `${query.maxBudgetPerHour} kzt per hour` : null,
    ...query.excludedStyles.map((style) => `not ${style}`),
  ].filter((part): part is string => Boolean(part && part.trim()));

  return parts.join(" ").trim() || prompt.trim() || "car";
}

async function fetchCandidates(
  retrievalPrompt: string,
  embedding: number[],
  query: ParsedRecommendationQuery,
): Promise<RawSearchRow[]> {
  const lexicalQuery = retrievalPrompt.trim() || "car";
  const rows = await sql<RawSearchRow[]>`
    with ranked_documents as (
      select
        partner_car_id as "partnerCarId",
        car_model_id as "carModelId",
        brand,
        model,
        year,
        title,
        image_url as "imageUrl",
        details_url as "detailsUrl",
        booking_url as "bookingUrl",
        price_hour as "priceHour",
        price_day as "priceDay",
        rating,
        ratings_count as "ratingsCount",
        carrier_name as "carrierName",
        array(select jsonb_array_elements_text(tags)) as tags,
        searchable_text as "searchableText",
        transmission,
        seats,
        (vector_embedding <=> ${toVectorLiteral(embedding)}::vector) as "vectorDistance",
        ts_rank_cd(lexical_document, websearch_to_tsquery('simple', ${lexicalQuery})) as "lexicalScore"
      from ai_car_documents
      where (${query.maxBudgetPerHour ?? null}::numeric is null or price_hour is null or price_hour <= ${query.maxBudgetPerHour ?? null} * 1.25)
        and (${query.passengers ?? null}::int is null or seats is null or seats >= ${query.passengers ?? null})
        and (${query.minYear ?? null}::int is null or year >= ${query.minYear ?? null})
        and (${query.transmission ?? null}::text is null or lower(coalesce(transmission, '')) = ${query.transmission ?? null})
        and (${query.minRating ?? null}::numeric is null or (rating is not null and rating >= ${query.minRating ?? null}))
        and (
          cardinality(${sql.array(query.preferredBrands, 25)}) = 0
          or lower(brand) = any(${sql.array(query.preferredBrands, 25)})
        )
      order by
        (vector_embedding <=> ${toVectorLiteral(embedding)}::vector) asc,
        ts_rank_cd(lexical_document, websearch_to_tsquery('simple', ${lexicalQuery})) desc
      limit 24
    )
    select *
    from ranked_documents
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

  return matchedRows.length > 0 ? matchedRows : rows;
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

export async function searchCars(
  prompt: string,
  query: ParsedRecommendationQuery,
): Promise<SearchCandidate[]> {
  const retrievalPrompt = buildRetrievalPrompt(prompt, query);
  const embedding = await createEmbedding(retrievalPrompt);
  const candidates = await fetchCandidates(retrievalPrompt, embedding, query);
  const availableCandidates = await applyAvailabilityFilter(candidates, query);
  const styleFilteredCandidates = applyPreferredStyleFilter(availableCandidates, query);
  const excludedStyleFilteredCandidates = applyExcludedStyleFilter(styleFilteredCandidates, query);
  const filteredCandidates = applyBudgetFilter(excludedStyleFilteredCandidates, query);

  return filteredCandidates
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
    .sort((left, right) => right.finalScore - left.finalScore)
    .slice(0, 6);
}
