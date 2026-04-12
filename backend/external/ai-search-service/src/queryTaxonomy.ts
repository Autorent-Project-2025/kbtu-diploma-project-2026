import { sql } from "./db/sql";

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

// Dynamic dictionaries — loaded from DB at startup, zero hardcoded brands/models
let _brands: string[] = [];
let _modelToBrand: Record<string, string> = {};

export function getBrandDictionary(): string[] {
  return _brands;
}

export function getModelToBrandDictionary(): Record<string, string> {
  return _modelToBrand;
}

export function getCatalogSummary(): string {
  if (_brands.length === 0) return "";
  const seen = new Set<string>();
  const entries: string[] = [];
  for (const [model, brand] of Object.entries(_modelToBrand)) {
    const key = `${brand} ${model}`;
    if (!seen.has(key)) {
      seen.add(key);
      entries.push(key);
    }
  }
  return entries.length > 0 ? `Available cars in catalog: ${entries.join(", ")}` : "";
}

export async function loadTaxonomyFromDatabase(): Promise<void> {
  const brandSet = new Set<string>();
  const modelMap: Record<string, string> = {};

  // 1. Brands and models from indexed car documents
  const carRows = await sql<{ brand: string; model: string }[]>`
    select distinct lower(brand) as brand, lower(model) as model
    from ai_car_documents
    where brand is not null and model is not null
  `;

  for (const row of carRows) {
    const brand = row.brand.trim();
    const model = row.model.trim();
    if (brand) brandSet.add(brand);
    if (model && brand) modelMap[model] = brand;
  }

  // 2. Aliases from brand_model_aliases table (cyrillic, abbreviations, etc.)
  //    Managed via SQL migration — no hardcoded aliases in code.
  try {
    const aliasRows = await sql<{ alias: string; canonical_brand: string; canonical_model: string | null }[]>`
      select lower(alias) as alias, lower(canonical_brand) as canonical_brand,
             lower(coalesce(canonical_model, '')) as canonical_model
      from brand_model_aliases
    `;

    for (const row of aliasRows) {
      const alias = row.alias.trim();
      const brand = row.canonical_brand.trim();
      if (!alias || !brand) continue;

      // Alias for a brand (e.g. "тойота" → toyota)
      brandSet.add(alias);

      // Alias for a model (e.g. "кобальт" → chevrolet cobalt)
      if (row.canonical_model?.trim()) {
        modelMap[alias] = brand;
      }
    }
  } catch {
    // Table may not exist on first migration run
  }

  _brands = [...brandSet];
  _modelToBrand = modelMap;
}

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
