import api from "./axios";
import type { Ticket } from "../types/Ticket";

export interface PartnerCarReviewPayload {
  carBrand?: string;
  carModel?: string;
  carYear?: number;
  licensePlate?: string;
  color?: string | null;
  requestedStatus?: number | null;
  isActive?: boolean | null;
  transmission?: string | null;
  fuelType?: string | null;
  seats?: number | null;
  doors?: number | null;
  bodyType?: string | null;
  horsepower?: number | null;
  confirmedTags?: string[];
}

export async function getPendingTickets(): Promise<Ticket[]> {
  const res = await api.get("/tickets/pending");
  return (res.data ?? []) as Ticket[];
}

export async function getAllTickets(search?: string): Promise<Ticket[]> {
  const res = await api.get("/tickets/all", { params: { search } });
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

export async function issueTicketFine(
  ticketId: string,
  amount: number,
  comment: string
): Promise<Ticket> {
  const res = await api.post(`/tickets/${ticketId}/issue-fine`, {
    amount,
    comment,
  });
  return res.data as Ticket;
}

export interface TicketDocumentTemporaryLink {
  fileName: string;
  url: string;
  expiresAtUtc: string;
}

export async function getTicketDocumentTemporaryLink(
  ticketId: string,
  documentType:
    | "identity"
    | "license"
    | "ownership"
    | "front"
    | "back"
    | "side_left"
    | "side_right"
    | "interior"
): Promise<TicketDocumentTemporaryLink> {
  const res = await api.get(`/tickets/${ticketId}/documents/${documentType}/temporary-link`);
  return res.data as TicketDocumentTemporaryLink;
}
