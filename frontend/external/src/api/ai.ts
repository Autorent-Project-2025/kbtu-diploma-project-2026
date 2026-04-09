import api from "./axios";
import { resolveAssetUrl } from "../utils/resolveAssetUrl";

type NumericLike = number | string | null | undefined;

export interface AiRecommendationCard {
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
}

export interface AiRecommendationQuery {
  prompt: string;
  maxBudgetPerHour: number | null;
  passengers: number | null;
  transmission: string | null;
  preferredStyles: string[];
  preferredBrands: string[];
  minYear: number | null;
  startTime: string | null;
  endTime: string | null;
  requiresAvailableOnDates: boolean;
}

export interface AiRecommendationResponse {
  assistantText: string;
  appliedFilters: AiRecommendationQuery;
  totalCandidates: number;
  cars: AiRecommendationCard[];
}

export interface AiChatMessage {
  id: number;
  role: "assistant" | "user";
  content: string;
  cars: AiRecommendationCard[];
}

export interface AiChatHistoryResponse {
  messages: AiChatMessage[];
}

type AiRecommendationCardDto = Omit<
  AiRecommendationCard,
  | "priceHour"
  | "priceDay"
  | "rating"
  | "lexicalScore"
  | "vectorScore"
  | "businessScore"
  | "finalScore"
> & {
  priceHour: NumericLike;
  priceDay: NumericLike;
  rating: NumericLike;
  lexicalScore: NumericLike;
  vectorScore: NumericLike;
  businessScore: NumericLike;
  finalScore: NumericLike;
};

type AiRecommendationResponseDto = Omit<AiRecommendationResponse, "cars"> & {
  cars: AiRecommendationCardDto[];
};

type AiChatMessageDto = Omit<AiChatMessage, "cars"> & {
  cars: AiRecommendationCardDto[];
};

type AiChatHistoryResponseDto = {
  messages: AiChatMessageDto[];
};

function toNullableNumber(value: NumericLike): number | null {
  if (value == null || value === "") {
    return null;
  }

  const parsed = Number(value);
  return Number.isFinite(parsed) ? parsed : null;
}

function normalizeRecommendationCard(
  car: AiRecommendationCardDto,
): AiRecommendationCard {
  return {
    ...car,
    priceHour: toNullableNumber(car.priceHour),
    priceDay: toNullableNumber(car.priceDay),
    rating: toNullableNumber(car.rating),
    lexicalScore: toNullableNumber(car.lexicalScore) ?? 0,
    vectorScore: toNullableNumber(car.vectorScore) ?? 0,
    businessScore: toNullableNumber(car.businessScore) ?? 0,
    finalScore: toNullableNumber(car.finalScore) ?? 0,
    imageUrl: resolveAssetUrl(car.imageUrl) ?? car.imageUrl,
  };
}

function normalizeChatMessage(message: AiChatMessageDto): AiChatMessage {
  return {
    id: typeof message.id === "number" && Number.isFinite(message.id) ? message.id : 0,
    role: message.role === "assistant" ? "assistant" : "user",
    content: typeof message.content === "string" ? message.content : "",
    cars: Array.isArray(message.cars)
      ? message.cars.map((car) => normalizeRecommendationCard(car))
      : [],
  };
}

export async function getAiRecommendations(
  prompt: string,
): Promise<AiRecommendationResponse> {
  const response = await api.post("/ai/recommendations", { prompt });
  const payload = response.data as AiRecommendationResponseDto;

  return {
    ...payload,
    cars: (payload.cars ?? []).map((car) => normalizeRecommendationCard(car)),
  };
}

export async function getAiChatHistory(): Promise<AiChatHistoryResponse> {
  const response = await api.get("/ai/history");
  const payload = response.data as AiChatHistoryResponseDto;

  return {
    messages: (payload.messages ?? []).map((message) => normalizeChatMessage(message)),
  };
}

export async function saveAiChatHistory(
  messages: AiChatMessage[],
): Promise<AiChatHistoryResponse> {
  const response = await api.put("/ai/history", { messages });
  const payload = response.data as AiChatHistoryResponseDto;

  return {
    messages: (payload.messages ?? []).map((message) => normalizeChatMessage(message)),
  };
}
