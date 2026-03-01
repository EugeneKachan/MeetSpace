import { Injectable } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import {
  Office,
  CreateOfficeRequest,
  UpdateOfficeRequest,
  CreateRoomRequest,
  UpdateRoomRequest,
  PagedResult,
} from '../../models/entities.model';

@Injectable({
  providedIn: 'root',
})
export class OfficesService {
  private readonly officesUrl: string = `${environment.apiUrl}/api/offices`;
  private readonly roomsUrl: string = `${environment.apiUrl}/api/rooms`;

  constructor(private http: HttpClient) {}

  public getOffices(
    page: number = 1,
    pageSize: number = 10,
    search: string = '',
    sortBy: string = 'name',
    sortDir: string = 'asc'
  ): Observable<PagedResult<Office>> {
    const params = new HttpParams()
      .set('page', page.toString())
      .set('pageSize', pageSize.toString())
      .set('search', search)
      .set('sortBy', sortBy)
      .set('sortDir', sortDir);
    return this.http.get<PagedResult<Office>>(this.officesUrl, { params });
  }

  public createOffice(request: CreateOfficeRequest): Observable<string> {
    return this.http.post<string>(this.officesUrl, request);
  }

  public updateOffice(id: string, request: UpdateOfficeRequest): Observable<void> {
    return this.http.put<void>(`${this.officesUrl}/${id}`, request);
  }

  public deactivateOffice(id: string): Observable<void> {
    return this.http.delete<void>(`${this.officesUrl}/${id}`);
  }

  public assignManager(officeId: string, userId: string): Observable<void> {
    return this.http.post<void>(`${this.officesUrl}/${officeId}/managers`, { userId });
  }

  public removeManager(officeId: string, userId: string): Observable<void> {
    return this.http.delete<void>(`${this.officesUrl}/${officeId}/managers/${userId}`);
  }

  public createRoom(request: CreateRoomRequest): Observable<string> {
    return this.http.post<string>(this.roomsUrl, request);
  }

  public updateRoom(id: string, request: UpdateRoomRequest): Observable<void> {
    return this.http.put<void>(`${this.roomsUrl}/${id}`, request);
  }

  public deactivateRoom(id: string): Observable<void> {
    return this.http.delete<void>(`${this.roomsUrl}/${id}`);
  }
}
