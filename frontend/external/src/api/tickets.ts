import api from "./axios";
import type { Ticket } from "../types/Ticket";

export interface CreateTicketPayload {
  firstName: string;
  lastName: string;
  email: string;
  birthDate: string;
  phoneNumber: string;
  identityDocumentFile: File;
  driverLicenseFile: File;
}

export async function createTicket(
  payload: CreateTicketPayload
): Promise<Ticket> {
  const formData = new FormData();
  formData.append("ticketType", "Client");
  formData.append("firstName", payload.firstName);
  formData.append("lastName", payload.lastName);
  formData.append("email", payload.email);
  formData.append("birthDate", payload.birthDate);
  formData.append("phoneNumber", payload.phoneNumber);

  formData.append("identityDocumentFile", payload.identityDocumentFile);
  formData.append("driverLicenseFile", payload.driverLicenseFile);

  const res = await api.post("/tickets", formData, {
    headers: {
      "Content-Type": "multipart/form-data"
    }
  });

  return res.data as Ticket;
}

export interface CreatePartnerTicketPayload {
  ownerFirstName: string;
  ownerLastName: string;
  ownerEmail: string;
  phoneNumber: string;
  ownerIdentityFile: File;
}

export async function createPartnerTicket(
  payload: CreatePartnerTicketPayload
): Promise<Ticket> {
  const formData = new FormData();
  formData.append("ticketType", "Partner");
  formData.append("firstName", payload.ownerFirstName);
  formData.append("lastName", payload.ownerLastName);
  formData.append("email", payload.ownerEmail);
  formData.append("phoneNumber", payload.phoneNumber);
  formData.append("identityDocumentFile", payload.ownerIdentityFile);

  const res = await api.post("/tickets", formData, {
    headers: {
      "Content-Type": "multipart/form-data"
    }
  });

  return res.data as Ticket;
}

export interface CreatePartnerCarTicketPayload {
  email: string;
  carBrand: string;
  carModel: string;
  carYear: number;
  licensePlate: string;
  ownershipDocumentFile: File;
  carImageFiles: File[];
}

export async function createPartnerCarTicket(
  payload: CreatePartnerCarTicketPayload
): Promise<Ticket> {
  const formData = new FormData();
  formData.append("ticketType", "PartnerCar");
  formData.append("email", payload.email);
  formData.append("carBrand", payload.carBrand);
  formData.append("carModel", payload.carModel);
  formData.append("carYear", String(payload.carYear));
  formData.append("licensePlate", payload.licensePlate);
  formData.append("ownershipDocumentFile", payload.ownershipDocumentFile);

  for (const file of payload.carImageFiles) {
    formData.append("carImageFiles", file);
  }

  const res = await api.post("/tickets", formData, {
    headers: {
      "Content-Type": "multipart/form-data",
    },
  });

  return res.data as Ticket;
}
