import api from "./axios";
import type { PaginatedResponse } from "../types/Pagination";

// ─── Types ───────────────────────────────────────────────────────────────────

export interface ClientProfile {
  id: number;
  firstName: string;
  lastName: string;
  createdOn: string;
  birthDate: string;
  identityDocumentFileName: string | null;
  driverLicenseFileName: string | null;
  relatedUserId: string;
  phoneNumber: string;
  avatarUrl: string | null;
}

export interface UpdateProfilePayload {
  firstName: string;
  lastName: string;
  birthDate: string;
  phoneNumber: string;
  avatarUrl: string | null;
}

export interface BookingStats {
  totalCount: number;
  activeCount: number;
  completedCount: number;
  totalSpent: number;
}

export interface MyComment {
  id: number;
  userId: string;
  userName: string;
  carId: number;
  partnerCarId: number | null;
  content: string;
  rating: number;
  createdOn: string;
}

// ─── API calls ───────────────────────────────────────────────────────────────

export async function getMyProfile(): Promise<ClientProfile> {
  const res = await api.get("/clients/profile");
  return res.data as ClientProfile;
}

export async function updateMyProfile(
  payload: UpdateProfilePayload,
): Promise<ClientProfile> {
  const res = await api.put("/clients/profile", payload);
  return res.data as ClientProfile;
}

export async function getMyBookingStats(): Promise<BookingStats> {
  const res = await api.get("/bookings/my/stats");
  return res.data as BookingStats;
}

export async function getMyComments(
  page = 1,
  pageSize = 5,
): Promise<PaginatedResponse<MyComment>> {
  const res = await api.get("/cars/comments/my", {
    params: { page, pageSize },
  });
  const data = res.data as {
    items: MyComment[];
    totalCount: number;
    page: number;
    pageSize: number;
    totalPages?: number;
  };
  return {
    items: data.items ?? [],
    totalCount: data.totalCount ?? 0,
    page: data.page ?? page,
    pageSize: data.pageSize ?? pageSize,
    totalPages:
      data.totalPages ??
      Math.ceil((data.totalCount ?? 0) / (data.pageSize ?? pageSize)),
  };
}
