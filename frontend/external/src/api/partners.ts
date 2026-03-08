import api from "./axios";
import type { BookingStatus } from "../types/Booking";
import type {
  Partner,
  PartnerBooking,
  PartnerLedgerEntry,
  PartnerWallet,
} from "../types/Partner";

interface PartnerWalletApiDto {
  partnerUserId: string;
  currency: string;
  pendingAmount: number;
  availableAmount: number;
  reservedAmount: number;
  updatedAt: string;
}

interface PartnerLedgerEntryApiDto {
  id: number;
  partnerUserId: string;
  bookingId?: number | null;
  customerPaymentId?: number | null;
  partnerPayoutId?: number | null;
  entryType: string;
  bucket: string;
  amountDelta: number;
  currency: string;
  description?: string | null;
  createdAt: string;
}

interface PartnerBookingApiDto {
  id: number;
  userId: string;
  partnerCarId: number;
  partnerUserId: string;
  carBrand: string;
  carModel: string;
  startTime: string;
  endTime: string;
  priceHour?: number | null;
  totalPrice?: number | null;
  createdAt: string;
  status?: string | null;
}

function normalizeBookingStatus(value: string | null | undefined): BookingStatus {
  const normalized = (value ?? "").trim().toLowerCase();
  if (normalized === "pending") return "pending";
  if (normalized === "confirmed") return "confirmed";
  if (normalized === "active") return "active";
  if (normalized === "completed") return "completed";
  return "canceled";
}

function mapWallet(dto: PartnerWalletApiDto): PartnerWallet {
  return {
    partnerUserId: dto.partnerUserId,
    currency: dto.currency,
    pendingAmount: dto.pendingAmount ?? 0,
    availableAmount: dto.availableAmount ?? 0,
    reservedAmount: dto.reservedAmount ?? 0,
    updatedAt: dto.updatedAt,
  };
}

function mapLedgerEntry(dto: PartnerLedgerEntryApiDto): PartnerLedgerEntry {
  return {
    id: dto.id,
    partnerUserId: dto.partnerUserId,
    bookingId: dto.bookingId ?? null,
    customerPaymentId: dto.customerPaymentId ?? null,
    partnerPayoutId: dto.partnerPayoutId ?? null,
    entryType: dto.entryType,
    bucket: dto.bucket,
    amountDelta: dto.amountDelta ?? 0,
    currency: dto.currency,
    description: dto.description ?? null,
    createdAt: dto.createdAt,
  };
}

function mapBooking(dto: PartnerBookingApiDto): PartnerBooking {
  return {
    id: dto.id,
    userId: dto.userId,
    partnerCarId: dto.partnerCarId,
    partnerUserId: dto.partnerUserId,
    carBrand: dto.carBrand ?? "",
    carModel: dto.carModel ?? "",
    startTime: dto.startTime,
    endTime: dto.endTime,
    priceHour: dto.priceHour ?? null,
    totalPrice: dto.totalPrice ?? null,
    createdAt: dto.createdAt,
    status: normalizeBookingStatus(dto.status),
  };
}

export async function getMyPartner(): Promise<Partner> {
  const response = await api.get("/partners/me");
  return response.data as Partner;
}

export async function getMyPartnerWallet(): Promise<PartnerWallet> {
  const response = await api.get("/partners/me/wallet");
  return mapWallet(response.data as PartnerWalletApiDto);
}

export async function getMyPartnerLedger(take = 200): Promise<PartnerLedgerEntry[]> {
  const response = await api.get("/partners/me/ledger", {
    params: { take },
  });

  return ((response.data ?? []) as PartnerLedgerEntryApiDto[]).map(mapLedgerEntry);
}

export async function getMyPartnerBookings(): Promise<PartnerBooking[]> {
  const response = await api.get("/partners/me/bookings");
  return ((response.data ?? []) as PartnerBookingApiDto[]).map(mapBooking);
}

export interface TemporaryFileLink {
  fileName: string;
  url: string;
  expiresAtUtc: string;
}

export async function getMyPartnerFileTemporaryLink(fileName: string): Promise<TemporaryFileLink> {
  const response = await api.get("/partners/me/files/temporary-link", {
    params: { fileName },
  });
  return response.data as TemporaryFileLink;
}

export interface PartnerPublicProfile {
  relatedUserId: string;
  carrierName: string;
}

export async function getPartnerPublicProfileByRelatedUserId(
  relatedUserId: string
): Promise<PartnerPublicProfile> {
  const response = await api.get(`/partners/public/by-related-user/${encodeURIComponent(relatedUserId)}`);
  return response.data as PartnerPublicProfile;
}
