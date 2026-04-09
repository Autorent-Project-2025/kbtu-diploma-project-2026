import { createEmbedding } from "../embeddings";
import { sql } from "../db/sql";
import { observabilityLogger } from "../observability/logger";
import {
  getAvailableModels,
  getAvailablePartnerCarsByModel,
  getCarModelDetails,
  getPartnerCarDetails,
  getPartnerPublicProfile,
} from "../integrations/catalogClient";
import { buildSearchTags } from "../search/tagBuilder";
import { SearchDocument } from "../types";

function buildSearchableText(parts: Array<string | null | undefined>): string {
  return parts
    .map((item) => item?.trim())
    .filter(Boolean)
    .join(" ")
    .replace(/\s+/g, " ")
    .trim();
}

function toVectorLiteral(values: number[]): string {
  return `[${values.map((value) => Number(value).toFixed(6)).join(",")}]`;
}

export async function ensureSchemaReachable() {
  await sql`select 1`;
}

async function upsertDocument(document: SearchDocument) {
  await sql`
    insert into ai_car_documents (
      partner_car_id,
      car_model_id,
      partner_user_id,
      carrier_name,
      brand,
      model,
      year,
      title,
      description,
      color,
      transmission,
      fuel_type,
      engine,
      seats,
      price_hour,
      price_day,
      rating,
      ratings_count,
      image_url,
      details_url,
      booking_url,
      tags,
      searchable_text,
      vector_embedding,
      updated_at
    )
    values (
      ${document.partnerCarId},
      ${document.carModelId},
      ${document.partnerUserId},
      ${document.carrierName},
      ${document.brand},
      ${document.model},
      ${document.year},
      ${document.title},
      ${document.description},
      ${document.color},
      ${document.transmission},
      ${document.fuelType},
      ${document.engine},
      ${document.seats},
      ${document.priceHour},
      ${document.priceDay},
      ${document.rating},
      ${document.ratingsCount},
      ${document.imageUrl},
      ${document.detailsUrl},
      ${document.bookingUrl},
      ${sql.json(document.tags)},
      ${document.searchableText},
      ${toVectorLiteral(document.embedding)}::vector,
      now()
    )
    on conflict (partner_car_id) do update
    set
      car_model_id = excluded.car_model_id,
      partner_user_id = excluded.partner_user_id,
      carrier_name = excluded.carrier_name,
      brand = excluded.brand,
      model = excluded.model,
      year = excluded.year,
      title = excluded.title,
      description = excluded.description,
      color = excluded.color,
      transmission = excluded.transmission,
      fuel_type = excluded.fuel_type,
      engine = excluded.engine,
      seats = excluded.seats,
      price_hour = excluded.price_hour,
      price_day = excluded.price_day,
      rating = excluded.rating,
      ratings_count = excluded.ratings_count,
      image_url = excluded.image_url,
      details_url = excluded.details_url,
      booking_url = excluded.booking_url,
      tags = excluded.tags,
      searchable_text = excluded.searchable_text,
      vector_embedding = excluded.vector_embedding,
      updated_at = now()
  `;
}

async function deleteMissingDocuments(activePartnerCarIds: number[]) {
  if (activePartnerCarIds.length === 0) {
    await sql`delete from ai_car_documents`;
    return;
  }

  await sql`
    delete from ai_car_documents
    where not (partner_car_id = any(${sql.array(activePartnerCarIds, 23)}))
  `;
}

function resolveImageUrl(
  partnerCarDetails: Awaited<ReturnType<typeof getPartnerCarDetails>>,
  modelDetails: Awaited<ReturnType<typeof getCarModelDetails>>,
): string | null {
  return (
    partnerCarDetails.images?.[0]?.imageUrl ??
    modelDetails.images?.[0]?.imageUrl ??
    null
  );
}

