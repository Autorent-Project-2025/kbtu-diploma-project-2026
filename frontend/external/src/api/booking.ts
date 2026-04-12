import api from "./axios";
import type {
  Booking,
  BookingCharge,
  BookingCarCommentSubmissionResult,
  BookingCompletionSubmissionResult,
  BookingPricingBreakdown,
  BookingPaymentState,
  BookingPaymentStatus,
  BookingStatus,
  SubmitBookingCarCommentPayload,
  SubmitBookingPaymentPayload,
} from "../types/Booking";
import type { PaginatedResponse } from "../types/Pagination";
import { resolveAssetUrl } from "../utils/resolveAssetUrl";

export interface GetMyBookingsParams {
  page?: number;
  pageSize?: number;
}

export interface BookingPricePreview {
  partnerCarId: number;
  marketValueKzt: number;
  rating: number;
  currentAvailableCarsCount: number;
  daysBeforeBooking: number;
  billableHours: number;
  ratingCoefficient: number;
  advanceBookingCoefficient: number;
  availabilityCoefficient: number;
  priceHour: number;
  finalPrice: number;
  currency: string;
  isMarketValueStale: boolean;
}

export interface PartnerCarAvailability {
  available: boolean;
}

interface BookingApiDto {
  id: number;
  partnerCarId: number;
  partnerUserId?: string;
  carBrand: string;
  carModel: string;
  partnerName?: string | null;
  coverImageUrl?: string | null;
  imageUrls?: string[] | null;
  startTime: string;
  endTime: string;
  priceHour?: number | null;
  totalPrice?: number | null;
  tripStartedAt?: string | null;
  tripCompletedAt?: string | null;
  completionReviewTicketId?: string | null;
  carCommentId?: number | null;
  carCommentSubmittedAt?: string | null;
  canLeaveComment?: boolean | null;
  pricingBreakdown?: BookingPricingBreakdown | null;
  cancellationActor?: string | null;
  cancellationReason?: string | null;
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

interface BookingChargeApiDto {
  id: number;
  bookingId: number;
  chargeType?: string | null;
  amount?: number | null;
  partnerShareAmount?: number | null;
  currency?: string | null;
  status?: string | null;
  description?: string | null;
  createdAt?: string | null;
  updatedAt?: string | null;
  paidAt?: string | null;
  canceledAt?: string | null;
}

interface BookingCompletionSubmissionApiDto {
  booking: BookingApiDto;
  reviewTicketId: string;
  latePenaltyAmount?: number | null;
}

interface BookingCarCommentSubmissionApiDto {
  booking: BookingApiDto;
  commentId: number;
  submittedAt: string;
}

function normalizeStatus(value: string | null | undefined): BookingStatus {
  const normalized = (value ?? "").trim().toLowerCase();
  if (normalized === "pending") return "pending";
  if (normalized === "confirmed") return "confirmed";
  if (normalized === "active") return "active";
  if (
    normalized === "awaitingreview" ||
    normalized === "awaiting_review" ||
    normalized === "awaitingreview"
  ) {
    return "awaitingReview";
  }
  if (normalized === "completed") return "completed";
  return "canceled";
}

function mapBooking(dto: BookingApiDto): Booking {
  const normalizedImageUrls = (dto.imageUrls ?? [])
    .map((imageUrl) => resolveAssetUrl(imageUrl) ?? imageUrl)
    .filter((imageUrl): imageUrl is string => Boolean(imageUrl));
  const resolvedCoverImageUrl = resolveAssetUrl(dto.coverImageUrl ?? null);

  return {
    id: dto.id,
    carId: dto.partnerCarId,
    partnerUserId: dto.partnerUserId,
    carBrand: dto.carBrand ?? "",
    carModel: dto.carModel ?? "",
    partnerName: dto.partnerName?.trim() || null,
    coverImageUrl:
      resolvedCoverImageUrl ??
      normalizedImageUrls[0] ??
      null,
    imageUrls: normalizedImageUrls,
    startDate: dto.startTime,
    endDate: dto.endTime,
    price: dto.totalPrice ?? null,
    priceHour: dto.priceHour ?? null,
    tripStartedAt: dto.tripStartedAt ?? null,
    tripCompletedAt: dto.tripCompletedAt ?? null,
    completionReviewTicketId: dto.completionReviewTicketId ?? null,
    carCommentId: dto.carCommentId ?? null,
    carCommentSubmittedAt: dto.carCommentSubmittedAt ?? null,
    canLeaveComment: Boolean(dto.canLeaveComment),
    pricingBreakdown: dto.pricingBreakdown ?? null,
    cancellationActor: dto.cancellationActor?.trim() || null,
    cancellationReason: dto.cancellationReason?.trim() || null,
    status: normalizeStatus(dto.status),
  };
}

function normalizePaymentStatus(
  value: string | null | undefined,
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
  dto: BookingPaymentStatusApiDto,
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

function mapBookingCharge(dto: BookingChargeApiDto): BookingCharge {
  return {
    id: dto.id,
    bookingId: dto.bookingId,
    chargeType: dto.chargeType?.trim() || "Unknown",
    amount: dto.amount ?? 0,
    partnerShareAmount: dto.partnerShareAmount ?? 0,
    currency: dto.currency?.trim().toUpperCase() || "KZT",
    status: dto.status?.trim() || "Pending",
    description: dto.description ?? null,
    createdAt: dto.createdAt ?? new Date().toISOString(),
    updatedAt: dto.updatedAt ?? dto.createdAt ?? new Date().toISOString(),
    paidAt: dto.paidAt ?? null,
    canceledAt: dto.canceledAt ?? null,
  };
}

function mapBookingCompletionSubmission(
  dto: BookingCompletionSubmissionApiDto,
): BookingCompletionSubmissionResult {
  return {
    booking: mapBooking(dto.booking),
    reviewTicketId: dto.reviewTicketId,
    latePenaltyAmount: dto.latePenaltyAmount ?? 0,
  };
}

function mapBookingCarCommentSubmission(
  dto: BookingCarCommentSubmissionApiDto,
): BookingCarCommentSubmissionResult {
  return {
    booking: mapBooking(dto.booking),
    commentId: dto.commentId,
    submittedAt: dto.submittedAt,
  };
}

/**
 * Получить бронирования текущего пользователя с пагинацией.
 */
export async function getMyBookings(
  params?: GetMyBookingsParams,
): Promise<PaginatedResponse<Booking> | Booking[]> {
  const queryParams = new URLSearchParams();
  if (params?.page) queryParams.append("page", params.page.toString());
  if (params?.pageSize)
    queryParams.append("pageSize", params.pageSize.toString());

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
        Math.ceil(
          (payload.totalCount ?? mappedItems.length) / (payload.pageSize ?? 1),
        ),
    };
  }

