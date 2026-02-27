export interface Office {
  id: string;
  name: string;
  address: string;
  isActive: boolean;
}

export interface Room {
  id: string;
  name: string;
  capacity: number;
  description: string;
  officeId: string;
  isActive: boolean;
}

export interface Booking {
  id: string;
  roomId: string;
  userId: string;
  startTime: Date;
  endTime: Date;
  title: string;
  createdAt: Date;
  isCancelled: boolean;
}
