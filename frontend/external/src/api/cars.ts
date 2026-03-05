import api from "./axios";
import { getPartnerPublicProfileByRelatedUserId } from "./partners";
import type { PaginatedResponse } from "../types/Pagination";

function toCamelCase(obj: any): any {
  if (Array.isArray(obj)) {
    return obj.map(toCamelCase);
  }

  if (obj !== null && typeof obj === "object") {
    return Object.keys(obj).reduce((result, key) => {
      const camelKey = key.charAt(0).toLowerCase() + key.slice(1);
      result[camelKey] = toCamelCase(obj[key]);
      return result;
    }, {} as any);
  }

  return obj;
}

export interface AvailableCarModel {
  modelId: number;
  brand: string;
  model: string;
  year: number;
  availableCarsCount: number;
  minPriceHour?: number | null;
  maxPriceHour?: number | null;
  averageRating?: number | null;
}

export interface AvailableModelCard extends AvailableCarModel {
  imageUrl: string | null;
  description?: string | null;
}

export interface CarModelImageDto {
  id: number;
  imageId?: string | null;
  imageUrl: string;
  imageType: number;
  displayOrder: number;
}

export interface CarModelFeatureDto {
  name: string;
}

export interface CarModelDetailsDto {
  id: number;
  brand: string;
  model: string;
  year: number;
  engine?: string | null;
  transmission?: string | null;
  seats?: number | null;
  fuelType?: string | null;
  doors?: number | null;
  description?: string | null;
  rating?: number | null;
  ratingsCount: number;
  priceHour?: number | null;
  priceDay?: number | null;
  features: CarModelFeatureDto[];
  images: CarModelImageDto[];
}

export interface PartnerCarSummaryDto {
  id: number;
  partnerId: string;
  carModelId: number;
  licensePlate: string;
  priceHour?: number | null;
  priceDay?: number | null;
  status: number;
  rating?: number | null;
  ratingsCount: number;
  modelBrand: string;
  modelName: string;
  modelYear: number;
}

export interface CarCommentDto {
  id: number;
  userId: string;
  userName: string;
  carId: number;
  partnerCarId?: number | null;
  content: string;
  rating: number;
  createdOn: string;
}

export interface ModelReviewDto extends CarCommentDto {
  carrierPartnerId: string;
  carrierName: string;
}

export interface ModelDetailsPayload {
  model: CarModelDetailsDto;
  availability: AvailableCarModel | null;
  reviews: ModelReviewDto[];
  minPriceHour: number | null;
  maxPriceHour: number | null;
}

export interface MatchCarByModelPayload {
  modelId: number;
  startTime: string;
  endTime: string;
}

export interface MatchCarByModelResult {
  isAvailable: boolean;
  partnerCarId?: number | null;
  partnerId?: string | null;
  priceHour?: number | null;
  modelBrand?: string | null;
  modelName?: string | null;
  modelYear?: number | null;
  suggestedStartTimesUtc?: string[];
}

const unknownCarrierName = "Неизвестный перевозчик";
const carrierNameCache = new Map<string, string>();

function getMinPrice(cars: PartnerCarSummaryDto[]): number | null {
  const prices = cars
    .map((item) => item.priceHour)
    .filter((value): value is number => value !== null && value !== undefined);

  return prices.length > 0 ? Math.min(...prices) : null;
}

function getMaxPrice(cars: PartnerCarSummaryDto[]): number | null {
  const prices = cars
    .map((item) => item.priceHour)
    .filter((value): value is number => value !== null && value !== undefined);

  return prices.length > 0 ? Math.max(...prices) : null;
}

async function resolveCarrierName(relatedUserId: string): Promise<string> {
  const normalized = (relatedUserId ?? "").trim();
  if (!normalized) {
    return unknownCarrierName;
  }

  const cached = carrierNameCache.get(normalized);
  if (cached) {
    return cached;
  }

  try {
    const profile = await getPartnerPublicProfileByRelatedUserId(normalized);
    const resolved = (profile.carrierName ?? "").trim() || unknownCarrierName;
    carrierNameCache.set(normalized, resolved);
    return resolved;
  } catch {
    carrierNameCache.set(normalized, unknownCarrierName);
    return unknownCarrierName;
  }
}

