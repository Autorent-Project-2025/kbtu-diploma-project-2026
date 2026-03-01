export interface Booking {
  id: number;
  carId: number;
  carBrand: string;
  carModel: string;
  startDate: string;
  endDate: string;
  price: number | null;
  status: BookingStatus;
}

export type BookingStatus =
  | "pending" //
  | "confirmed" //
  | "active" //
  | "completed" //
  | "Canceled"; //

export interface BookingWithCarStatus extends Booking {
  computedStatus: ComputedBookingStatus;
}

export type ComputedBookingStatus =
  | "upcoming" // Предстоящая (еще не началась)
  | "active" // Активная (идет сейчас)
  | "completed" // Завершенная (прошла)
  | "canceled"; // Отмененная
