export type CommercialBadgeKey = "deal" | "econom" | "premium";

export interface CommercialBadge {
  key: CommercialBadgeKey;
  label: string;
}

export interface CommercialBadgeSource {
  priceHour?: number | null;
  referencePriceHour?: number | null;
  minPriceHour?: number | null;
  maxPriceHour?: number | null;
}

const ECONOM_PRICE_HOUR_MAX = 4000;
const PREMIUM_PRICE_HOUR_MIN = 8000;
const DEAL_DISCOUNT_RATIO = 0.9;
const DEAL_MIN_SAVINGS = 500;

const badgeLabels: Record<CommercialBadgeKey, string> = {
  deal: "Выгодная цена",
  econom: "Эконом",
  premium: "Премиум",
};

const badgeClassMap: Record<CommercialBadgeKey, string> = {
  deal:
    "border-emerald-200 bg-emerald-50 text-emerald-700 dark:border-emerald-500/30 dark:bg-emerald-500/10 dark:text-emerald-300",
  econom:
    "border-sky-200 bg-sky-50 text-sky-700 dark:border-sky-500/30 dark:bg-sky-500/10 dark:text-sky-300",
  premium:
    "border-amber-200 bg-amber-50 text-amber-700 dark:border-amber-500/30 dark:bg-amber-500/10 dark:text-amber-300",
};

function normalizePositiveNumber(value: number | null | undefined): number | null {
  if (typeof value !== "number" || !Number.isFinite(value) || value <= 0) {
    return null;
  }

  return value;
}

function resolveReferencePriceHour(source: CommercialBadgeSource): number | null {
  const directReference = normalizePositiveNumber(source.referencePriceHour);
  if (directReference != null) {
    return directReference;
  }

  const minPriceHour = normalizePositiveNumber(source.minPriceHour);
  const maxPriceHour = normalizePositiveNumber(source.maxPriceHour);
  if (minPriceHour != null && maxPriceHour != null) {
    return (minPriceHour + maxPriceHour) / 2;
  }

  return minPriceHour ?? maxPriceHour;
}

function createBadge(key: CommercialBadgeKey): CommercialBadge {
  return {
    key,
    label: badgeLabels[key],
  };
}

export function buildCommercialBadges(
  source: CommercialBadgeSource,
  maxBadges = 2,
): CommercialBadge[] {
  const priceHour = normalizePositiveNumber(source.priceHour);
  if (priceHour == null) {
    return [];
  }

  const badges: CommercialBadge[] = [];
  const referencePriceHour = resolveReferencePriceHour(source);
  const hasDealPrice =
    referencePriceHour != null &&
    priceHour <= referencePriceHour * DEAL_DISCOUNT_RATIO &&
    referencePriceHour - priceHour >= DEAL_MIN_SAVINGS;

  if (hasDealPrice) {
    badges.push(createBadge("deal"));
  }

  if (priceHour <= ECONOM_PRICE_HOUR_MAX) {
    badges.push(createBadge("econom"));
  } else if (priceHour >= PREMIUM_PRICE_HOUR_MIN) {
    badges.push(createBadge("premium"));
  }

  return badges.slice(0, Math.max(0, maxBadges));
}

export function getCommercialBadgeClasses(key: CommercialBadgeKey): string {
  return badgeClassMap[key];
}

export function buildBadgesFromKeys(keys: string[]): CommercialBadge[] {
  return keys
    .filter((key): key is CommercialBadgeKey => key in badgeLabels)
    .map((key) => createBadge(key));
}
