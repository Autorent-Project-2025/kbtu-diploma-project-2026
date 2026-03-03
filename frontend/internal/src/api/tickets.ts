import api from "./axios";
import type { Ticket } from "../types/Ticket";

export async function getPendingTickets(): Promise<Ticket[]> {
  const res = await api.get("/tickets/pending");
  return (res.data ?? []) as Ticket[];
}

export async function getTicketById(ticketId: string): Promise<Ticket> {
  const res = await api.get(`/tickets/${ticketId}`);
  return res.data as Ticket;
}

export async function approveTicket(ticketId: string): Promise<Ticket> {
  const res = await api.post(`/tickets/${ticketId}/approve`);
  return res.data as Ticket;
}

export async function rejectTicket(ticketId: string, decisionReason: string): Promise<Ticket> {
  const res = await api.post(`/tickets/${ticketId}/reject`, { decisionReason });
  return res.data as Ticket;
}

export interface TicketDocumentTemporaryLink {
  fileName: string;
  url: string;
  expiresAtUtc: string;
}

export async function getTicketDocumentTemporaryLink(
  ticketId: string,
  documentType: "identity" | "license"
): Promise<TicketDocumentTemporaryLink> {
  const res = await api.get(`/tickets/${ticketId}/documents/${documentType}/temporary-link`);
  return res.data as TicketDocumentTemporaryLink;
}
