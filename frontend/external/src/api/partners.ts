import api from "./axios";
import type { Partner } from "../types/Partner";

export async function getMyPartner(): Promise<Partner> {
  const response = await api.get("/partners/me");
  return response.data as Partner;
}
