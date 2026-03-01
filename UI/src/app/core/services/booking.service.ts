import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { ActiveOffice, BookingListItem, CreateBookingRequest, RoomFilter, RoomListItem } from '../../models/entities.model';

@Injectable({
  providedIn: 'root',
})
export class BookingService {
  private readonly officesUrl: string = `${environment.apiUrl}/api/offices`;
  private readonly roomsUrl: string = `${environment.apiUrl}/api/rooms`;
  private readonly bookingsUrl: string = `${environment.apiUrl}/api/bookings`;

  constructor(private readonly http: HttpClient) {}

  public getActiveOffices(): Observable<ActiveOffice[]> {
    return this.http.get<ActiveOffice[]>(`${this.officesUrl}/active`);
  }

  public getRooms(filter: RoomFilter): Observable<RoomListItem[]> {
    let params = new HttpParams().set('officeId', filter.officeId);
    if (filter.minCapacity != null) {
      params = params.set('minCapacity', filter.minCapacity);
    }
    if (filter.date) {
      params = params.set('date', filter.date);
    }
    if (filter.startTime) {
      params = params.set('startTime', filter.startTime);
    }
    if (filter.endTime) {
      params = params.set('endTime', filter.endTime);
    }
    return this.http.get<RoomListItem[]>(this.roomsUrl, { params });
  }

  public getUserBookings(): Observable<BookingListItem[]> {
    return this.http.get<BookingListItem[]>(this.bookingsUrl);
  }

  public createBooking(request: CreateBookingRequest): Observable<string> {
    return this.http.post<string>(this.bookingsUrl, request);
  }

  public cancelBooking(id: string): Observable<void> {
    return this.http.delete<void>(`${this.bookingsUrl}/${id}`);
  }
}
