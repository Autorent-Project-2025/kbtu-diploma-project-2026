export type SemanticTag =
  | "econom"
  | "comfort"
  | "business"
  | "sport"
  | "suv"
  | "electric"
  | "family";

export interface PartnerCarSemanticInput {
  fuelType?: string | null;
  bodyType?: string | null;
  seats?: number | null;
  horsepower?: number | null;
}

export const semanticTagOptions: ReadonlyArray<{
  value: SemanticTag;
  label: string;
}> = [
  { value: "econom", label: "Эконом" },
  { value: "comfort", label: "Комфорт" },
  { value: "business", label: "Бизнес" },
  { value: "sport", label: "Спортивная" },
  { value: "suv", label: "Внедорожник" },
  { value: "electric", label: "Электро" },
  { value: "family", label: "Семейная" },
];

export const transmissionOptions = [
  { value: "automatic", label: "АКПП" },
  { value: "manual", label: "МКПП" },
  { value: "cvt", label: "CVT" },
  { value: "robot", label: "Робот" },
] as const;

export const fuelTypeOptions = [
  { value: "petrol", label: "Бензин" },
  { value: "diesel", label: "Дизель" },
  { value: "hybrid", label: "Гибрид" },
  { value: "electric", label: "Электро" },
  { value: "gas", label: "Газ" },
] as const;

export const bodyTypeOptions = [
  { value: "sedan", label: "Седан" },
  { value: "hatchback", label: "Хэтчбек" },
  { value: "wagon", label: "Универсал" },
  { value: "coupe", label: "Купе" },
  { value: "suv", label: "Внедорожник" },
  { value: "crossover", label: "Кроссовер" },
  { value: "pickup", label: "Пикап" },
  { value: "minivan", label: "Минивэн" },
] as const;

export function getSemanticTagLabel(tag: string): string {
  return semanticTagOptions.find((option) => option.value === tag)?.label ?? tag;
}

export function suggestSemanticTags(
  input: PartnerCarSemanticInput,
): SemanticTag[] {
  const suggestions: SemanticTag[] = [];

  function push(tag: SemanticTag) {
    if (!suggestions.includes(tag)) {
      suggestions.push(tag);
    }
  }

  const fuelType = input.fuelType?.trim().toLowerCase();
  const bodyType = input.bodyType?.trim().toLowerCase();
  const seats = input.seats ?? null;
  const horsepower = input.horsepower ?? null;

  if (fuelType === "electric" || fuelType === "ev") {
    push("electric");
  }

  if (bodyType === "suv" || bodyType === "crossover" || bodyType === "offroad") {
    push("suv");
  }

  if (typeof horsepower === "number" && horsepower >= 250) {
    push("sport");
  }

  if ((typeof seats === "number" && seats >= 5) || bodyType === "minivan" || bodyType === "suv") {
    push("family");
  }

  return suggestions;
}
