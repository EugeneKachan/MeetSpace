import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { UsersService } from './users.service';
import { environment } from '../../../environments/environment';
import { User, PagedResult, CreateUserRequest, UpdateUserRequest } from '../../models/entities.model';

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

const MOCK_PAGED_USERS: PagedResult<User> = {
  items: [MOCK_USER],
  totalCount: 1,
  page: 1,
  pageSize: 10,
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

  it('getUsers() should send GET to /api/users with default query params', () => {
    service.getUsers().subscribe((result) => {
      expect(result).toEqual(MOCK_PAGED_USERS);
    });

    const req = httpMock.expectOne(r => r.url === API_URL && r.method === 'GET');
    expect(req.request.params.get('page')).toBe('1');
    expect(req.request.params.get('pageSize')).toBe('10');
    expect(req.request.params.get('search')).toBe('');
    expect(req.request.params.get('sortBy')).toBe('lastName');
    expect(req.request.params.get('sortDir')).toBe('asc');
    req.flush(MOCK_PAGED_USERS);
  });

  it('getUsers() should send custom pagination params when provided', () => {
    service.getUsers(2, 25, 'alice', 'email', 'desc').subscribe();

    const req = httpMock.expectOne(r => r.url === API_URL && r.method === 'GET');
    expect(req.request.params.get('page')).toBe('2');
    expect(req.request.params.get('pageSize')).toBe('25');
    expect(req.request.params.get('search')).toBe('alice');
    expect(req.request.params.get('sortBy')).toBe('email');
    expect(req.request.params.get('sortDir')).toBe('desc');
    req.flush({ items: [], totalCount: 0, page: 2, pageSize: 25 });
  });

  it('getUsers() should return empty items list when server returns zero results', () => {
    const empty: PagedResult<User> = { items: [], totalCount: 0, page: 1, pageSize: 10 };

    service.getUsers().subscribe((result) => {
      expect(result.items).toEqual([]);
      expect(result.totalCount).toBe(0);
    });

    const req = httpMock.expectOne(r => r.url === API_URL && r.method === 'GET');
    req.flush(empty);
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
