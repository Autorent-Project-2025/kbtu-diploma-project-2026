export interface Booking {
  id: number;
  carId: number;
  partnerUserId?: string;
  carBrand: string;
  carModel: string;
  startDate: string;
  endDate: string;
  price: number | null;
  priceHour?: number | null;
  status: BookingStatus;
}

export type BookingStatus =
  | "pending" //
  | "confirmed" //
  | "active" //
  | "completed" //
  | "canceled"; //

export interface BookingWithCarStatus extends Booking {
  computedStatus: ComputedBookingStatus;
}

export type ComputedBookingStatus =
  | "paymentPending" // Бронь создана, но ожидает оплаты
  | "upcoming" // Предстоящая (еще не началась)
  | "active" // Активная (идет сейчас)
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
