import api from "./axios";
import { resolveAssetUrl } from "../utils/resolveAssetUrl";

export interface PartnerCarSummary {
  id: number;
  modelDisplayName: string;
  rating?: number | null;
  bookingCount: number;
  licensePlate: string;
  ownershipFileName?: string | null;
  priceHour?: number | null;
  priceDay?: number | null;
  color?: string | null;
}

export interface PartnerCarImage {
  id: number;
  imageId?: string | null;
  imageUrl: string;
  imageType: number;
  displayOrder: number;
}

export interface PartnerCarComment {
  id: number;
  userId: string;
  userName: string;
  carId: number;
  bookingId?: number | null;
  partnerCarId?: number | null;
  avatarUrl?: string | null;
  content: string;
  rating: number;
  createdOn: string;
}

export interface PartnerCarDetails {
  id: number;
  partnerUserId: string;
  licensePlate: string;
  ownershipFileName?: string | null;
  color?: string | null;
  priceHour?: number | null;
  priceDay?: number | null;
  status: number;
  isActive: boolean;
  createdAt: string;
  rating?: number | null;
  ratingsCount: number;
  modelId: number;
  brand: string;
  model: string;
  year: number;
  engine?: string | null;
  transmission?: string | null;
  seats?: number | null;
  fuelType?: string | null;
  doors?: number | null;
  bodyType?: string | null;
  horsepower?: number | null;
  description?: string | null;
  images: PartnerCarImage[];
  comments: PartnerCarComment[];
  bookings: Array<{
    id: number;
    carId: number;
    userId: string;
    startDate: string;
    endDate: string;
    price?: number | null;
    status?: string | null;
  }>;
}

export interface PublicPartnerCarDetails {
  id: number;
  partnerUserId: string;
  licensePlate: string;
  carModelId: number;
  color?: string | null;
  priceHour?: number | null;
  priceDay?: number | null;
  status: number;
  createdAt: string;
  rating?: number | null;
  ratingsCount: number;
  modelBrand: string;
  modelName: string;
  modelYear: number;
  commercialBadgeKeys: string[];
  images: PartnerCarImage[];
  comments?: PartnerCarComment[];
  bookings?: Array<{
    id: number;
    carId: number;
    userId: string;
    startDate: string;
    endDate: string;
    price?: number | null;
    status?: string | null;
  }>;
}

export async function getMyPartnerCars(): Promise<PartnerCarSummary[]> {
  const response = await api.get("/cars/my");
  return (response.data ?? []) as PartnerCarSummary[];
}

export async function getMyPartnerCarDetails(carId: number): Promise<PartnerCarDetails> {
  const response = await api.get(`/cars/my/${carId}`);
  const payload = response.data as PartnerCarDetails;

  return {
    ...payload,
    images: (payload.images ?? []).map((image) => ({
      ...image,
      imageUrl: resolveAssetUrl(image.imageUrl) ?? image.imageUrl,
    })),
    comments: (payload.comments ?? []).map((comment) => ({
      ...comment,
      avatarUrl: resolveAssetUrl(comment.avatarUrl ?? null) ?? comment.avatarUrl,
    })),
    bookings: payload.bookings ?? [],
  };
}

export async function getPublicPartnerCarDetails(
  partnerCarId: number,
): Promise<PublicPartnerCarDetails> {
  const response = await api.get(`/cars/partner-cars/${partnerCarId}`);
  const payload = response.data as PublicPartnerCarDetails;

  return {
    ...payload,
    images: (payload.images ?? []).map((image) => ({
      ...image,
      imageUrl: resolveAssetUrl(image.imageUrl) ?? image.imageUrl,
    })),
  };
}

export async function getCarBrands(): Promise<string[]> {
  const response = await api.get("/cars/catalog/brands");
  return (response.data ?? []) as string[];
}

export async function deletePartnerCarImage(imageId: number): Promise<void> {
  await api.delete(`/cars/images/partner-cars/${imageId}`);
}

export async function getCarModelNames(brand?: string | null): Promise<string[]> {
  const response = await api.get("/cars/catalog/models", {
    params: {
      brand: brand?.trim() || undefined,
    },
  });
  return (response.data ?? []) as string[];
}
