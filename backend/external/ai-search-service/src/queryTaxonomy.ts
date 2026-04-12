export const STYLE_DICTIONARY: Array<{ label: string; variants: string[] }> = [
  {
    label: "sport",
    variants: ["sport", "sports", "спортив", "спорт", "спорткар", "купе", "coupe", "rx7", "supra"],
  },
  {
    label: "business",
    variants: ["business", "бизнес", "делов", "meeting", "airport", "аэропорт", "aeroport"],
  },
  { label: "family", variants: ["family", "сем", "дет", "багаж", "children", "trunk"] },
  { label: "city", variants: ["city", "город", "urban", "ежеднев", "парковк"] },
  { label: "luxury", variants: ["luxury", "premium", "люкс", "премиум"] },
];

export const TRANSMISSION_DICTIONARY: Array<{ label: string; variants: string[] }> = [
  { label: "automatic", variants: ["automatic", "автомат", "акпп"] },
  { label: "manual", variants: ["manual", "механик", "мкпп"] },
];

export const BRAND_DICTIONARY = [
  "toyota",
  "nissan",
  "kia",
  "mazda",
  "bmw",
  "mercedes",
  "lexus",
  "audi",
];

export const STYLE_LABELS_TEXT = STYLE_DICTIONARY.map((item) => item.label).join(", ");
export const TRANSMISSION_LABELS_TEXT = TRANSMISSION_DICTIONARY.map((item) => item.label).join(", ");

export const LOCAL_EMBEDDING_TOKEN_SYNONYMS: Record<string, string[]> = {
  ...Object.fromEntries(
    STYLE_DICTIONARY.map((item) => [item.label, item.variants]),
  ),
  ...Object.fromEntries(
    TRANSMISSION_DICTIONARY.map((item) => [item.label, item.variants]),
  ),
  budget: ["budget", "бюджет", "cheap", "деш", "эконом"],
};
