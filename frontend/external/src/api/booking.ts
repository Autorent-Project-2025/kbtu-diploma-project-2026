import api from "./axios";
import type {
  Booking,
  BookingPaymentState,
  BookingPaymentStatus,
  BookingStatus,
  SubmitBookingPaymentPayload,
} from "../types/Booking";
import type { PaginatedResponse } from "../types/Pagination";

export interface GetMyBookingsParams {
  page?: number;
  pageSize?: number;
}

interface BookingApiDto {
  id: number;
  partnerCarId: number;
  partnerUserId?: string;
  carBrand: string;
  carModel: string;
  startTime: string;
  endTime: string;
  priceHour?: number | null;
  totalPrice?: number | null;
  status?: string | null;
}

interface BookingPaymentStatusApiDto {
  bookingId: number;
  bookingStatus?: string | null;
  paymentStatus?: string | null;
  paymentAttemptId?: number | null;
  sessionKey?: string | null;
  amount?: number | null;
  currency?: string | null;
  cardHolder?: string | null;
  cardLast4?: string | null;
  failureReason?: string | null;
  bookingCreatedAt?: string | null;
  bookingExpiresAt?: string | null;
  paymentCreatedAt?: string | null;
  paymentUpdatedAt?: string | null;
  paymentCompletedAt?: string | null;
  paymentExpiresAt?: string | null;
  requiresInput?: boolean | null;
  canRetry?: boolean | null;
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
    partnerUserId: dto.partnerUserId,
    carBrand: dto.carBrand ?? "",
    carModel: dto.carModel ?? "",
    startDate: dto.startTime,
    endDate: dto.endTime,
    price: dto.totalPrice ?? null,
    priceHour: dto.priceHour ?? null,
    status: normalizeStatus(dto.status),
  };
}

function normalizePaymentStatus(
  value: string | null | undefined
): BookingPaymentState {
  const normalized = (value ?? "").trim().toLowerCase();
  if (normalized === "started") return "started";
  if (normalized === "succeeded") return "succeeded";
  if (normalized === "failed") return "failed";
  if (normalized === "expired") return "expired";
  if (normalized === "canceled") return "canceled";
  return "not_started";
}

function mapBookingPaymentStatus(
  dto: BookingPaymentStatusApiDto
): BookingPaymentStatus {
  return {
    bookingId: dto.bookingId,
    bookingStatus: normalizeStatus(dto.bookingStatus),
    paymentStatus: normalizePaymentStatus(dto.paymentStatus),
    paymentAttemptId: dto.paymentAttemptId ?? null,
    sessionKey: dto.sessionKey ?? null,
    amount: dto.amount ?? null,
    currency: dto.currency?.trim().toUpperCase() || "KZT",
    cardHolder: dto.cardHolder ?? null,
    cardLast4: dto.cardLast4 ?? null,
    failureReason: dto.failureReason ?? null,
    bookingCreatedAt: dto.bookingCreatedAt ?? new Date().toISOString(),
    bookingExpiresAt: dto.bookingExpiresAt ?? null,
    paymentCreatedAt: dto.paymentCreatedAt ?? null,
    paymentUpdatedAt: dto.paymentUpdatedAt ?? null,
    paymentCompletedAt: dto.paymentCompletedAt ?? null,
    paymentExpiresAt: dto.paymentExpiresAt ?? null,
    requiresInput: Boolean(dto.requiresInput),
    canRetry: Boolean(dto.canRetry),
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

/**
 * Создать новое бронирование.
 * partnerCarId - это id машины партнера.
 */
export async function createBooking(
  partnerCarId: number,
  start: string,
  end: string
): Promise<Booking> {
  const response = await api.post("/bookings", {
    partnerCarId,
    startTime: start,
    endTime: end,
  });

  return mapBooking(response.data as BookingApiDto);
}

export async function getBooking(bookingId: number): Promise<Booking> {
  const response = await api.get(`/bookings/${bookingId}`);
  return mapBooking(response.data as BookingApiDto);
}

export async function startBookingPayment(
  bookingId: number
): Promise<BookingPaymentStatus> {
  const response = await api.post(`/bookings/${bookingId}/payment/start`);
  return mapBookingPaymentStatus(response.data as BookingPaymentStatusApiDto);
}

export async function getBookingPaymentStatus(
  bookingId: number
): Promise<BookingPaymentStatus> {
  const response = await api.get(`/bookings/${bookingId}/payment/status`);
  return mapBookingPaymentStatus(response.data as BookingPaymentStatusApiDto);
}

export async function submitBookingPayment(
  bookingId: number,
  payload: SubmitBookingPaymentPayload
): Promise<BookingPaymentStatus> {
  const response = await api.post(`/bookings/${bookingId}/payment/submit`, payload);
  return mapBookingPaymentStatus(response.data as BookingPaymentStatusApiDto);
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
