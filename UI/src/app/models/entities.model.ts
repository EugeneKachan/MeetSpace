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

/** DTO returned by GET /api/bookings */
export interface BookingListItem {
  id: string;
  roomId: string;
  roomName: string;
  officeName: string;
  date: string;       // YYYY-MM-DD
  startTime: string;  // HH:mm
  endTime: string;    // HH:mm
  title: string;
  isCancelled: boolean;
}

export interface CreateBookingRequest {
  officeId: string;
  roomId: string;
  date: string;       // YYYY-MM-DD
  startTime: string;  // HH:mm:ss
  endTime: string;    // HH:mm:ss
  title: string;
}

/** Lightweight office returned by GET /api/offices/active (FR-9). */
export interface ActiveOffice {
  id: string;
  name: string;
  address: string;
}

/** Room returned by GET /api/rooms (FR-10). */
export interface RoomListItem {
  id: string;
  name: string;
  capacity: number;
  description: string;
}

export interface RoomFilter {
  officeId: string;
  minCapacity?: number | null;
  date?: string | null;       // ISO date string YYYY-MM-DD
  startTime?: string | null;  // HH:mm
  endTime?: string | null;    // HH:mm
}

/** Generic server-side paged response. */
export interface PagedResult<T> {
  items: T[];
  totalCount: number;
  page: number;
  pageSize: number;
}
