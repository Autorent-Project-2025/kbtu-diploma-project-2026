export function buildSearchTags(input: {
  brand: string;
  model: string;
  year: number;
  engine?: string | null;
  transmission?: string | null;
  fuelType?: string | null;
  seats?: number | null;
  features?: Array<{ name?: string | null }> | null;
  priceHour?: number | null;
}): string[] {
  const tags = new Set<string>();
  const normalizedTransmission = input.transmission?.trim().toLowerCase() ?? "";
  const normalizedFuelType = input.fuelType?.trim().toLowerCase() ?? "";

  for (const feature of input.features ?? []) {
    if (feature?.name?.trim()) {
      tags.add(feature.name.trim().toLowerCase());
    }
  }

  if (input.engine) {
    tags.add(input.engine.trim());
  }

  if (input.transmission) {
    tags.add(input.transmission.trim());
  }

  if (input.fuelType) {
    tags.add(input.fuelType.trim());
  }

  if (typeof input.seats === "number") {
    tags.add(`${input.seats} seats`);
  }

  if (normalizedTransmission.includes("auto")) {
    tags.add("automatic");
  }

  if (normalizedTransmission.includes("man")) {
    tags.add("manual");
  }

  if (normalizedFuelType.includes("petrol") || normalizedFuelType.includes("benz")) {
    tags.add("petrol");
  }

  return [...tags];
}
