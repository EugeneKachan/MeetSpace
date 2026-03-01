export interface User {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
  role: string;
  isActive: boolean;
  createdAt: string;
}

export interface CreateUserRequest {
  firstName: string;
  lastName: string;
  email: string;
  password: string;
  role: string;
  isActive: boolean;
}

export interface UpdateUserRequest {
  firstName: string;
  lastName: string;
  email: string;
  role: string;
  isActive: boolean;
}

export interface Office {
  id: string;
  name: string;
  address: string;
  isActive: boolean;
  rooms: Room[];
  managers: ManagerSummary[];
}

export interface ManagerSummary {
  id: string;
  firstName: string;
  lastName: string;
  email: string;
}

export interface Room {
  id: string;
  name: string;
  capacity: number;
  description: string;
  officeId: string;
  isActive: boolean;
}

export interface CreateOfficeRequest {
  name: string;
  address: string;
  isActive: boolean;
  rooms: CreateRoomForOfficeRequest[];
}

export interface UpdateOfficeRequest {
  id: string;
  name: string;
  address: string;
  isActive: boolean;
}

export interface CreateRoomRequest {
  officeId: string;
  name: string;
  capacity: number;
  description: string;
}

export interface CreateRoomForOfficeRequest {
  name: string;
  capacity: number;
  description: string;
}

export interface UpdateRoomRequest {
  id: string;
  name: string;
  capacity: number;
  description: string;
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
