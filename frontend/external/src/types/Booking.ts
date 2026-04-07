export interface BookingPricingBreakdown {
  quotedAtUtc: string;
  marketValueKzt: number;
  rating: number;
  currentAvailableCarsCount: number;
  daysBeforeBooking: number;
  billableHours: number;
  ratingCoefficient: number;
  advanceBookingCoefficient: number;
  availabilityCoefficient: number;
  quotedPriceHour: number;
  quotedTotalPrice: number;
  currency: string;
  isMarketValueStale: boolean;
}

export interface Booking {
  id: number;
  carId: number;
  partnerUserId?: string;
  carBrand: string;
  carModel: string;
  partnerName?: string | null;
  coverImageUrl?: string | null;
  imageUrls?: string[];
  startDate: string;
  endDate: string;
  price: number | null;
  priceHour?: number | null;
  tripStartedAt?: string | null;
  tripCompletedAt?: string | null;
  completionReviewTicketId?: string | null;
  usedSubscription?: boolean;
  pricingBreakdown?: BookingPricingBreakdown | null;
  status: BookingStatus;
}

export interface BookingCharge {
  id: number;
  bookingId: number;
  chargeType: "LatePenalty" | "DamageFine" | string;
  amount: number;
  partnerShareAmount: number;
  currency: string;
  status: "Pending" | "Paid" | "Canceled" | string;
  description?: string | null;
  createdAt: string;
  updatedAt: string;
  paidAt?: string | null;
  canceledAt?: string | null;
}

export interface BookingCompletionSubmissionResult {
  booking: Booking;
  reviewTicketId: string;
  latePenaltyAmount: number;
}

export type BookingStatus =
  | "pending" //
  | "confirmed" //
  | "active" //
  | "awaitingReview" //
  | "completed" //
  | "canceled"; //

export interface BookingWithCarStatus extends Booking {
  computedStatus: ComputedBookingStatus;
}

export type ComputedBookingStatus =
  | "paymentPending" // Бронь создана, но ожидает оплаты
  | "upcoming" // Предстоящая (еще не началась)
  | "active" // Активная (идет сейчас)
  | "awaitingReview" // Поездка завершена, ожидает проверки
  | "completed" // Завершенная (прошла)
  | "canceled"; // Отмененная

export type BookingPaymentState =
  | "not_started"
  | "started"
  | "succeeded"
  | "failed"
  | "expired"
  | "canceled";

export interface BookingPaymentStatus {
  bookingId: number;
  bookingStatus: BookingStatus;
  paymentStatus: BookingPaymentState;
  paymentAttemptId?: number | null;
  sessionKey?: string | null;
  amount?: number | null;
  currency: string;
  cardHolder?: string | null;
  cardLast4?: string | null;
  failureReason?: string | null;
  bookingCreatedAt: string;
  bookingExpiresAt?: string | null;
  paymentCreatedAt?: string | null;
  paymentUpdatedAt?: string | null;
  paymentCompletedAt?: string | null;
  paymentExpiresAt?: string | null;
  requiresInput: boolean;
  canRetry: boolean;
}

export interface SubmitBookingPaymentPayload {
  sessionKey: string;
  cardHolder: string;
  cardNumber: string;
  expiryMonth: number;
  expiryYear: number;
  cvv: string;
}
