export interface Car {
  id: number;
  brand: string;
  model: string;
  year: number;
  priceHour: number | null;
  priceDay: number | null;
  imageUrl: string | null;
  rating: number | null;
  description: string | null;
}

type BookingSelectionBase = Omit<Car, "id">;

export interface BookingBusySlot {
  bookingId: number;
  startTime: string;
  endTime: string;
  status: string | null;
}

export type BookingModelSelection = BookingSelectionBase & {
  kind: "model";
  modelId: number;
};

export type BookingPartnerCarSelection = BookingSelectionBase & {
  kind: "partnerCar";
  partnerCarId: number;
  partnerUserId: string;
  carrierName: string | null;
  licensePlate: string | null;
  busySlots: BookingBusySlot[];
};

export type BookingSelection =
  | BookingModelSelection
  | BookingPartnerCarSelection;

// Спецификации автомобиля
export interface CarSpecifications {
  engine?: string;
  transmission?: string;
  fuelType?: string;
  seats?: number;
  doors?: number;
  color?: string;
  mileage?: number;
}

export interface CarDetails extends Car {
  comments: CarComment[];
  // Дополнительные поля
  images?: string[]; // Массив дополнительных изображений
  features?: string[]; // Массив особенностей
  specifications?: CarSpecifications; // Характеристики
  engine?: string;
  transmission?: string;
  fuelType?: string;
  seats?: number;
  doors?: number;
  color?: string;
  mileage?: number;
}

export interface CarComment {
  id: number;
  userId: number;
  userName: string;
  carId: number;
  content: string;
  rating: number;
  created_On: string; // После camelCase преобразования будет created_On
}

// Для создания комментария
export interface CreateCommentDto {
  carId: number;
  content: string;
  rating: number;
}
