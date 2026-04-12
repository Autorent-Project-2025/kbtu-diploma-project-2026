export interface StatusStyle {
  label: string;
  css: string;
}

/* ── Booking statuses ──────────────────────────────────────── */

export const bookingStatusMap: Record<string, StatusStyle> = {
  Pending: { label: "Ожидание", css: "bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400" },
  PaymentPending: { label: "Оплата", css: "bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-400" },
  Confirmed: { label: "Подтверждено", css: "bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400" },
  Active: { label: "Активно", css: "bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400" },
  Completed: { label: "Завершено", css: "bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400" },
  CompletionReviewPending: { label: "Ревью", css: "bg-purple-100 text-purple-700 dark:bg-purple-900/30 dark:text-purple-400" },
  Cancelled: { label: "Отменено", css: "bg-red-100 text-red-600 dark:bg-red-900/30 dark:text-red-400" },
  Expired: { label: "Истекло", css: "bg-gray-100 text-gray-500 dark:bg-gray-800 dark:text-gray-500" },
};

export function bookingStatusLabel(status?: string | null): string {
  return (status && bookingStatusMap[status]?.label) ?? status ?? "—";
}

export function bookingStatusBadge(status?: string | null): string {
  return (status && bookingStatusMap[status]?.css) ?? "bg-gray-100 text-gray-500";
}

/* ── Car statuses ──────────────────────────────────────────── */

export const carStatusMap: Record<number, StatusStyle> = {
  0: { label: "На модерации", css: "bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400" },
  1: { label: "Активна", css: "bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400" },
  2: { label: "Неактивна", css: "bg-gray-100 text-gray-500 dark:bg-gray-800 dark:text-gray-500" },
  3: { label: "Заблокирована", css: "bg-red-100 text-red-600 dark:bg-red-900/30 dark:text-red-400" },
};

export function carStatusLabel(status: number): string {
  return carStatusMap[status]?.label ?? `Статус ${status}`;
}

export function carStatusBadge(status: number): string {
  return carStatusMap[status]?.css ?? "bg-gray-100 text-gray-500";
}

/* ── Ticket statuses ───────────────────────────────────────── */

export const ticketStatusMap: Record<number, StatusStyle> = {
  0: { label: "На рассмотрении", css: "bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400" },
  1: { label: "Одобрено", css: "bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400" },
  2: { label: "Отклонено", css: "bg-red-100 text-red-600 dark:bg-red-900/30 dark:text-red-400" },
};

export function ticketStatusLabel(status: number): string {
  return ticketStatusMap[status]?.label ?? `Статус ${status}`;
}

export function ticketStatusBadge(status: number): string {
  return ticketStatusMap[status]?.css ?? "bg-gray-100 text-gray-500";
}

/* ── Charge statuses ───────────────────────────────────────── */

export const chargeStatusMap: Record<string, StatusStyle> = {
  Pending: { label: "Ожидание", css: "bg-yellow-100 text-yellow-700" },
  Paid: { label: "Оплачено", css: "bg-emerald-100 text-emerald-700" },
  Cancelled: { label: "Отменено", css: "bg-gray-100 text-gray-500" },
};

/* ── Payout statuses ───────────────────────────────────────── */

export const payoutStatusMap: Record<string, StatusStyle> = {
  Pending: { label: "Ожидание", css: "bg-yellow-100 text-yellow-700" },
  Processing: { label: "В обработке", css: "bg-blue-100 text-blue-700" },
  Paid: { label: "Выплачено", css: "bg-emerald-100 text-emerald-700" },
  Failed: { label: "Ошибка", css: "bg-red-100 text-red-600" },
  Cancelled: { label: "Отменено", css: "bg-gray-100 text-gray-500" },
};
