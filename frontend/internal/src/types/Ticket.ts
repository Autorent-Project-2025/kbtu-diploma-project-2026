export interface PartnerCarTicketImageData {
  imageId: string;
  imageUrl: string;
}

export interface TicketDataBase {
  $type?: string;
  firstName?: string;
  lastName?: string;
  fullName?: string;
  phoneNumber?: string;
  identityDocumentFileName?: string | null;
  decisionReason?: string | null;
  reviewedByManagerId?: string | null;
  reviewedAt?: string | null;
}

export interface ClientTicketData extends TicketDataBase {
  $type: "client";
  birthDate: string;
  driverLicenseFileName?: string | null;
  avatarUrl?: string | null;
}

export interface PartnerTicketData extends TicketDataBase {
  $type: "partner";
  companyName: string;
  contactEmail: string;
}

export interface PartnerCarTicketData extends TicketDataBase {
  $type: "partner-car";
  relatedPartnerUserId: string;
  carBrand: string;
  carModel: string;
  carYear?: number | null;
  licensePlate: string;
  priceHour?: number | null;
  priceDay?: number | null;
  ownershipDocumentFileName: string;
  carImages: PartnerCarTicketImageData[];
}

export type TicketData = ClientTicketData | PartnerTicketData | PartnerCarTicketData | TicketDataBase;

export interface Ticket {
  id: string;
  ticketType: number;
  data: TicketData;
  firstName: string;
  lastName: string;
  fullName: string;
  email: string;
  birthDate?: string | null;
  phoneNumber: string;
  identityDocumentFileName?: string | null;
  driverLicenseFileName?: string | null;
  ownershipDocumentFileName?: string | null;
  avatarUrl?: string | null;
  relatedPartnerUserId?: string | null;
  carBrand?: string | null;
  carModel?: string | null;
  carYear?: number | null;
  licensePlate?: string | null;
  priceHour?: number | null;
  priceDay?: number | null;
  carImages?: PartnerCarTicketImageData[];
  status: number;
  decisionReason?: string | null;
  createdAt: string;
  reviewedByManagerId?: string | null;
  reviewedAt?: string | null;
}
