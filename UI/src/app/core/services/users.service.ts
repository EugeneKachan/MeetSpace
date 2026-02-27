import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { User, CreateUserRequest, UpdateUserRequest } from '../../models/entities.model';

@Injectable({
  providedIn: 'root'
})
export class UsersService {
  private readonly apiUrl = `${environment.apiUrl}/api/users`;

  constructor(private http: HttpClient) {}

  getUsers(): Observable<User[]> {
    return this.http.get<User[]>(this.apiUrl);
  }

  createUser(request: CreateUserRequest): Observable<User> {
    return this.http.post<User>(this.apiUrl, request);
  }

  updateUser(id: string, request: UpdateUserRequest): Observable<User> {
    return this.http.put<User>(`${this.apiUrl}/${id}`, { id, ...request });
  }
}
