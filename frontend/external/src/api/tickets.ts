import api from "./axios";
import { auth } from "../store/auth";
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
  carBrand: string;
  carModel: string;
  carYear: number;
  licensePlate: string;
  transmission?: string | null;
  fuelType?: string | null;
  seats?: number | null;
  doors?: number | null;
  bodyType?: string | null;
  horsepower?: number | null;
  selectedTags?: string[];
  ownershipDocumentFile: File;
  carImages: Array<{
    file: File;
    imageType: PartnerCarImageType;
  }>;
}

export type PartnerCarImageType =
  | "front"
  | "back"
  | "side"
  | "interior"
  | "general";

export async function createPartnerCarTicket(
  payload: CreatePartnerCarTicketPayload
): Promise<Ticket> {
  const formData = new FormData();
  formData.append("ticketType", "PartnerCar");
  const email = auth.getEmail();
  if (email) {
    formData.append("email", email);
  }
  formData.append("carBrand", payload.carBrand);
  formData.append("carModel", payload.carModel);
  formData.append("carYear", String(payload.carYear));
  formData.append("licensePlate", payload.licensePlate);
  if (payload.transmission) {
    formData.append("transmission", payload.transmission);
  }
  if (payload.fuelType) {
    formData.append("fuelType", payload.fuelType);
  }
  if (payload.seats != null) {
    formData.append("seats", String(payload.seats));
  }
  if (payload.doors != null) {
    formData.append("doors", String(payload.doors));
  }
  if (payload.bodyType) {
    formData.append("bodyType", payload.bodyType);
  }
  if (payload.horsepower != null) {
    formData.append("horsepower", String(payload.horsepower));
  }
  for (const tag of payload.selectedTags ?? []) {
    formData.append("selectedTags", tag);
  }
  formData.append("ownershipDocumentFile", payload.ownershipDocumentFile);

  for (const image of payload.carImages) {
    formData.append("carImageFiles", image.file);
    formData.append("carImageTypes", image.imageType);
  }

  const res = await api.post("/tickets", formData, {
    headers: {
      "Content-Type": "multipart/form-data",
    },
  });

  return res.data as Ticket;
}
