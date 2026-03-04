import api from "./axios";
import type { Car, CarDetails } from "../types/Car";
import type { PaginatedResponse } from "../types/Pagination";

// Функция для преобразования PascalCase → camelCase
function toCamelCase(obj: any): any {
  if (Array.isArray(obj)) {
    return obj.map(toCamelCase);
  } else if (obj !== null && typeof obj === "object") {
    return Object.keys(obj).reduce((result, key) => {
      const camelKey = key.charAt(0).toLowerCase() + key.slice(1);
      result[camelKey] = toCamelCase(obj[key]);
      return result;
    }, {} as any);
  }
  return obj;
}

export interface GetCarsParams {
  brand?: string;
  model?: string;
  sortBy?: "rating" | "priceHour" | "year";
  sortOrder?: "asc" | "desc";
  page?: number;
  pageSize?: number;
}

export interface GetCarCommentsParams {
  page?: number;
  pageSize?: number;
}

export interface AvailableCarModel {
  modelId: number;
  brand: string;
  model: string;
  year: number;
  availableCarsCount: number;
  minPriceHour?: number | null;
  maxPriceHour?: number | null;
  averageRating?: number | null;
}

export interface MatchCarByModelPayload {
  modelId: number;
  startTime: string;
  endTime: string;
}

export interface MatchCarByModelResult {
  isAvailable: boolean;
  partnerCarId?: number | null;
  partnerId?: string | null;
  priceHour?: number | null;
  modelBrand?: string | null;
  modelName?: string | null;
  modelYear?: number | null;
  suggestedStartTimesUtc?: string[];
}

/**
 * Получить список всех автомобилей с пагинацией
 */
export async function getCars(
  params?: GetCarsParams
): Promise<PaginatedResponse<Car> | Car[]> {
  console.log("🔍 getCars called with params:", params);

  try {
    // Загружаем список машин через gateway
    const queryParams = new URLSearchParams();

    if (params?.brand) queryParams.append("brand", params.brand);
    if (params?.model) queryParams.append("model", params.model);

    // ✅ ИСПРАВЛЕНО: Отправляем правильные имена полей в API
    if (params?.sortBy) {
      // Преобразуем camelCase → PascalCase для бэкенда
      let apiSortBy: string = params.sortBy;
      if (params.sortBy === "priceHour") {
        apiSortBy = "PriceHour";
      } else if (params.sortBy === "rating") {
        apiSortBy = "Rating";
      } else if (params.sortBy === "year") {
        apiSortBy = "Year";
      }
      queryParams.append("sortBy", apiSortBy);
      console.log("📤 Sending sortBy to API:", apiSortBy);
    }

    if (params?.sortOrder) {
      queryParams.append("sortOrder", params.sortOrder);
      console.log("📤 Sending sortOrder to API:", params.sortOrder);
    }

    if (params?.page) queryParams.append("page", params.page.toString());
    if (params?.pageSize)
      queryParams.append("pageSize", params.pageSize.toString());

    const url = `/cars${
      queryParams.toString() ? "?" + queryParams.toString() : ""
    }`;

    console.log("🌐 API Request URL:", url);
    const res = await api.get(url);

    // Преобразуем PascalCase → camelCase
    const transformed = toCamelCase(res.data);
    console.log("✅ Transformed response:", transformed);

    // Если ответ содержит items, это пагинированный ответ
    if (transformed.items) {
      console.log(
        "📦 Returning paginated response with",
        transformed.items.length,
        "items"
      );
      return transformed as PaginatedResponse<Car>;
    }

    // Иначе это просто массив - оборачиваем в пагинированный формат
    console.log("📦 Wrapping array response");
    return {
      items: transformed,
      totalCount: transformed.length,
      page: 1,
      pageSize: transformed.length,
      totalPages: 1,
    };
  } catch (error) {
    // Fallback на тот же endpoint, если основной запрос не удался
    console.log("⚠️ Primary endpoint failed, using fallback /cars");
    const res = await api.get("/cars");
    const items = toCamelCase(res.data);

    console.log("📥 Fallback data received:", items);

    // Применяем клиентскую сортировку и пагинацию
    let sortedItems = [...items];

    // ✅ ИСПРАВЛЕНО: Правильная сортировка с числовым сравнением
    if (params?.sortBy) {
      console.log(
        "🔄 Applying client-side sort:",
        params.sortBy,
        params.sortOrder
      );

      sortedItems.sort((a, b) => {
        const aVal = a[params.sortBy!];
        const bVal = b[params.sortBy!];

        // ✅ Числовое сравнение для чисел
        const aNum = Number(aVal);
        const bNum = Number(bVal);

        if (params.sortOrder === "asc") {
          return aNum - bNum;
        } else {
          return bNum - aNum;
        }
      });

      console.log(
        "✅ Sorted items (first 3):",
        sortedItems.slice(0, 3).map((car) => ({
          brand: car.brand,
          [params.sortBy!]: car[params.sortBy!],
        }))
      );
    }

    // Пагинация
    const page = params?.page || 1;
    const pageSize = params?.pageSize || 9;
    const startIndex = (page - 1) * pageSize;
    const endIndex = startIndex + pageSize;
    const paginatedItems = sortedItems.slice(startIndex, endIndex);

    console.log("📄 Pagination:", {
      page,
      pageSize,
      startIndex,
      endIndex,
      totalItems: sortedItems.length,
      returnedItems: paginatedItems.length,
    });

    return {
      items: paginatedItems,
      totalCount: sortedItems.length,
      page,
      pageSize,
      totalPages: Math.ceil(sortedItems.length / pageSize),
    };
  }
}

