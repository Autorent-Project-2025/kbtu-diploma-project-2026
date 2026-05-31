// Pure label maps, badge classes and ticket type-guards for the tickets views.

import type { PartnerCarTicketData, Ticket } from "../types/Ticket";

export function statusLabel(status: number): string {
  if (status === 1) return "На рассмотрении";
  if (status === 2) return "Одобрена";
  if (status === 3) return "Отклонена";
  if (status === 4) return "Выставлен штраф";
  return "Неизвестно";
}

export function ticketTypeLabel(ticketType: number): string {
  if (ticketType === 2) return "Партнёр";
  if (ticketType === 3) return "Авто партнёра";
  if (ticketType === 4) return "Завершение поездки";
  if (ticketType === 5) return "Отмена бронирования";
  return "Клиент";
}

export function getTicketTypeBadgeClass(ticketType: number): string {
  if (ticketType === 2)
    return "bg-blue-100 text-blue-800 dark:bg-blue-900/30 dark:text-blue-300";
  if (ticketType === 3)
    return "bg-violet-100 text-violet-800 dark:bg-violet-900/30 dark:text-violet-300";
  if (ticketType === 4)
    return "bg-amber-100 text-amber-800 dark:bg-amber-900/30 dark:text-amber-300";
  if (ticketType === 5)
    return "bg-rose-100 text-rose-800 dark:bg-rose-900/30 dark:text-rose-300";
  return "bg-emerald-100 text-emerald-800 dark:bg-emerald-900/30 dark:text-emerald-300";
}

export function isClientTicket(ticket: Ticket): boolean {
  return ticket.ticketType === 1;
}
export function isPartnerTicket(ticket: Ticket): boolean {
  return ticket.ticketType === 2;
}
export function isPartnerCarTicket(ticket: Ticket): boolean {
  return ticket.ticketType === 3;
}
export function isBookingCompletionTicket(
  ticket: Ticket | null | undefined,
): boolean {
  return ticket?.ticketType === 4;
}
export function isPartnerBookingCancellationTicket(
  ticket: Ticket | null | undefined,
): boolean {
  return ticket?.ticketType === 5;
}

export function completionPhotoLabel(slot: string): string {
  if (slot === "front") return "спереди";
  if (slot === "back") return "сзади";
  if (slot === "side_left") return "сбоку слева";
  if (slot === "side_right") return "сбоку справа";
  if (slot === "interior") return "из салона";
  return slot;
}

export function partnerCarImageTypeLabel(
  imageType?: string | null,
  index?: number,
): string {
  if (imageType === "front") return "Фото спереди";
  if (imageType === "back") return "Фото сзади";
  if (imageType === "side") return "Фото сбоку";
  if (imageType === "interior") return "Фото салона";
  if (imageType === "general") return "Общий вид";
  return `Фото ${(index ?? 0) + 1}`;
}

export function partnerCarRequestKindLabel(value?: string | null): string {
  const normalized = (value ?? "").trim().toLowerCase();
  if (normalized === "update") return "Изменение машины";
  return "Новая машина";
}

export function resolvePartnerCarRequestKind(
  ticket: Ticket | null | undefined,
): string {
  if (!ticket || !isPartnerCarTicket(ticket)) {
    return "create";
  }

  const data = ticket.data as PartnerCarTicketData | undefined;
  return ticket.partnerCarRequestKind ?? data?.requestKind ?? "create";
}

export function partnerCarStatusLabel(status?: number | null): string {
  if (status === 0) return "Доступна";
  if (status === 1) return "Забронирована";
  if (status === 2) return "В поездке";
  if (status === 3) return "На обслуживании";
  return "Не указано";
}

export function partnerBookingStatusLabel(status?: string | null): string {
  const normalized = (status ?? "").trim().toLowerCase();
  if (normalized === "pending") return "Ожидает оплаты";
  if (normalized === "confirmed") return "Подтверждено";
  if (normalized === "active") return "Активно";
  if (normalized === "awaitingreview") return "Ожидает проверки";
  if (normalized === "completed") return "Завершено";
  if (normalized === "canceled") return "Отменено";
  return status || "Неизвестно";
}

export function aiVerdictLabel(verdict?: string | null): string {
  switch (verdict) {
    case "ok":
      return "Повреждений не найдено";
    case "damages_found":
      return "AI нашёл повреждения";
    case "invalid_session":
      return "Сессия отклонена";
    default:
      return "Вердикт недоступен";
  }
}

export function aiStatusBadge(status?: string | null): {
  label: string;
  tone: "ok" | "warn" | "error" | "muted";
} {
  switch (status) {
    case "ok":
      return { label: "AI-анализ выполнен", tone: "ok" };
    case "invalid_session":
      return { label: "AI отклонил сессию", tone: "warn" };
    case "error":
      return { label: "Ошибка AI-анализа", tone: "error" };
    case "unavailable":
      return { label: "AI недоступен", tone: "muted" };
    default:
      return { label: "AI-анализ не выполнялся", tone: "muted" };
  }
}

export function formatConfidencePercent(confidence: number): string {
  if (!Number.isFinite(confidence)) return "—";
  return `${Math.round(confidence * 100)}%`;
}
