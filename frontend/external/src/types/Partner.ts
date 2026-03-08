import type { BookingStatus } from "./Booking";

export interface Partner {
  id: number;
  ownerFirstName: string;
  ownerLastName: string;
  createdOn: string;
  contractFileName?: string | null;
  ownerIdentityFileName: string;
  registrationDate: string;
  partnershipEndDate: string;
  relatedUserId: string;
  phoneNumber: string;
}

export interface PartnerWallet {
  partnerUserId: string;
  currency: string;
  pendingAmount: number;
  availableAmount: number;
  reservedAmount: number;
  updatedAt: string;
}

export interface PartnerLedgerEntry {
  id: number;
  partnerUserId: string;
  bookingId?: number | null;
  customerPaymentId?: number | null;
  partnerPayoutId?: number | null;
  entryType: string;
  bucket: string;
  amountDelta: number;
  currency: string;
  description?: string | null;
  createdAt: string;
}

export interface PartnerBooking {
  id: number;
  userId: string;
  partnerCarId: number;
  partnerUserId: string;
  carBrand: string;
  carModel: string;
  startTime: string;
  endTime: string;
  priceHour?: number | null;
  totalPrice?: number | null;
  createdAt: string;
  status: BookingStatus;
}
