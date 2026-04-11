import api from "./axios";
import type { Complaint } from "../types/Complaint";

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
