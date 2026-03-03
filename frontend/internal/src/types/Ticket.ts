export interface Ticket {
  id: string;
  ticketType: number;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  birthDate?: string | null;
  phoneNumber: string;
  identityDocumentFileName?: string | null;
  driverLicenseFileName?: string | null;
  avatarUrl?: string | null;
  status: number;
  decisionReason?: string | null;
  createdAt: string;
  reviewedByManagerId?: string | null;
  reviewedAt?: string | null;
}
