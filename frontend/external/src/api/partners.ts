import api from "./axios";
import type { Partner } from "../types/Partner";

export async function getMyPartner(): Promise<Partner> {
  const response = await api.get("/partners/me");
  return response.data as Partner;
}

export interface TemporaryFileLink {
  fileName: string;
  url: string;
  expiresAtUtc: string;
}

export async function getMyPartnerFileTemporaryLink(fileName: string): Promise<TemporaryFileLink> {
  const response = await api.get("/partners/me/files/temporary-link", {
    params: { fileName },
  });
  return response.data as TemporaryFileLink;
}

export interface PartnerPublicProfile {
  relatedUserId: string;
  carrierName: string;
}

export async function getPartnerPublicProfileByRelatedUserId(
  relatedUserId: string
): Promise<PartnerPublicProfile> {
  const response = await api.get(`/partners/public/by-related-user/${encodeURIComponent(relatedUserId)}`);
  return response.data as PartnerPublicProfile;
}
