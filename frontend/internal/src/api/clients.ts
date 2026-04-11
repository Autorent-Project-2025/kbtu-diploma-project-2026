import api from "./axios";

export interface ClientDto {
  id: number;
  firstName: string;
  lastName: string;
  createdOn: string;
  birthDate: string;
  identityDocumentFileName?: string;
  driverLicenseFileName?: string;
  relatedUserId: string;
  phoneNumber: string;
  avatarUrl?: string;
  avatarImageId?: string;
  bookingActionsBlocked: boolean;
  bookingBlockReason?: string;
  bookingBlockedAt?: string;
}

export interface ClientUpdatePayload {
  firstName: string;
  lastName: string;
  birthDate: string;
  phoneNumber: string;
  relatedUserId: string;
  identityDocumentFileName?: string;
  driverLicenseFileName?: string;
  avatarUrl?: string;
  avatarImageId?: string;
}

export async function getClients(search?: string): Promise<ClientDto[]> {
  const res = await api.get("/clients", { params: { search } });
  return (res.data ?? []) as ClientDto[];
}

export async function getClient(id: number): Promise<ClientDto> {
  const res = await api.get(`/clients/${id}`);
  return res.data as ClientDto;
}

export async function updateClient(
  id: number,
  data: ClientUpdatePayload,
): Promise<ClientDto> {
  const res = await api.put(`/clients/${id}`, data);
  return res.data as ClientDto;
}

export async function deleteClient(id: number): Promise<void> {
  await api.delete(`/clients/${id}`);
}

export async function blockClient(relatedUserId: string, reason?: string): Promise<ClientDto> {
  const res = await api.post(`/clients/by-user/${relatedUserId}/booking-access/block`, { reason });
  return res.data as ClientDto;
}

export async function unblockClient(relatedUserId: string): Promise<ClientDto> {
  const res = await api.post(`/clients/by-user/${relatedUserId}/booking-access/unblock`);
  return res.data as ClientDto;
}
