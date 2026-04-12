import api from "./axios";
import type { PaginatedResponse } from "../types/Pagination";
import { getPublicPartnerCarDetails } from "./partnerCars";
import { getPartnerPublicProfileByRelatedUserId } from "./partners";
import { resolveAssetUrl } from "../utils/resolveAssetUrl";

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
  avatarImageId: string | null;
  bookingActionsBlocked: boolean;
  bookingBlockReason: string | null;
  bookingBlockedAt: string | null;
}

export interface UpdateProfilePayload {
  firstName: string;
  lastName: string;
  birthDate: string;
  phoneNumber: string;
  avatarUrl: string | null;
  avatarImageId: string | null;
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
  carDisplayName?: string | null;
  licensePlate?: string | null;
  carrierName?: string | null;
  content: string;
  rating: number;
  createdOn: string;
}

export interface AvatarUploadResult {
  imageId: string;
  imageUrl: string;
}

interface CommentContext {
  carDisplayName: string | null;
  licensePlate: string | null;
  carrierName: string | null;
}

const commentContextCache = new Map<number, CommentContext>();
const carrierNameCache = new Map<string, string | null>();

function normalizeProfile(profile: ClientProfile): ClientProfile {
  return {
    ...profile,
    avatarUrl: resolveAssetUrl(profile.avatarUrl) ?? profile.avatarUrl,
  };
}

// ─── API calls ───────────────────────────────────────────────────────────────

export async function getMyProfile(): Promise<ClientProfile> {
  const res = await api.get("/clients/profile");
  return normalizeProfile(res.data as ClientProfile);
}

export async function updateMyProfile(
  payload: UpdateProfilePayload,
): Promise<ClientProfile> {
  const res = await api.put("/clients/profile", payload);
  return normalizeProfile(res.data as ClientProfile);
}

export async function uploadAvatarImage(
  file: File,
): Promise<AvatarUploadResult> {
  const res = await api.post("/internal/api/images", file, {
    headers: {
      "Content-Type": "application/octet-stream",
    },
  });

  const payload = res.data as AvatarUploadResult;
  if (!payload?.imageId?.trim() || !payload?.imageUrl?.trim()) {
    throw new Error("Image service returned invalid upload response.");
  }

  return {
    imageId: payload.imageId,
    imageUrl: resolveAssetUrl(payload.imageUrl) ?? payload.imageUrl,
  };
}

export async function deleteAvatarImage(imageId: string): Promise<void> {
  await api.delete(`/internal/api/images/${encodeURIComponent(imageId)}`);
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
  const items = await enrichCommentsWithContext(data.items ?? []);
  return {
    items,
    totalCount: data.totalCount ?? 0,
    page: data.page ?? page,
    pageSize: data.pageSize ?? pageSize,
    totalPages:
      data.totalPages ??
      Math.ceil((data.totalCount ?? 0) / (data.pageSize ?? pageSize)),
  };
}

async function enrichCommentsWithContext(items: MyComment[]): Promise<MyComment[]> {
  const partnerCarIds = items
    .map((comment) => comment.partnerCarId)
    .filter(
      (partnerCarId): partnerCarId is number =>
        typeof partnerCarId === "number" &&
        Number.isInteger(partnerCarId) &&
        partnerCarId > 0,
    );

  if (partnerCarIds.length === 0) {
    return items;
  }

  const uniquePartnerCarIds = Array.from(new Set(partnerCarIds));
  const contexts = await Promise.all(
    uniquePartnerCarIds.map(async (partnerCarId) => [
      partnerCarId,
      await getCommentContext(partnerCarId),
    ] as const),
  );

  const contextMap = new Map<number, CommentContext>(contexts);

  return items.map((comment) => {
    const context = comment.partnerCarId ? contextMap.get(comment.partnerCarId) : null;
    return {
      ...comment,
      carDisplayName: context?.carDisplayName ?? null,
      licensePlate: context?.licensePlate ?? null,
      carrierName: context?.carrierName ?? null,
    };
  });
}

async function getCommentContext(partnerCarId: number): Promise<CommentContext> {
  const cached = commentContextCache.get(partnerCarId);
  if (cached) {
    return cached;
  }

  try {
    const car = await getPublicPartnerCarDetails(partnerCarId);
    const carDisplayName = [
      car.modelBrand?.trim(),
      car.modelName?.trim(),
      car.modelYear ? String(car.modelYear) : "",
    ]
      .filter(Boolean)
      .join(" ")
      .trim();

    const carrierName = await resolveCarrierName(car.partnerUserId);
    const context: CommentContext = {
      carDisplayName: carDisplayName || `Машина #${partnerCarId}`,
      licensePlate: car.licensePlate?.trim() || null,
      carrierName,
    };

    commentContextCache.set(partnerCarId, context);
    return context;
  } catch {
    const fallback: CommentContext = {
      carDisplayName: `Машина #${partnerCarId}`,
      licensePlate: null,
      carrierName: null,
    };

    commentContextCache.set(partnerCarId, fallback);
    return fallback;
  }
}

async function resolveCarrierName(partnerUserId: string | null | undefined): Promise<string | null> {
  const normalizedPartnerUserId = partnerUserId?.trim() ?? "";
  if (!normalizedPartnerUserId) {
    return null;
  }

  if (carrierNameCache.has(normalizedPartnerUserId)) {
    return carrierNameCache.get(normalizedPartnerUserId) ?? null;
  }

  try {
    const profile = await getPartnerPublicProfileByRelatedUserId(normalizedPartnerUserId);
    const carrierName = profile.carrierName?.trim() || null;
    carrierNameCache.set(normalizedPartnerUserId, carrierName);
    return carrierName;
  } catch {
    carrierNameCache.set(normalizedPartnerUserId, null);
    return null;
  }
}
