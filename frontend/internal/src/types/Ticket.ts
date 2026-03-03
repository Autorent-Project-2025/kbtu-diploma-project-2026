export interface Ticket {
  id: string;
  fullName: string;
  email: string;
  birthDate: string;
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
