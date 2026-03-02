import api from "./axios";
import type { Ticket } from "../types/Ticket";

export async function createTicket(
  fullName: string,
  email: string,
  birthDate: string
): Promise<Ticket> {
  const res = await api.post("/tickets", {
    fullName,
    email,
    birthDate,
  });

  return res.data as Ticket;
}