  const list = (
    Array.isArray(response.data) ? response.data : []
  ) as BookingApiDto[];
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
  end: string,
): Promise<Booking> {
  const response = await api.post("/bookings", {
    partnerCarId,
    startTime: start,
    endTime: end,
  });

  return mapBooking(response.data as BookingApiDto);
}

export async function getBookingPricePreview(
  partnerCarId: number,
  startTime: string,
  endTime: string,
): Promise<BookingPricePreview> {
  const response = await api.get("/bookings/price-preview", {
    params: {
      partnerCarId,
      startTime,
      endTime,
    },
  });

  return response.data as BookingPricePreview;
}

export async function getPartnerCarAvailability(
  partnerCarId: number,
  startTime: string,
  endTime: string,
): Promise<PartnerCarAvailability> {
  const response = await api.get("/bookings/available", {
    params: {
      partnerCarId,
      startTime,
      endTime,
    },
  });

  return response.data as PartnerCarAvailability;
}

export async function getBooking(bookingId: number): Promise<Booking> {
  const response = await api.get(`/bookings/${bookingId}`);
  return mapBooking(response.data as BookingApiDto);
}

export async function startBookingTrip(bookingId: number): Promise<void> {
  await api.post(`/bookings/${bookingId}/start`);
}

export async function completeBookingTrip(bookingId: number): Promise<void> {
  await api.post(`/bookings/${bookingId}/complete`);
}

export async function submitBookingCompletionReview(
  bookingId: number,
  files: {
    completionFrontPhotoFile: File;
    completionBackPhotoFile: File;
    completionSideLeftPhotoFile: File;
    completionSideRightPhotoFile: File;
    completionInteriorPhotoFile: File;
  },
): Promise<BookingCompletionSubmissionResult> {
  const formData = new FormData();
  formData.append("completionFrontPhotoFile", files.completionFrontPhotoFile);
  formData.append("completionBackPhotoFile", files.completionBackPhotoFile);
  formData.append(
    "completionSideLeftPhotoFile",
    files.completionSideLeftPhotoFile,
  );
  formData.append(
    "completionSideRightPhotoFile",
    files.completionSideRightPhotoFile,
  );
  formData.append(
    "completionInteriorPhotoFile",
    files.completionInteriorPhotoFile,
  );

  const response = await api.post(
    `/bookings/${bookingId}/complete-review`,
    formData,
    {
      headers: {
        "Content-Type": "multipart/form-data",
      },
    },
  );

  return mapBookingCompletionSubmission(
    response.data as BookingCompletionSubmissionApiDto,
  );
}

export async function submitBookingCarComment(
  bookingId: number,
  payload: SubmitBookingCarCommentPayload,
): Promise<BookingCarCommentSubmissionResult> {
  const response = await api.post(`/bookings/${bookingId}/car-comment`, payload);
  return mapBookingCarCommentSubmission(
    response.data as BookingCarCommentSubmissionApiDto,
  );
}

export async function getBookingCharges(
  bookingId: number,
): Promise<BookingCharge[]> {
  const response = await api.get(`/bookings/${bookingId}/charges`);
  const payload = Array.isArray(response.data) ? response.data : [];
  return payload.map((item) => mapBookingCharge(item as BookingChargeApiDto));
}

export async function payBookingCharge(
  bookingId: number,
  chargeId: number,
): Promise<BookingCharge> {
  const response = await api.post(
    `/bookings/${bookingId}/charges/${chargeId}/pay`,
  );
  return mapBookingCharge(response.data as BookingChargeApiDto);
}

export async function startBookingPayment(
  bookingId: number,
): Promise<BookingPaymentStatus> {
  const response = await api.post(`/bookings/${bookingId}/payment/start`);
  return mapBookingPaymentStatus(response.data as BookingPaymentStatusApiDto);
}

export async function getBookingPaymentStatus(
  bookingId: number,
): Promise<BookingPaymentStatus> {
  const response = await api.get(`/bookings/${bookingId}/payment/status`);
  return mapBookingPaymentStatus(response.data as BookingPaymentStatusApiDto);
}

export async function submitBookingPayment(
  bookingId: number,
  payload: SubmitBookingPaymentPayload,
): Promise<BookingPaymentStatus> {
  const response = await api.post(
    `/bookings/${bookingId}/payment/submit`,
    payload,
  );
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
