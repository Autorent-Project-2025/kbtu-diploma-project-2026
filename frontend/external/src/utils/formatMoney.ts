export function formatMoney(amount: number | null | undefined, currency = "KZT"): string {
  if (amount == null) {
    return "—";
  }

  return new Intl.NumberFormat("ru-RU", {
    style: "currency",
    currency,
    minimumFractionDigits: 0,
    maximumFractionDigits: 2,
  }).format(amount);
}

export function formatPricePerHour(amount: number | null | undefined, currency = "KZT"): string {
  if (amount == null) {
    return "по запросу";
  }

  return `${formatMoney(amount, currency)}/час`;
}
