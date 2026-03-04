import api from "./axios";

export interface CarModelOption {
  id: number;
  brand: string;
  model: string;
  year: number;
}

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

export interface PartnerCarDetails {
  id: number;
  partnerId: string;
  licensePlate: string;
  ownershipFileName?: string | null;
  color?: string | null;
  priceHour?: number | null;
  priceDay?: number | null;
  status: number;
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
  description?: string | null;
  images: PartnerCarImage[];
}

interface CarModelsResponse {
  items: CarModelOption[];
  totalCount: number;
  page: number;
  pageSize: number;
}

export async function getMyPartnerCars(): Promise<PartnerCarSummary[]> {
  const response = await api.get("/cars/my");
  return (response.data ?? []) as PartnerCarSummary[];
}

export async function getMyPartnerCarDetails(carId: number): Promise<PartnerCarDetails> {
  const response = await api.get(`/cars/my/${carId}`);
  return response.data as PartnerCarDetails;
}

export async function getCarModels(): Promise<CarModelOption[]> {
  const response = await api.get("/cars/models", {
    params: {
      page: 1,
      pageSize: 300,
    },
  });

  const payload = response.data as CarModelsResponse;
  return payload.items ?? [];
}
