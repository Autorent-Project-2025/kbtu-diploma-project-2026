import api from "./axios";
import type { AccessRequest, BookingReview } from "../types/AccessRequest";

// ── Manager endpoints ──

export async function createAccessRequest(
  complaintId: string,
  reason: string,
): Promise<AccessRequest> {
  const res = await api.post(
    `/tickets/complaints/${complaintId}/booking-access-requests`,
    { reason },
  );
  return res.data as AccessRequest;
}

export async function getMyAccessRequest(
  complaintId: string,
): Promise<AccessRequest | null> {
  const res = await api.get(
    `/tickets/complaints/${complaintId}/booking-access-requests/mine`,
  );
  return (res.data ?? null) as AccessRequest | null;
}

export async function getBookingReview(
  complaintId: string,
): Promise<BookingReview> {
  const res = await api.get(
    `/tickets/complaints/${complaintId}/booking-review`,
  );
  return res.data as BookingReview;
}

// ── Supermanager endpoints ──

export async function getAllAccessRequests(
  status?: number,
): Promise<AccessRequest[]> {
  const res = await api.get("/tickets/complaints/access-requests", {
    params: status != null ? { status } : undefined,
  });
  return (res.data ?? []) as AccessRequest[];
}

export async function getAccessRequestById(
  id: string,
): Promise<AccessRequest> {
  const res = await api.get(`/tickets/complaints/access-requests/${id}`);
  return res.data as AccessRequest;
}

export async function approveAccessRequest(
  id: string,
  decisionNote?: string,
  expiresInHours: number = 24,
): Promise<AccessRequest> {
  const res = await api.post(
    `/tickets/complaints/access-requests/${id}/approve`,
    { decisionNote, expiresInHours },
  );
  return res.data as AccessRequest;
}

export async function rejectAccessRequest(
  id: string,
  decisionNote?: string,
): Promise<AccessRequest> {
  const res = await api.post(
    `/tickets/complaints/access-requests/${id}/reject`,
    { decisionNote },
  );
  return res.data as AccessRequest;
}

export async function revokeAccessRequest(
  id: string,
): Promise<AccessRequest> {
  const res = await api.post(
    `/tickets/complaints/access-requests/${id}/revoke`,
  );
  return res.data as AccessRequest;
}
