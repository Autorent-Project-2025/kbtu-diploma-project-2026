import api from "./axios";
import type { Booking, BookingStatus } from "../types/Booking";
import type { PaginatedResponse } from "../types/Pagination";

export interface GetMyBookingsParams {
  page?: number;
  pageSize?: number;
}

interface BookingApiDto {
  id: number;
  partnerCarId: number;
  partnerId?: string;
  carBrand: string;
  carModel: string;
  startTime: string;
  endTime: string;
  priceHour?: number | null;
  totalPrice?: number | null;
  status?: string | null;
}

function normalizeStatus(value: string | null | undefined): BookingStatus {
  const normalized = (value ?? "").trim().toLowerCase();
  if (normalized === "pending") return "pending";
  if (normalized === "confirmed") return "confirmed";
  if (normalized === "active") return "active";
  if (normalized === "completed") return "completed";
  return "canceled";
}

function mapBooking(dto: BookingApiDto): Booking {
  return {
    id: dto.id,
    carId: dto.partnerCarId,
    partnerId: dto.partnerId,
    carBrand: dto.carBrand ?? "",
    carModel: dto.carModel ?? "",
    startDate: dto.startTime,
    endDate: dto.endTime,
    price: dto.totalPrice ?? null,
    priceHour: dto.priceHour ?? null,
    status: normalizeStatus(dto.status),
  };
}

/**
 * Получить бронирования текущего пользователя с пагинацией.
 */
export async function getMyBookings(
  params?: GetMyBookingsParams
): Promise<PaginatedResponse<Booking> | Booking[]> {
  const queryParams = new URLSearchParams();
  if (params?.page) queryParams.append("page", params.page.toString());
  if (params?.pageSize) queryParams.append("pageSize", params.pageSize.toString());

  const url = `/bookings/my${queryParams.toString() ? `?${queryParams.toString()}` : ""}`;
  const response = await api.get(url);

  if (!response.data) {
    return {
      items: [],
      totalCount: 0,
      page: params?.page || 1,
      pageSize: params?.pageSize || 10,
      totalPages: 0,
    };
  }

  if (response.data.items) {
    const payload = response.data as {
      items: BookingApiDto[];
      totalCount: number;
      page: number;
      pageSize: number;
      totalPages?: number;
    };

    const mappedItems = (payload.items ?? []).map(mapBooking);
    return {
      items: mappedItems,
      totalCount: payload.totalCount ?? mappedItems.length,
      page: payload.page ?? params?.page ?? 1,
      pageSize: payload.pageSize ?? params?.pageSize ?? mappedItems.length,
      totalPages:
        payload.totalPages ??
        Math.ceil((payload.totalCount ?? mappedItems.length) / (payload.pageSize ?? 1)),
    };
  }

  const list = (Array.isArray(response.data) ? response.data : []) as BookingApiDto[];
  const mapped = list.map(mapBooking);
  return {
    items: mapped,
    totalCount: mapped.length,
    page: 1,
    pageSize: mapped.length || 10,
    totalPages: mapped.length > 0 ? 1 : 0,
  };
}

export interface CreateBookingOptions {
  partnerId?: string;
  priceHour?: number | null;
}

/**
 * Создать новое бронирование.
 * partnerCarId - это id машины партнера.
 */
export async function createBooking(
  partnerCarId: number,
  start: string,
  end: string,
  options?: CreateBookingOptions
): Promise<Booking> {
  let partnerId = options?.partnerId;
  let priceHour = options?.priceHour;

  if (!partnerId || priceHour === undefined) {
    const carDetailsResponse = await api.get(`/cars/partner-cars/${partnerCarId}`);
    const carDetails = carDetailsResponse.data as {
      partnerId?: string;
      priceHour?: number | null;
    };

    partnerId = partnerId ?? carDetails.partnerId;
    if (priceHour === undefined) {
      priceHour = carDetails.priceHour ?? null;
    }
  }

  if (!partnerId) {
    throw new Error("Не удалось получить владельца машины для бронирования.");
  }

  const response = await api.post("/bookings", {
    partnerCarId,
    partnerId,
    priceHour: priceHour ?? null,
    startTime: start,
    endTime: end,
  });

  return mapBooking(response.data as BookingApiDto);
}

/**
 * Отменить бронирование.
 */
export async function cancelBooking(bookingId: number) {
  const response = await api.post(`/bookings/${bookingId}/cancel`);
  return response.data;
}

/**
 * Получить информацию о текущей доступности машины.
 */
export async function getCarBookings(carId: number): Promise<Booking[]> {
  try {
    const start = new Date();
    const end = new Date(start.getTime() + 1000);
    const response = await api.get("/bookings/available", {
      params: {
        partnerCarId: carId,
        startTime: start.toISOString(),
        endTime: end.toISOString(),
      },
    });

    if (response.data?.available === false) {
      return [
        {
          id: -carId,
          carId,
          carBrand: "",
          carModel: "",
          startDate: start.toISOString(),
          endDate: end.toISOString(),
          price: null,
          status: "active",
        },
      ];
    }

    return [];
  } catch (error) {
    console.error("Failed to fetch car availability:", error);
    return [];
  }
}
