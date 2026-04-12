import api from "./axios";

export interface BookingCharge {
  id: number;
  bookingId: number;
  userId: string;
  partnerUserId: string;
  chargeType: string;
  amount: number;
  partnerShareAmount: number;
  currency: string;
  status: string;
  description: string | null;
  createdAt: string;
  updatedAt: string;
  paidAt: string | null;
  canceledAt: string | null;
  refundedAt: string | null;
}

export async function getBookingCharges(bookingId: number): Promise<BookingCharge[]> {
  const res = await api.get(`/payments/view/bookings/${bookingId}/charges`);
  return (res.data ?? []) as BookingCharge[];
}
