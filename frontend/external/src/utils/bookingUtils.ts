import type { Booking, ComputedBookingStatus } from "../types/Booking";

/**
 * Вычисляет реальный статус бронирования на основе дат и статуса из БД
 */
export function computeBookingStatus(booking: Booking): ComputedBookingStatus {
  switch (booking.status) {
    case "pending":
      return "paymentPending";
    case "confirmed":
      return "upcoming";
    case "active":
      return "active";
    case "awaitingReview":
      return "awaitingReview";
    case "completed":
      return "completed";
    case "canceled":
      return "canceled";
    default:
      return "upcoming";
  }
}

/**
 * Проверяет, можно ли отменить бронирование
 */
export function canCancelBooking(booking: Booking): boolean {
  const status = computeBookingStatus(booking);

  // На backend отмена разрешена только до фактического старта поездки.
  return status === "paymentPending" || status === "upcoming";
}

export function canStartTrip(booking: Booking, now = new Date()): boolean {
  if (booking.status !== "confirmed") {
    return false;
  }

  const startDate = new Date(booking.startDate);
  return now.getTime() >= startDate.getTime() - 15 * 60 * 1000;
}

export function canCompleteTrip(booking: Booking): boolean {
  return booking.status === "active" && Boolean(booking.tripStartedAt);
}

export function hasCompletionReviewDetails(booking: Booking): boolean {
  return (
    booking.status === "active" ||
    booking.status === "awaitingReview" ||
    booking.status === "completed"
  );
}

/**
 * Проверяет, доступен ли автомобиль для бронирования в указанный период
 */
export function isCarAvailable(
  carBookings: Booking[],
  startDate: Date,
  endDate: Date
): boolean {
  const blockingBookings = carBookings.filter(
    (booking) =>
      booking.status === "pending" ||
      booking.status === "confirmed" ||
      booking.status === "active"
  );

  // Проверяем пересечение дат
  for (const booking of blockingBookings) {
    const bookingStart = new Date(booking.startDate);
    const bookingEnd = new Date(booking.endDate);

    // Проверка пересечения периодов
    const overlaps =
      (startDate >= bookingStart && startDate < bookingEnd) ||
      (endDate > bookingStart && endDate <= bookingEnd) ||
      (startDate <= bookingStart && endDate >= bookingEnd);

    if (overlaps) {
      return false;
    }
  }

  return true;
}

/**
 * Форматирует дату для отображения
 */
export function formatBookingDate(dateString: string): string {
  if (!dateString) return "";
  const date = new Date(dateString);
  return new Intl.DateTimeFormat("ru-RU", {
    day: "2-digit",
    month: "short",
    year: "numeric",
    hour: "2-digit",
    minute: "2-digit",
  }).format(date);
}

/**
 * Вычисляет продолжительность бронирования
 */
export function getBookingDuration(startDate: string, endDate: string) {
  const start = new Date(startDate);
  const end = new Date(endDate);
  const diffMs = end.getTime() - start.getTime();

  if (diffMs <= 0) return null;

  const totalMinutes = Math.floor(diffMs / (1000 * 60));
  const days = Math.floor(totalMinutes / (60 * 24));
  const hours = Math.floor((totalMinutes % (60 * 24)) / 60);
  const minutes = totalMinutes % 60;

  return { days, hours, minutes, totalMinutes };
}

export function getTripDuration(
  tripStartedAt?: string | null,
  tripCompletedAt?: string | null,
  now = new Date()
) {
  if (!tripStartedAt) {
    return null;
  }

  const start = new Date(tripStartedAt);
  const end = tripCompletedAt ? new Date(tripCompletedAt) : now;
  const diffMs = end.getTime() - start.getTime();
  if (diffMs <= 0) {
    return null;
  }

  const totalMinutes = Math.floor(diffMs / (1000 * 60));
  const days = Math.floor(totalMinutes / (60 * 24));
  const hours = Math.floor((totalMinutes % (60 * 24)) / 60);
  const minutes = totalMinutes % 60;

  return { days, hours, minutes, totalMinutes };
}
