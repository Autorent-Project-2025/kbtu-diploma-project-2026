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
