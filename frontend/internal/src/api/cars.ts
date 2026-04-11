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

export interface CarImageDto {
  id: number;
  imageUrl: string;
  isPrimary?: boolean;
  sortOrder?: number;
}

export interface CarCommentDto {
  id: number;
  userId: string;
  userName?: string;
  partnerCarId: number;
  bookingId?: number;
  rating: number;
  content: string;
  createdAt: string;
}

export interface PartnerCarsPagedResult {
  items: PartnerCarDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface PartnerCarsFilter {
  page?: number;
  pageSize?: number;
  status?: number;
  partnerUserId?: string;
  carModelId?: number;
  search?: string;
}

export interface PartnerCarUpdatePayload {
  licensePlate: string;
  color?: string;
  status: number;
}

export async function getPartnerCars(
  filter: PartnerCarsFilter = {},
): Promise<PartnerCarsPagedResult> {
  const { page = 1, pageSize = 20, status, partnerUserId, carModelId, search } = filter;
  const res = await api.get("/cars/partner-cars", {
    params: { page, pageSize, status, partnerUserId, carModelId, search },
  });
  return res.data as PartnerCarsPagedResult;
}

export async function getPartnerCar(id: number): Promise<PartnerCarDto> {
  const res = await api.get(`/cars/partner-cars/${id}`);
  return res.data as PartnerCarDto;
}

export async function getPartnerCarImages(partnerCarId: number): Promise<CarImageDto[]> {
  const res = await api.get(`/cars/images/partner-cars/${partnerCarId}`);
  return (res.data ?? []) as CarImageDto[];
}

export async function getPartnerCarComments(
  partnerCarId: number,
  page = 1,
  pageSize = 20,
): Promise<CarCommentDto[]> {
  const res = await api.get(`/cars/comments/partner-cars/${partnerCarId}`, {
    params: { page, pageSize },
  });
  const data = res.data;
  if (Array.isArray(data)) return data as CarCommentDto[];
  if (data?.items) return data.items as CarCommentDto[];
  return [];
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