/**
 * Получить детали конкретного автомобиля
 */
export async function getCarDetails(id: number): Promise<CarDetails> {
  const res = await api.get(`/cars/${id}`);
  console.log("Raw backend response:", res.data);
  const transformed = toCamelCase(res.data);
  console.log("Transformed to camelCase:", transformed);
  return transformed;
}

export async function getAvailableCarModels(): Promise<AvailableCarModel[]> {
  const response = await api.get("/cars/available-models");
  const payload = toCamelCase(response.data);
  return (payload ?? []) as AvailableCarModel[];
}

export async function matchCarByModel(
  payload: MatchCarByModelPayload
): Promise<MatchCarByModelResult> {
  const response = await api.post("/cars/match", payload);
  return toCamelCase(response.data) as MatchCarByModelResult;
}

/**
 * Получить комментарии для конкретного автомобиля с пагинацией
 */
export async function getCarComments(
  carId: number,
  params?: GetCarCommentsParams
): Promise<PaginatedResponse<any>> {
  try {
    // Сначала получаем детали машины (там есть комментарии)
    const carDetails = await getCarDetails(carId);

    // Извлекаем комментарии из деталей машины
    const allComments = carDetails.comments || [];

    // Применяем пагинацию
    const page = params?.page || 1;
    const pageSize = params?.pageSize || 3;
    const startIndex = (page - 1) * pageSize;
    const endIndex = startIndex + pageSize;
    const paginatedComments = allComments.slice(startIndex, endIndex);

    return {
      items: paginatedComments,
      totalCount: allComments.length,
      page,
      pageSize,
      totalPages: Math.ceil(allComments.length / pageSize),
    };
  } catch (error) {
    console.log("Error loading comments:", error);
    return {
      items: [],
      totalCount: 0,
      page: 1,
      pageSize: params?.pageSize || 3,
      totalPages: 0,
    };
  }
}

/**
 * Создать комментарий к машине
 */
export async function createCarComment(
  carId: number,
  content: string,
  rating: number
) {
  const res = await api.post("/cars/comment", {
    carId,
    content,
    rating,
  });
  return toCamelCase(res.data);
}

/**
 * Обновить комментарий
 */
export async function updateCarComment(
  commentId: number,
  content: string,
  rating: number
) {
  const res = await api.put(`/cars/comment/${commentId}`, {
    content,
    rating,
  });
  return toCamelCase(res.data);
}

/**
 * Удалить комментарий
 */
export async function deleteCarComment(commentId: number) {
  const res = await api.delete(`/cars/comment/${commentId}`);
  return res.data;
}
