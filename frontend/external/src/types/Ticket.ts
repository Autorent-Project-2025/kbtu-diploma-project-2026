export interface Ticket {
  id: string;
  fullName: string;
  email: string;
  birthDate: string;
  status: number;
  decisionReason?: string | null;
  createdAt: string;
  reviewedByManagerId?: string | null;
  reviewedAt?: string | null;
}
