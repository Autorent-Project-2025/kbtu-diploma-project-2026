export interface Complaint {
  id: string;
  bookingId: number;
  reporterActorType: number;
  targetType: number;
  category: number;
  status: number;
  priority: number;
  createdByUserId: string;
  subject: string;
  description: string;
  infoRequestText: string | null;
  infoRequestAt: string | null;
  infoResponseText: string | null;
  infoResponseAt: string | null;
  resolutionType: number | null;
  resolutionNote: string | null;
  resolvedAt: string | null;
  rejectionReason: string | null;
  rejectedAt: string | null;
  snapshotData: BookingSnapshot;
  createdAt: string;
  updatedAt: string;
  attachments: ComplaintAttachment[];
}

export interface ComplaintAttachment {
  id: string;
  fileName: string;
  originalFileName: string;
  fileType: string;
  attachmentPhase: number;
  createdAt: string;
}

export interface BookingSnapshot {
  bookingId: number;
  status: string;
  carBrand: string;
  carModel: string;
  partnerName: string | null;
  coverImageUrl: string | null;
  startTime: string;
  endTime: string;
  totalPrice: number | null;
  reporterFullName: string;
  counterpartyName: string;
  counterpartyUserId: string;
}
