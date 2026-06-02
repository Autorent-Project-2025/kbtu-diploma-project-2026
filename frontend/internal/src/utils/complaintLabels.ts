// Pure label maps and badge-class helpers for complaint views.

export const categoryLabels: Record<number, string> = {
  1: "Состояние авто",
  2: "Задержка передачи",
  3: "Качество сервиса",
  4: "Безопасность",
  5: "Поведение клиента",
  99: "Другое",
};

export const statusLabels: Record<number, string> = {
  1: "Новая",
  2: "На рассмотрении",
  3: "Ожидает ответа",
  4: "Решена",
  5: "Отклонена",
};

export const priorityLabels: Record<number, string> = {
  1: "Обычный",
  2: "Высокий",
  3: "Срочный",
};

export const reporterLabels: Record<number, string> = {
  1: "Клиент",
  2: "Партнёр",
};

export const targetLabels: Record<number, string> = {
  1: "Партнёр",
  2: "Клиент",
};

export const resolutionLabels: Record<number, string> = {
  1: "В пользу заявителя",
  2: "В пользу контрагента",
  3: "Компромисс",
  4: "Действий не требуется",
};

export const reopenStatusLabels: Record<number, string> = {
  1: "Ожидает",
  2: "Одобрен",
  3: "Отклонён",
};

export const chargeTypeLabels: Record<string, string> = {
  LatePenalty: "Штраф за опоздание",
  DamageFine: "Штраф за повреждение",
};

export function complaintStatusBadge(status: number): string {
  const map: Record<number, string> = {
    1: "bg-blue-100 text-blue-700 dark:bg-blue-900/30 dark:text-blue-400",
    2: "bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400",
    3: "bg-orange-100 text-orange-700 dark:bg-orange-900/30 dark:text-orange-400",
    4: "bg-emerald-100 text-emerald-700 dark:bg-emerald-900/30 dark:text-emerald-400",
    5: "bg-red-100 text-red-600 dark:bg-red-900/30 dark:text-red-400",
  };
  return map[status] ?? "bg-gray-100 text-gray-500";
}

export function priorityBadge(priority: number): string {
  const map: Record<number, string> = {
    1: "bg-gray-100 text-gray-600 dark:bg-gray-800 dark:text-gray-400",
    2: "bg-yellow-100 text-yellow-700 dark:bg-yellow-900/30 dark:text-yellow-400",
    3: "bg-red-100 text-red-600 dark:bg-red-900/30 dark:text-red-400",
  };
  return map[priority] ?? "bg-gray-100 text-gray-500";
}

export function chargeStatusLabel(status: string): string {
  const map: Record<string, string> = {
    Pending: "Ожидает",
    Paid: "Оплачен",
    Canceled: "Отменён",
    Refunded: "Возвращён",
  };
  return map[status] ?? status;
}

export function chargeStatusClass(status: string): string {
  const base = "px-2 py-0.5 rounded-full text-xs font-bold";
  const map: Record<string, string> = {
    Pending: `${base} bg-amber-100 text-amber-700 dark:bg-amber-900/30 dark:text-amber-400`,
    Paid: `${base} bg-green-100 text-green-700 dark:bg-green-900/30 dark:text-green-400`,
    Canceled: `${base} bg-gray-100 text-gray-500 dark:bg-gray-800 dark:text-gray-400`,
    Refunded: `${base} bg-rose-100 text-rose-700 dark:bg-rose-900/30 dark:text-rose-400`,
  };
  return map[status] ?? `${base} bg-gray-100 text-gray-500`;
}
