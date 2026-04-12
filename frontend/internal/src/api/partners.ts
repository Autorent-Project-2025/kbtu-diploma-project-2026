import api from "./axios";

export interface PartnerDto {
  id: number;
  ownerFirstName: string;
  ownerLastName: string;
  createdOn: string;
  contractFileName?: string;
  ownerIdentityFileName?: string;
  registrationDate?: string;
  partnershipEndDate?: string;
  relatedUserId: string;
  phoneNumber?: string;
  isActive: boolean;
  deactivatedAt?: string | null;
  deactivationReason?: string | null;
}

export interface PartnerWalletDto {
  partnerUserId: string;
  balance: number;
  currency: string;
}

export interface LedgerEntryDto {
  id: number;
  type: string;
  amount: number;
  currency: string;
  description?: string;
  createdAt: string;
  referenceId?: string;
}

export interface PayoutDto {
  id: number;
  partnerUserId: string;
  amount: number;
  currency: string;
  status: string;
  reason?: string;
  createdAt: string;
  updatedAt?: string;
}

export async function getPartners(search?: string): Promise<PartnerDto[]> {
  const res = await api.get("/partners", { params: { search } });
  return (res.data ?? []) as PartnerDto[];
}

export async function getPartner(id: number): Promise<PartnerDto> {
  const res = await api.get(`/partners/${id}`);
  return res.data as PartnerDto;
}

export async function getPartnerWallet(partnerId: number): Promise<PartnerWalletDto> {
  const res = await api.get(`/partners/${partnerId}/wallet`);
  return res.data as PartnerWalletDto;
}

export async function getPartnerLedger(partnerId: number, take = 50): Promise<LedgerEntryDto[]> {
  const res = await api.get(`/partners/${partnerId}/ledger`, { params: { take } });
  return (res.data ?? []) as LedgerEntryDto[];
}

export async function getPartnerPayouts(partnerId: number, take = 50): Promise<PayoutDto[]> {
  const res = await api.get(`/partners/${partnerId}/payouts`, { params: { take } });
  return (res.data ?? []) as PayoutDto[];
}

export async function deactivatePartner(id: number, reason: string): Promise<PartnerDto> {
  const res = await api.post(`/partners/${id}/deactivate`, { reason });
  return res.data as PartnerDto;
}

export async function activatePartner(id: number): Promise<PartnerDto> {
  const res = await api.post(`/partners/${id}/activate`);
  return res.data as PartnerDto;
}

export interface FileTemporaryLinkDto {
  fileName: string;
  url: string;
  expiresAtUtc: string;
}

export async function getPartnerFileTemporaryLink(
  partnerId: number,
  fileName: string,
): Promise<FileTemporaryLinkDto> {
  const res = await api.get(`/partners/${partnerId}/files/temporary-link`, {
    params: { fileName },
  });
  return res.data as FileTemporaryLinkDto;
}
