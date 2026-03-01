import api from "./axios";
import type { Booking } from "../types/Booking";
import type { PaginatedResponse } from "../types/Pagination";

export interface GetMyBookingsParams {
  page?: number;
  pageSize?: number;
}

/**
 * Получить бронирования текущего пользователя с пагинацией
 * Пытается использовать /api/v2/bookings/my (с пагинацией)
 * Если не работает, fallback на /api/booking/my
 */
export async function getMyBookings(
  params?: GetMyBookingsParams
): Promise<PaginatedResponse<Booking> | Booking[]> {
  try {
    // Пробуем использовать v2 endpoint с пагинацией
    const queryParams = new URLSearchParams();

    if (params?.page) queryParams.append("page", params.page.toString());
    if (params?.pageSize)
      queryParams.append("pageSize", params.pageSize.toString());

    const url = `/v2/bookings/my${
      queryParams.toString() ? "?" + queryParams.toString() : ""
    }`;
    const res = await api.get(url);

    // Если статус 204 No Content - возвращаем пустой массив
    if (res.status === 204 || !res.data) {
      return {
        items: [],
        totalCount: 0,
        page: params?.page || 1,
        pageSize: params?.pageSize || 10,
        totalPages: 0,
      };
    }

    // Если ответ содержит items, это пагинированный ответ
    if (res.data.items) {
      return res.data as PaginatedResponse<Booking>;
    }

    // Иначе это просто массив - оборачиваем в пагинированный формат
    const items = Array.isArray(res.data) ? res.data : [];
    return {
      items,
      totalCount: items.length,
      page: 1,
      pageSize: items.length,
      totalPages: items.length > 0 ? 1 : 0,
    };
  } catch (error) {
    // Fallback на старый endpoint
    console.log("v2 endpoint not available, using fallback /booking/my");

    try {
      const res = await api.get("/booking/my");

      // Если статус 204 No Content - возвращаем пустой массив
      if (res.status === 204 || !res.data) {
        return {
          items: [],
          totalCount: 0,
          page: params?.page || 1,
          pageSize: params?.pageSize || 10,
          totalPages: 0,
        };
      }

      const items = Array.isArray(res.data) ? res.data : [];
      // Оборачиваем в пагинированный формат для единообразия
      return {
        items,
        totalCount: items.length,
        page: 1,
        pageSize: items.length,
        totalPages: items.length > 0 ? 1 : 0,
      };
    } catch (fallbackError) {
      console.error(
        "Failed to fetch bookings from fallback endpoint:",
        fallbackError
      );
      // Возвращаем пустой результат вместо ошибки
      return {
        items: [],
        totalCount: 0,
        page: params?.page || 1,
        pageSize: params?.pageSize || 10,
        totalPages: 0,
      };
    }
  }
}

/**
 * Создать новое бронирование
 */
export async function createBooking(carId: number, start: string, end: string) {
  const res = await api.post("/booking", {
    carId,
    startDate: start,
    endDate: end,
  });
  return res.data;
}

/**
 * Отменить бронирование
 */
export async function cancelBooking(bookingId: number) {
  const res = await api.post(`/booking/${bookingId}/cancel`);
  return res.data;
}

/**
 * Получить бронирования для конкретной машины
 */
export async function getCarBookings(carId: number) {
  const res = await api.get(`/booking/car/${carId}`);
  return res.data;
}
