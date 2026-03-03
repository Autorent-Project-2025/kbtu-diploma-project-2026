import api from "./axios";
import type { Ticket } from "../types/Ticket";

export async function createTicket(
  fullName: string,
  email: string,
  birthDate: string,
  phoneNumber: string
): Promise<Ticket> {
  const res = await api.post("/tickets", {
    fullName,
    email,
    birthDate,
    phoneNumber,
  });

  return res.data as Ticket;
}
