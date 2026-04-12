import api from "./axios";
import type { Complaint, ReopenRequest } from "../types/Complaint";

/**
 * Создать новую жалобу (multipart/form-data).
 */
export async function createComplaint(data: FormData): Promise<Complaint> {
  const response = await api.post("/tickets/complaints", data, {
    headers: {
      "Content-Type": "multipart/form-data",
    },
  });

  return response.data as Complaint;
}

/**
 * Получить все жалобы текущего пользователя.
 */
export async function getMyComplaints(): Promise<Complaint[]> {
  const response = await api.get("/tickets/complaints/my");
  const payload = Array.isArray(response.data) ? response.data : [];
  return payload as Complaint[];
}

/**
 * Получить жалобу по ID.
 */
export async function getMyComplaintById(id: string): Promise<Complaint> {
  const response = await api.get(`/tickets/complaints/my/${id}`);
  return response.data as Complaint;
}

/**
 * Ответить на запрос информации по жалобе (multipart/form-data).
 */
export async function respondToInfoRequest(
  id: string,
  data: FormData,
): Promise<Complaint> {
  const response = await api.post(`/tickets/complaints/my/${id}/respond`, data, {
    headers: {
      "Content-Type": "multipart/form-data",
    },
  });

  return response.data as Complaint;
}

/**
 * Получить жалобу по booking ID.
 */
export async function getMyComplaintByBooking(bookingId: number): Promise<Complaint | null> {
  try {
    const response = await api.get(`/tickets/complaints/my/by-booking/${bookingId}`);
    return response.data as Complaint;
  } catch {
    return null;
  }
}

/**
 * Создать запрос на переоткрытие жалобы.
 */
export async function createReopenRequest(
  complaintId: string,
  reason: string,
): Promise<ReopenRequest> {
  const response = await api.post(`/tickets/complaints/my/${complaintId}/reopen-request`, { reason });
  return response.data as ReopenRequest;
}

/**
 * Получить запросы на переоткрытие жалобы.
 */
export async function getReopenRequests(complaintId: string): Promise<ReopenRequest[]> {
  const response = await api.get(`/tickets/complaints/my/${complaintId}/reopen-requests`);
  return (response.data ?? []) as ReopenRequest[];
}

/**
 * Получить временную ссылку на вложение жалобы.
 */
export async function getAttachmentLink(
  complaintId: string,
  attachmentId: string,
): Promise<string> {
  const response = await api.get(
    `/tickets/complaints/my/${complaintId}/attachments/${attachmentId}/temporary-link`,
  );
  return (response.data as { url: string }).url;
}
