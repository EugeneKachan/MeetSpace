import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { UsersService } from './users.service';
import { environment } from '../../../environments/environment';
import { User, CreateUserRequest, UpdateUserRequest } from '../../models/entities.model';

const API_URL = `${environment.apiUrl}/api/users`;

const MOCK_USER: User = {
  id: 'user-1',
  firstName: 'Jane',
  lastName: 'Smith',
  email: 'jane.smith@example.com',
  role: 'Employee',
  isActive: true,
  createdAt: '2024-01-01T00:00:00Z'
};

describe('UsersService', () => {
  let service: UsersService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule]
    });

    service = TestBed.inject(UsersService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create', () => {
    expect(service).toBeTruthy();
  });

  // -------------------------------------------------------------------------
  // getUsers()
  // -------------------------------------------------------------------------

  it('getUsers() should send GET to /api/users and return users', () => {
    const users: User[] = [MOCK_USER];

    service.getUsers().subscribe((result) => {
      expect(result.length).toBe(1);
      expect(result[0]).toEqual(MOCK_USER);
    });

    const req = httpMock.expectOne(API_URL);
    expect(req.request.method).toBe('GET');
    req.flush(users);
  });

  it('getUsers() should return empty array when server returns []', () => {
    service.getUsers().subscribe((result) => {
      expect(result).toEqual([]);
    });

    const req = httpMock.expectOne(API_URL);
    req.flush([]);
  });

  // -------------------------------------------------------------------------
  // createUser()
  // -------------------------------------------------------------------------

  it('createUser() should send POST with the request body and return created user', () => {
    const request: CreateUserRequest = {
      firstName: 'Jane',
      lastName: 'Smith',
      email: 'jane@example.com',
      password: 'Password1!',
      role: 'Employee',
      isActive: true
    };

    service.createUser(request).subscribe((result) => {
      expect(result).toEqual(MOCK_USER);
    });

    const req = httpMock.expectOne(API_URL);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(request);
    req.flush(MOCK_USER);
  });

  // -------------------------------------------------------------------------
  // updateUser()
  // -------------------------------------------------------------------------

  it('updateUser() should send PUT to /api/users/{id} with merged body', () => {
    const id = 'user-1';
    const request: UpdateUserRequest = {
      firstName: 'Jane',
      lastName: 'Updated',
      email: 'jane@example.com',
      role: 'OfficeManager',
      isActive: false
    };

    const updatedUser: User = { ...MOCK_USER, ...request };

    service.updateUser(id, request).subscribe((result) => {
      expect(result).toEqual(updatedUser);
    });

    const req = httpMock.expectOne(`${API_URL}/${id}`);
    expect(req.request.method).toBe('PUT');
    // Body must include the id merged in
    expect(req.request.body).toEqual({ id, ...request });
    req.flush(updatedUser);
  });
});
