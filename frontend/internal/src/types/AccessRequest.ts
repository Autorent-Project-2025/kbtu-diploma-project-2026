export interface AccessRequest {
  id: string;
  complaintId: string;
  bookingId: number;
  requestedByManagerId: string;
  status: number; // 1=Pending, 2=Approved, 3=Rejected, 4=Expired, 5=Revoked
  reason: string;
  requestedAt: string;
  reviewedBySupermanagerId: string | null;
  reviewedAt: string | null;
  decisionNote: string | null;
  expiresAt: string | null;
}

export interface BookingReview {
  bookingId: number;
  status: string;
  carBrand: string;
  carModel: string;
  coverImageUrl: string | null;
  partnerName: string | null;
  startTime: string;
  endTime: string;
  totalPrice: number | null;
  tripStartedAt: string | null;
  complaintId: string;
  complaintSubject: string;
}
