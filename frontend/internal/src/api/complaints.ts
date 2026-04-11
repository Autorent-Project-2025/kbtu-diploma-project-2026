import api from "./axios";
import type { Complaint } from "../types/Complaint";

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
  resolutionType: string,
  resolutionNote: string,
): Promise<Complaint> {
  const res = await api.post(`/tickets/complaints/all/${id}/resolve`, { resolutionType, resolutionNote });
  return res.data as Complaint;
}

export async function rejectComplaint(id: string, reason: string): Promise<Complaint> {
  const res = await api.post(`/tickets/complaints/all/${id}/reject`, { reason });
  return res.data as Complaint;
}

export async function getComplaintAttachmentLink(
  complaintId: string,
  attachmentId: string,
): Promise<ComplaintAttachmentTemporaryLink> {
  const res = await api.get(`/tickets/complaints/all/${complaintId}/attachments/${attachmentId}/temporary-link`);
  return res.data as ComplaintAttachmentTemporaryLink;
}
