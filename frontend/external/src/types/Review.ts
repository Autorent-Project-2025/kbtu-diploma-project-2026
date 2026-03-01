export interface Review {
  id: number;
  userId: number;
  userName: string;
  userAvatar?: string;
  carId: number;
  rating: number; // 1-5
  comment: string;
  createdAt: string;
}

export interface CarRating {
  averageRating: number;
  totalReviews: number;
  distribution: {
    5: number;
    4: number;
    3: number;
    2: number;
    1: number;
  };
}
