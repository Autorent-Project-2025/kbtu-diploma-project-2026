import api from "./axios";

export interface PartnerCarDto {
  id: number;
  partnerUserId: string;
  carModelId: number;
  licensePlate: string;
  ownershipFileName?: string;
  color?: string;
  priceHour?: number;
  priceDay?: number;
  status: number;
  createdAt: string;
  rating?: number;
  ratingsCount: number;
  modelBrand: string;
  modelName: string;
  modelYear: number;
  commercialBadgeKeys: string[];
}

export interface PartnerCarsPagedResult {
  items: PartnerCarDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface PartnerCarUpdatePayload {
  licensePlate: string;
  color?: string;
  status: number;
}

export async function getPartnerCars(
  page = 1,
  pageSize = 20,
): Promise<PartnerCarsPagedResult> {
  const res = await api.get("/cars/partner-cars", {
    params: { page, pageSize },
  });
  return res.data as PartnerCarsPagedResult;
}

export async function getPartnerCar(id: number): Promise<PartnerCarDto> {
  const res = await api.get(`/cars/partner-cars/${id}`);
  return res.data as PartnerCarDto;
}

export async function updatePartnerCar(
  id: number,
  data: PartnerCarUpdatePayload,
): Promise<PartnerCarDto> {
  const res = await api.put(`/cars/partner-cars/${id}`, data);
  return res.data as PartnerCarDto;
}

export async function deletePartnerCar(id: number): Promise<void> {
  await api.delete(`/cars/partner-cars/${id}`);
}
