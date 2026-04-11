import api from "./axios";

export interface BookingPricingBreakdown {
  quotedAtUtc?: string;
  marketValueKzt?: number;
  rating?: number;
  currentAvailableCarsCount?: number;
  daysBeforeBooking?: number;
  billableHours?: number;
  ratingCoefficient?: number;
  advanceBookingCoefficient?: number;
  availabilityCoefficient?: number;
  quotedPriceHour?: number;
  quotedTotalPrice?: number;
  currency?: string;
  isMarketValueStale?: boolean;
}

export interface BookingChargeDto {
  id: number;
  bookingId: number;
  chargeType: string;
  amount: number;
  partnerShareAmount?: number;
  currency: string;
  status: string;
  description?: string;
  createdAt: string;
  updatedAt?: string;
  paidAt?: string;
  canceledAt?: string;
}

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
  completionReviewTicketId?: string;
  carCommentId?: number;
  canLeaveComment?: boolean;
  status?: string;
  usedSubscription: boolean;
  pricingBreakdown?: BookingPricingBreakdown;
  imageUrls?: string[];
}

export interface BookingsPagedResult {
  items: BookingDto[];
  totalCount: number;
  page: number;
  pageSize: number;
  totalPages: number;
}

export interface BookingsFilter {
  page?: number;
  pageSize?: number;
  sortBy?: string;
  sortOrder?: "asc" | "desc";
  status?: string;
  userId?: string;
  partnerUserId?: string;
  partnerCarId?: number;
  search?: string;
}

export async function getAllBookings(
  filter: BookingsFilter = {},
): Promise<BookingsPagedResult> {
  const { page = 1, pageSize = 20, sortBy, sortOrder, status, userId, partnerUserId, partnerCarId, search } = filter;
  const res = await api.get("/bookings/all", {
    params: { page, pageSize, sortBy, sortOrder, status, userId, partnerUserId, partnerCarId, search },
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

export async function getBookingCharges(bookingId: number): Promise<BookingChargeDto[]> {
  const res = await api.get(`/bookings/all/${bookingId}/charges`);
  return (res.data ?? []) as BookingChargeDto[];
}
