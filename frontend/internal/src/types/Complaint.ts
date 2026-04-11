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

export interface ComplaintAttachment {
  id: string;
  fileName: string;
  originalFileName: string;
  fileType: string;
  attachmentPhase: number; // 1=Creation, 2=InfoResponse
  createdAt: string;
}

export interface Complaint {
  id: string;
  bookingId: number;
  chargeId: number | null;
  reporterActorType: number; // 1=Client, 2=Partner
  targetType: number; // 1=Partner, 2=Client
  category: number; // 1=CarCondition, 2=LateHandover, 3=ServiceQuality, 4=SafetyIssue, 5=ClientMisbehavior, 99=Other
  status: number; // 1=New, 2=InReview, 3=AwaitingResponse, 4=Resolved, 5=Rejected
  priority: number; // 1=Normal, 2=High, 3=Urgent
  createdByUserId: string;
  subject: string;
  description: string;
  assignedToManagerId: string | null;
  infoRequestText: string | null;
  infoRequestAt: string | null;
  infoRequestBy: string | null;
  infoResponseText: string | null;
  infoResponseAt: string | null;
  managerNote: string | null;
  managerNoteAt: string | null;
  managerNoteBy: string | null;
  resolutionType: number | null; // 1=InFavorOfReporter, 2=InFavorOfCounterparty, 3=CompromiseReached, 4=NoActionRequired
  resolutionNote: string | null;
  resolvedAt: string | null;
  resolvedBy: string | null;
  rejectionReason: string | null;
  rejectedAt: string | null;
  rejectedBy: string | null;
  snapshotData: BookingSnapshot;
  createdAt: string;
  updatedAt: string;
  attachments: ComplaintAttachment[];
}