async function buildDocument(partnerCarId: number): Promise<SearchDocument | null> {
  const partnerCar = await getPartnerCarDetails(partnerCarId);

  if (partnerCar.status !== 0) {
    return null;
  }

  const [modelDetails, partnerProfile] = await Promise.all([
    getCarModelDetails(partnerCar.carModelId),
    getPartnerPublicProfile(partnerCar.partnerUserId),
  ]);

  const tags = buildSearchTags({
    brand: modelDetails.brand,
    model: modelDetails.model,
    year: modelDetails.year,
    engine: modelDetails.engine,
    transmission: modelDetails.transmission,
    fuelType: modelDetails.fuelType,
    seats: modelDetails.seats,
    features: modelDetails.features ?? [],
    priceHour: partnerCar.priceHour ?? null,
  });

  const searchableText = buildSearchableText([
    `${modelDetails.brand} ${modelDetails.model} ${modelDetails.year}`,
    modelDetails.description,
    partnerCar.color,
    modelDetails.engine,
    modelDetails.transmission,
    modelDetails.fuelType,
    typeof modelDetails.seats === "number" ? `${modelDetails.seats} seats` : null,
    partnerProfile?.carrierName ?? null,
    tags.join(" "),
    ...(partnerCar.comments ?? []).map((comment) => comment.content),
  ]);

  const embedding = await createEmbedding(searchableText);

  return {
    partnerCarId: partnerCar.id,
    carModelId: partnerCar.carModelId,
    partnerUserId: partnerCar.partnerUserId,
    carrierName: partnerProfile?.carrierName ?? null,
    brand: modelDetails.brand,
    model: modelDetails.model,
    year: modelDetails.year,
    title: `${modelDetails.brand} ${modelDetails.model} ${modelDetails.year}`,
    description: modelDetails.description ?? null,
    color: partnerCar.color ?? null,
    transmission: modelDetails.transmission ?? null,
    fuelType: modelDetails.fuelType ?? null,
    engine: modelDetails.engine ?? null,
    seats: modelDetails.seats ?? null,
    priceHour: partnerCar.priceHour ?? null,
    priceDay: partnerCar.priceDay ?? null,
    rating: partnerCar.rating ?? null,
    ratingsCount: partnerCar.ratingsCount ?? 0,
    imageUrl: resolveImageUrl(partnerCar, modelDetails),
    detailsUrl: `/cars/${modelDetails.id}`,
    bookingUrl: `/cars/${modelDetails.id}`,
    tags,
    searchableText,
    embedding,
  };
}

export async function reindexEverything(): Promise<number> {
  const startedAt = Date.now();
  const availableModels = await getAvailableModels();
  const activePartnerCarIds: number[] = [];
  let indexedCount = 0;

  for (const model of availableModels) {
    const partnerCars = await getAvailablePartnerCarsByModel(model.modelId);

    for (const partnerCar of partnerCars) {
      activePartnerCarIds.push(partnerCar.id);
      const document = await buildDocument(partnerCar.id);
      if (!document) {
        await deleteDocumentByPartnerCarId(partnerCar.id);
        continue;
      }

      await upsertDocument(document);
      indexedCount += 1;
    }
  }

  await deleteMissingDocuments(activePartnerCarIds);

  observabilityLogger.info("search_index_full_refresh_completed", {
    indexedCount,
    durationMs: Date.now() - startedAt,
  });

  return indexedCount;
}

export async function reindexPartnerCar(partnerCarId: number): Promise<boolean> {
  const document = await buildDocument(partnerCarId);
  if (!document) {
    await deleteDocumentByPartnerCarId(partnerCarId);
    observabilityLogger.info("search_index_partner_car_removed", { partnerCarId });
    return false;
  }

  await upsertDocument(document);
  observabilityLogger.info("search_index_partner_car_upserted", { partnerCarId });
  return true;
}

export async function deleteDocumentByPartnerCarId(partnerCarId: number) {
  await sql`delete from ai_car_documents where partner_car_id = ${partnerCarId}`;
}
