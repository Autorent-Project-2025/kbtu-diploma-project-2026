import type { Booking, ComputedBookingStatus } from "../types/Booking";

/**
 * Вычисляет реальный статус бронирования на основе дат и статуса из БД
 */
export function computeBookingStatus(booking: Booking): ComputedBookingStatus {
  const now = new Date();
  const startDate = new Date(booking.startDate);
  const endDate = new Date(booking.endDate);

  if (booking.status === "pending") {
    return "paymentPending";
  }

  // Если отменено - всегда отменено
  if (booking.status === "canceled") {
    return "canceled";
  }

  // Если дата окончания прошла - завершено
  if (endDate < now) {
    return "completed";
  }

  // Если дата начала еще не наступила - предстоящее
  if (startDate > now) {
    return "upcoming";
  }

  // Если текущий момент между началом и концом - активное
  if (startDate <= now && endDate >= now) {
    return "active";
  }

  return "upcoming";
}

/**
 * Проверяет, можно ли отменить бронирование
 */
export function canCancelBooking(booking: Booking): boolean {
  const status = computeBookingStatus(booking);

  // Можно отменить только предстоящие или активные бронирования
  return status === "paymentPending" || status === "upcoming" || status === "active";
}

/**
 * Проверяет, доступен ли автомобиль для бронирования в указанный период
 */
export function isCarAvailable(
  carBookings: Booking[],
  startDate: Date,
  endDate: Date
): boolean {
  const activeBookings = carBookings.filter((booking) => {
    const status = computeBookingStatus(booking);
    // Учитываем только предстоящие и активные бронирования
    return status === "paymentPending" || status === "upcoming" || status === "active";
  });

  // Проверяем пересечение дат
  for (const booking of activeBookings) {
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
