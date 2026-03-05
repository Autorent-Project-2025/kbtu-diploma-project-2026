import api from "./axios";
import type { Ticket } from "../types/Ticket";

export interface PartnerCarReviewPayload {
  carBrand?: string;
  carModel?: string;
  carYear?: number;
  licensePlate?: string;
  priceHour?: number;
  priceDay?: number;
  email?: string;
}

export async function getPendingTickets(): Promise<Ticket[]> {
  const res = await api.get("/tickets/pending");
  return (res.data ?? []) as Ticket[];
}

export async function getTicketById(ticketId: string): Promise<Ticket> {
  const res = await api.get(`/tickets/${ticketId}`);
  return res.data as Ticket;
}

export async function approveTicket(
  ticketId: string,
  partnerCarData?: PartnerCarReviewPayload
): Promise<Ticket> {
  const res = await api.post(`/tickets/${ticketId}/approve`, {
    partnerCarData,
  });
  return res.data as Ticket;
}

export async function rejectTicket(
  ticketId: string,
  decisionReason: string,
  partnerCarData?: PartnerCarReviewPayload
): Promise<Ticket> {
  const res = await api.post(`/tickets/${ticketId}/reject`, { decisionReason, partnerCarData });
  return res.data as Ticket;
}

export interface TicketDocumentTemporaryLink {
  fileName: string;
  url: string;
  expiresAtUtc: string;
}

export async function getTicketDocumentTemporaryLink(
  ticketId: string,
  documentType: "identity" | "license" | "ownership"
): Promise<TicketDocumentTemporaryLink> {
  const res = await api.get(`/tickets/${ticketId}/documents/${documentType}/temporary-link`);
  return res.data as TicketDocumentTemporaryLink;
}
