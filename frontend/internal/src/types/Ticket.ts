export type BookingCompletionPhotoSlot =
  | "front"
  | "back"
  | "side_left"
  | "side_right"
  | "interior";

export interface PartnerCarTicketImageData {
  imageId: string;
  imageUrl: string;
  imageType?: "front" | "back" | "side" | "interior" | "general" | null;
}

export interface BookingCompletionTicketPhotoData {
  slot: BookingCompletionPhotoSlot;
  fileName: string;
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
  transmission?: string | null;
  fuelType?: string | null;
  seats?: number | null;
  doors?: number | null;
  bodyType?: string | null;
  horsepower?: number | null;
  selectedTags?: string[];
  suggestedTags?: string[];
  confirmedTags?: string[];
  ownershipDocumentFileName: string;
  carImages: PartnerCarTicketImageData[];
}

export interface BookingCompletionTicketData extends TicketDataBase {
  $type: "booking-completion";
  bookingId: number;
  plannedStartTime: string;
  plannedEndTime: string;
  tripStartedAt: string;
  tripCompletedAt: string;
  latePenaltyAmount?: number | null;
  damageFineAmount?: number | null;
  completionPhotos: BookingCompletionTicketPhotoData[];
}

export type TicketData =
  | ClientTicketData
  | PartnerTicketData
  | PartnerCarTicketData
  | BookingCompletionTicketData
  | TicketDataBase;

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
  bookingId?: number | null;
  plannedStartTime?: string | null;
  plannedEndTime?: string | null;
  tripStartedAt?: string | null;
  tripCompletedAt?: string | null;
  latePenaltyAmount?: number | null;
  damageFineAmount?: number | null;
  completionPhotos?: BookingCompletionTicketPhotoData[];
  carBrand?: string | null;
  carModel?: string | null;
  carYear?: number | null;
  licensePlate?: string | null;
  carImages?: PartnerCarTicketImageData[];
  status: number;
  decisionReason?: string | null;
  createdAt: string;
  reviewedByManagerId?: string | null;
  reviewedAt?: string | null;
}
