import api from "./axios";

export interface BookingDto {
  id: number;
  userId: string;
  partnerCarId: number;
  partnerUserId: string;
  carBrand: string;
  carModel: string;
  partnerName?: string;
  coverImageUrl?: string;
  startTime: string;
  endTime: string;
  priceHour?: number;
  totalPrice?: number;
  createdAt: string;
  tripStartedAt?: string;
  tripCompletedAt?: string;
  status?: string;
  usedSubscription: boolean;
}

export interface BookingsPagedResult {
  items: BookingDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export async function getAllBookings(
  page = 1,
  pageSize = 20,
  sortBy?: string,
  sortOrder?: "asc" | "desc",
): Promise<BookingsPagedResult> {
  const res = await api.get("/bookings/all", {
    params: { page, pageSize, sortBy, sortOrder },
  });
  return res.data as BookingsPagedResult;
}

export async function getBooking(id: number): Promise<BookingDto> {
  const res = await api.get(`/bookings/all/${id}`);
  return res.data as BookingDto;
}

export async function cancelBooking(id: number): Promise<void> {
  await api.post(`/bookings/all/${id}/cancel`);
}
