type FeatureLike = string | { name?: string | null };

export interface CarTagSource {
  engine?: string | null;
  transmission?: string | null;
  fuelType?: string | null;
  seats?: number | null;
  doors?: number | null;
  features?: FeatureLike[] | null;
}

const transmissionMap: Record<string, string> = {
  automatic: "АКПП",
  manual: "МКПП",
  cvt: "CVT",
  variator: "Вариатор",
  robot: "Робот",
  "dual clutch": "Робот",
};

const fuelTypeMap: Record<string, string> = {
  petrol: "Бензин",
  gasoline: "Бензин",
  diesel: "Дизель",
  hybrid: "Гибрид",
  electric: "Электро",
  ev: "Электро",
  gas: "Газ",
  lpg: "Газ",
};

const featureLabelMap: Record<string, string> = {
  econom: "Эконом",
  comfort: "Комфорт",
  sport: "Спортивная",
  business: "Бизнес",
  family: "Семейная",
  city: "Городская",
  luxury: "Премиум",
  suv: "Внедорожник",
  electric: "Электро",
  sedan: "Седан",
  coupe: "Купе",
};

function normalizeText(value: string | null | undefined): string | null {
  const normalized = value?.trim();
  return normalized ? normalized : null;
}

function normalizeTransmission(value: string | null | undefined): string | null {
  const normalized = normalizeText(value);
  if (!normalized) {
    return null;
  }

  return transmissionMap[normalized.toLowerCase()] ?? normalized;
}

function normalizeFuelType(value: string | null | undefined): string | null {
  const normalized = normalizeText(value);
  if (!normalized) {
    return null;
  }

  return fuelTypeMap[normalized.toLowerCase()] ?? normalized;
}

function normalizeFeature(feature: FeatureLike): string | null {
  const rawValue =
    typeof feature === "string"
      ? normalizeText(feature)
      : normalizeText(feature?.name);

  if (!rawValue) {
    return null;
  }

  return featureLabelMap[rawValue.toLowerCase()] ?? rawValue;
}

export function buildCarTags(source: CarTagSource, maxTags = 6): string[] {
  const tags: string[] = [];
  const seen = new Set<string>();

  function pushTag(value: string | null) {
    if (!value) {
      return;
    }

    const key = value.toLowerCase();
    if (seen.has(key)) {
      return;
    }

    seen.add(key);
    tags.push(value);
  }

  for (const feature of source.features ?? []) {
    pushTag(normalizeFeature(feature));
  }

  pushTag(normalizeText(source.engine));
  pushTag(normalizeTransmission(source.transmission));
  pushTag(normalizeFuelType(source.fuelType));
  pushTag(source.seats != null ? `${source.seats} мест` : null);
  pushTag(source.doors != null ? `${source.doors} двери` : null);

  return tags.slice(0, Math.max(0, maxTags));
}
