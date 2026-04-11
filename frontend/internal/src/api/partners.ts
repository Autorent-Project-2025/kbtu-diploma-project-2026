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

export async function getPartners(): Promise<PartnerDto[]> {
  const res = await api.get("/partners");
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