export async function getAvailableCarModels(): Promise<AvailableCarModel[]> {
  const response = await api.get("/cars/available-models");
  const payload = toCamelCase(response.data);
  return (payload ?? []) as AvailableCarModel[];
}

export async function getCarModelDetails(modelId: number): Promise<CarModelDetailsDto> {
  const response = await api.get(`/cars/models/${modelId}`);
  return toCamelCase(response.data) as CarModelDetailsDto;
}

export async function getPartnerCarsByModel(
  modelId: number,
  pageSize = 200
): Promise<PartnerCarSummaryDto[]> {
  const response = await api.get("/cars/partner-cars", {
    params: {
      carModelId: modelId,
      page: 1,
      pageSize,
    },
  });

  const payload = toCamelCase(response.data);
  if (Array.isArray(payload)) {
    return payload as PartnerCarSummaryDto[];
  }

  return ((payload as PaginatedResponse<PartnerCarSummaryDto>).items ?? []) as PartnerCarSummaryDto[];
}

export async function getPartnerCarComments(
  partnerCarId: number,
  pageSize = 100
): Promise<CarCommentDto[]> {
  const response = await api.get(`/cars/comments/partner-cars/${partnerCarId}`, {
    params: {
      page: 1,
      pageSize,
    },
  });

  const payload = toCamelCase(response.data);
  if (Array.isArray(payload)) {
    return payload as CarCommentDto[];
  }

  return ((payload as PaginatedResponse<CarCommentDto>).items ?? []) as CarCommentDto[];
}

export async function getAvailableModelCards(): Promise<AvailableModelCard[]> {
  const availableModels = await getAvailableCarModels();
  const details = await Promise.all(
    availableModels.map(async (item) => {
      try {
        return [item.modelId, await getCarModelDetails(item.modelId)] as const;
      } catch {
        return [item.modelId, null] as const;
      }
    })
  );

  const detailsMap = new Map<number, CarModelDetailsDto | null>(details);

  return availableModels.map((item) => {
    const detail = detailsMap.get(item.modelId);
    return {
      ...item,
      imageUrl: detail?.images?.[0]?.imageUrl ?? null,
      description: detail?.description ?? null,
    };
  });
}

export async function getModelDetailsPayload(modelId: number): Promise<ModelDetailsPayload> {
  const [model, availableModels, partnerCars] = await Promise.all([
    getCarModelDetails(modelId),
    getAvailableCarModels(),
    getPartnerCarsByModel(modelId),
  ]);

  const availability = availableModels.find((item) => item.modelId === modelId) ?? null;

  const reviewsByPartnerCar = await Promise.all(
    partnerCars.map(async (partnerCar) => {
      const comments = await getPartnerCarComments(partnerCar.id);
      const carrierName = await resolveCarrierName(partnerCar.partnerId);
      return comments.map((comment) => ({
        ...comment,
        carrierPartnerId: partnerCar.partnerId,
        carrierName,
      }));
    })
  );

  const reviews = reviewsByPartnerCar
    .flat()
    .sort((left, right) => {
      const leftTime = new Date(left.createdOn).getTime();
      const rightTime = new Date(right.createdOn).getTime();
      return rightTime - leftTime;
    });

  return {
    model,
    availability,
    reviews,
    minPriceHour: availability?.minPriceHour ?? getMinPrice(partnerCars),
    maxPriceHour: availability?.maxPriceHour ?? getMaxPrice(partnerCars),
  };
}

export async function matchCarByModel(
  payload: MatchCarByModelPayload
): Promise<MatchCarByModelResult> {
  const response = await api.post("/cars/match", payload);
  return toCamelCase(response.data) as MatchCarByModelResult;
}
