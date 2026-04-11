import api from "./axios";
import type { Complaint, ReopenRequest } from "../types/Complaint";

export interface ComplaintsFilter {
  status?: number;
  category?: number;
  priority?: number;
  assignedTo?: string;
}

export interface ComplaintAttachmentTemporaryLink {
  fileName: string;
  url: string;
  expiresAtUtc: string;
}

export async function getAllComplaints(filters?: ComplaintsFilter): Promise<Complaint[]> {
  const res = await api.get("/tickets/complaints/all", { params: filters });
  return (res.data ?? []) as Complaint[];
}

export async function getComplaintById(id: string): Promise<Complaint> {
  const res = await api.get(`/tickets/complaints/all/${id}`);
  return res.data as Complaint;
}

export async function takeComplaint(id: string): Promise<Complaint> {
  const res = await api.post(`/tickets/complaints/all/${id}/take`);
  return res.data as Complaint;
}

export async function requestInfo(id: string, message: string): Promise<Complaint> {
  const res = await api.post(`/tickets/complaints/all/${id}/request-info`, { message });
  return res.data as Complaint;
}

export async function addManagerNote(id: string, note: string): Promise<Complaint> {
  const res = await api.post(`/tickets/complaints/all/${id}/note`, { note });
  return res.data as Complaint;
}

export async function resolveComplaint(
  id: string,
  resolutionNote: string,
): Promise<Complaint> {
  const res = await api.post(`/tickets/complaints/all/${id}/resolve`, { resolutionNote });
  return res.data as Complaint;
}

export async function rejectComplaint(id: string, reason: string): Promise<Complaint> {
  const res = await api.post(`/tickets/complaints/all/${id}/reject`, { reason });
  return res.data as Complaint;
}

export async function getReopenRequests(complaintId: string): Promise<ReopenRequest[]> {
  const res = await api.get(`/tickets/complaints/all/${complaintId}/reopen-requests`);
  return (res.data ?? []) as ReopenRequest[];
}

export async function approveReopenRequest(requestId: string, note?: string): Promise<void> {
  await api.post(`/tickets/complaints/all/reopen-requests/${requestId}/approve`, { note });
}

export async function rejectReopenRequest(requestId: string, note?: string): Promise<void> {
  await api.post(`/tickets/complaints/all/reopen-requests/${requestId}/reject`, { note });
}

// ── Manager action endpoints ──

export async function cancelComplaintBooking(id: string, reason: string): Promise<Complaint> {
  const res = await api.post(`/tickets/complaints/all/${id}/actions/cancel-booking`, { reason });
  return res.data as Complaint;
}

export async function waiveComplaintCharge(
  id: string,
  chargeId: number,
  reason: string,
): Promise<Complaint> {
  const res = await api.post(`/tickets/complaints/all/${id}/actions/waive-charge`, { chargeId, reason });
  return res.data as Complaint;
}

export async function escalateComplaint(id: string, reason: string): Promise<Complaint> {
  const res = await api.post(`/tickets/complaints/all/${id}/actions/escalate`, { reason });
  return res.data as Complaint;
}

export interface ComplaintActionLog {
  id: string;
  complaintId: string;
  actionType: string;
  performedBy: string;
  comment?: string | null;
  targetEntityType?: string | null;
  targetEntityId?: string | null;
  createdAt: string;
}

export async function getComplaintActionLogs(id: string): Promise<ComplaintActionLog[]> {
  const res = await api.get(`/tickets/complaints/all/${id}/action-logs`);
  return (res.data ?? []) as ComplaintActionLog[];
}

export async function getComplaintAttachmentLink(
  complaintId: string,
  attachmentId: string,
): Promise<ComplaintAttachmentTemporaryLink> {
  const res = await api.get(`/tickets/complaints/all/${complaintId}/attachments/${attachmentId}/temporary-link`);
  return res.data as ComplaintAttachmentTemporaryLink;
}
