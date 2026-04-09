import { config } from "../config/env";

type AvailableModel = {
  modelId: number;
  brand: string;
  model: string;
  year: number;
  availableCarsCount: number;
};

type CarModelDetails = {
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
  features?: Array<{ name: string }>;
  images?: Array<{ imageUrl: string }>;
};

type PartnerCarSummary = {
  id: number;
  partnerUserId: string;
  carModelId: number;
  priceHour?: number | null;
  priceDay?: number | null;
  status: number;
  rating?: number | null;
  ratingsCount: number;
  modelBrand: string;
  modelName: string;
  modelYear: number;
};

type PartnerCarDetails = PartnerCarSummary & {
  color?: string | null;
  images?: Array<{ imageUrl: string }>;
  comments?: Array<{ content: string; rating: number }>;
};

type PartnerPublicProfile = {
  relatedUserId: string;
  carrierName: string;
};

type BookingAvailability = {
  available: boolean;
};

async function getJson<T>(baseUrl: string, path: string): Promise<T> {
  const response = await fetch(`${baseUrl.replace(/\/$/, "")}${path}`);
  if (!response.ok) {
    throw new Error(`Request to ${path} failed with status ${response.status}.`);
  }

  return (await response.json()) as T;
}

export async function getAvailableModels(): Promise<AvailableModel[]> {
  return getJson<AvailableModel[]>(config.carServiceBaseUrl, "/available-models");
}

export async function getCarModelDetails(modelId: number): Promise<CarModelDetails> {
  return getJson<CarModelDetails>(config.carServiceBaseUrl, `/models/${modelId}`);
}

export async function getAvailablePartnerCarsByModel(modelId: number): Promise<PartnerCarSummary[]> {
  const payload = await getJson<{ items?: PartnerCarSummary[] }>(
    config.carServiceBaseUrl,
    `/partner-cars?carModelId=${modelId}&status=0&page=1&pageSize=200`,
  );

  return payload.items ?? [];
}

export async function getPartnerCarDetails(partnerCarId: number): Promise<PartnerCarDetails> {
  return getJson<PartnerCarDetails>(config.carServiceBaseUrl, `/partner-cars/${partnerCarId}`);
}

export async function getPartnerPublicProfile(
  relatedUserId: string,
): Promise<PartnerPublicProfile | null> {
  const response = await fetch(
    `${config.partnerServiceBaseUrl.replace(/\/$/, "")}/public/by-related-user/${encodeURIComponent(relatedUserId)}`,
  );

  if (response.status === 404) {
    return null;
  }

  if (!response.ok) {
    throw new Error(`Partner profile request failed with status ${response.status}.`);
  }

  return (await response.json()) as PartnerPublicProfile;
}

export async function isPartnerCarAvailableOnDates(
  partnerCarId: number,
  startTime: string,
  endTime: string,
): Promise<boolean> {
  const payload = await getJson<BookingAvailability>(
    config.bookingServiceBaseUrl,
    `/available?partnerCarId=${partnerCarId}&startTime=${encodeURIComponent(startTime)}&endTime=${encodeURIComponent(endTime)}`,
  );

  return Boolean(payload.available);
}
