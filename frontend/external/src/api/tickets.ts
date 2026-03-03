import api from "./axios";
import type { Ticket } from "../types/Ticket";

export interface CreateTicketPayload {
  firstName: string;
  lastName: string;
  email: string;
  birthDate: string;
  phoneNumber: string;
  avatarUrl?: string;
  identityDocumentFile: File;
  driverLicenseFile: File;
}

export async function createTicket(
  payload: CreateTicketPayload
): Promise<Ticket> {
  const formData = new FormData();
  formData.append("firstName", payload.firstName);
  formData.append("lastName", payload.lastName);
  formData.append("email", payload.email);
  formData.append("birthDate", payload.birthDate);
  formData.append("phoneNumber", payload.phoneNumber);

  if (payload.avatarUrl?.trim()) {
    formData.append("avatarUrl", payload.avatarUrl.trim());
  }

  formData.append("identityDocumentFile", payload.identityDocumentFile);
  formData.append("driverLicenseFile", payload.driverLicenseFile);

  const res = await api.post("/tickets", formData, {
    headers: {
      "Content-Type": "multipart/form-data"
    }
  });

  return res.data as Ticket;
}
