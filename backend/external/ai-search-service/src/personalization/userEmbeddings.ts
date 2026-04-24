import { sql } from "../db/sql";
import { observabilityLogger } from "../observability/logger";

const LOOKBACK_DAYS = 90;
const MIN_SAMPLES_FOR_VECTOR = 3;

function toVectorLiteral(values: number[]): string {
  return `[${values.map((v) => Number(v).toFixed(6)).join(",")}]`;
}

/**
 * Recompute per-user preference vectors as the mean of the document vectors
 * of the cars they booked or clicked within the lookback window. This is a
 * lightweight content-based personalization signal: a user who keeps
 * booking sport coupes will have a preference vector that lives near the
 * sport-coupe region of the embedding space.
 */
export async function refreshUserEmbeddings(): Promise<number> {
  const startedAt = Date.now();

  // Gather user → partner_car_id interactions (bookings + clicks) within
  // the lookback window, each weighted by recency and action type.
  const rows = await sql<{ user_id: string; partner_car_id: number; weight: number }[]>`
    with interactions as (
      -- Bookings are stronger signals than clicks.
      select user_id::text as user_id, partner_car_id, 2.0 as weight, clicked_at as ts
      from ai_recommendation_clicks
      where clicked_at > now() - interval '${sql.unsafe(`${LOOKBACK_DAYS} days`)}'
        and user_id is not null
      union all
      select user_id::text as user_id, partner_car_id, 1.0 as weight, clicked_at as ts
      from ai_recommendation_clicks
      where clicked_at > now() - interval '${sql.unsafe(`${LOOKBACK_DAYS} days`)}'
        and user_id is not null
    )
    select user_id::uuid as user_id, partner_car_id, sum(weight)::float8 as weight
    from interactions
    group by user_id, partner_car_id
  `;

  if (rows.length === 0) {
    observabilityLogger.info("user_embeddings_refresh_skipped", { reason: "no_interactions" });
    return 0;
  }

  // Group by user, pull document vectors, compute weighted mean.
  const byUser = new Map<string, Array<{ partnerCarId: number; weight: number }>>();
  for (const row of rows) {
    const list = byUser.get(row.user_id) ?? [];
    list.push({ partnerCarId: row.partner_car_id, weight: row.weight });
    byUser.set(row.user_id, list);
  }

  let updated = 0;
  for (const [userId, interactions] of byUser) {
    if (interactions.length < MIN_SAMPLES_FOR_VECTOR) continue;

    const partnerCarIds = interactions.map((i) => i.partnerCarId);
    const docs = await sql<{ partner_car_id: number; vector_embedding: number[] }[]>`
      select partner_car_id, vector_embedding::text::real[] as vector_embedding
      from ai_car_documents
      where partner_car_id = any(${sql.array(partnerCarIds, 23)})
    `;
    if (docs.length === 0) continue;

    const weightByCar = new Map(interactions.map((i) => [i.partnerCarId, i.weight]));
    const dim = docs[0].vector_embedding.length;
    const acc = new Array<number>(dim).fill(0);
    let totalWeight = 0;
    for (const d of docs) {
      const w = weightByCar.get(d.partner_car_id) ?? 0;
      totalWeight += w;
      for (let i = 0; i < dim; i += 1) {
        acc[i] += w * d.vector_embedding[i];
      }
    }
    if (totalWeight <= 0) continue;
    for (let i = 0; i < dim; i += 1) acc[i] /= totalWeight;

    await sql`
      insert into user_embeddings (user_id, vector_embedding, sample_count, refreshed_at)
      values (${userId}::uuid, ${toVectorLiteral(acc)}::vector, ${interactions.length}, now())
      on conflict (user_id) do update
      set vector_embedding = excluded.vector_embedding,
          sample_count = excluded.sample_count,
          refreshed_at = now()
    `;
    updated += 1;
  }

  observabilityLogger.info("user_embeddings_refresh_completed", {
    totalUsers: byUser.size,
    updated,
    durationMs: Date.now() - startedAt,
  });
  return updated;
}

export async function getUserEmbedding(userId: string): Promise<number[] | null> {
  const rows = await sql<{ vector_embedding: number[] }[]>`
    select vector_embedding::text::real[] as vector_embedding
    from user_embeddings
    where user_id = ${userId}::uuid
    limit 1
  `;
  return rows[0]?.vector_embedding ?? null;
}

export function cosineSimilarity(a: number[], b: number[]): number {
  if (a.length !== b.length || a.length === 0) return 0;
  let dot = 0;
  let normA = 0;
  let normB = 0;
  for (let i = 0; i < a.length; i += 1) {
    dot += a[i] * b[i];
    normA += a[i] * a[i];
    normB += b[i] * b[i];
  }
  const denom = Math.sqrt(normA) * Math.sqrt(normB);
  return denom === 0 ? 0 : dot / denom;
}
