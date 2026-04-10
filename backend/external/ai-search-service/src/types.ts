export type ParsedRecommendationQuery = {
  prompt: string;
  maxBudgetPerHour: number | null;
  passengers: number | null;
  transmission: string | null;
  minRating: number | null;
  preferredStyles: string[];
  excludedStyles: string[];
  preferredBrands: string[];
  minYear: number | null;
  startTime: string | null;
  endTime: string | null;
  requiresAvailableOnDates: boolean;
};

export type SearchDocument = {
  partnerCarId: number;
  carModelId: number;
  partnerUserId: string;
  carrierName: string | null;
  brand: string;
  model: string;
  year: number;
  title: string;
  description: string | null;
  color: string | null;
  transmission: string | null;
  fuelType: string | null;
  engine: string | null;
  seats: number | null;
  priceHour: number | null;
  priceDay: number | null;
  rating: number | null;
  ratingsCount: number;
  imageUrl: string | null;
  detailsUrl: string;
  bookingUrl: string;
  tags: string[];
  searchableText: string;
  embedding: number[];
};

export type SearchCandidate = {
  partnerCarId: number;
  carModelId: number;
  brand: string;
  model: string;
  year: number;
  title: string;
  imageUrl: string | null;
  detailsUrl: string;
  bookingUrl: string;
  priceHour: number | null;
  priceDay: number | null;
  rating: number | null;
  ratingsCount: number;
  carrierName: string | null;
  tags: string[];
  lexicalScore: number;
  vectorScore: number;
  businessScore: number;
  finalScore: number;
  reasons: string[];
};

export type AiRecommendationResponse = {
  assistantText: string;
  appliedFilters: ParsedRecommendationQuery;
  totalCandidates: number;
  cars: SearchCandidate[];
};

export type AiChatMessage = {
  id: number;
  role: "assistant" | "user";
  content: string;
  cars: SearchCandidate[];
  appliedFilters: ParsedRecommendationQuery | null;
};

export type AiChatHistoryResponse = {
  messages: AiChatMessage[];
};
