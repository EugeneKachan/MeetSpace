import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { OfficesService } from './offices.service';
import { environment } from '../../../environments/environment';
import {
  Office,
  PagedResult,
  CreateOfficeRequest,
  UpdateOfficeRequest,
  CreateRoomRequest,
  UpdateRoomRequest,
} from '../../models/entities.model';

const OFFICES_URL = `${environment.apiUrl}/api/offices`;
const ROOMS_URL = `${environment.apiUrl}/api/rooms`;

const MOCK_OFFICE: Office = {
  id: 'office-1',
  name: 'London HQ',
  address: '10 Downing St',
  isActive: true,
  rooms: [],
  managers: [],
};

const MOCK_PAGED_OFFICES: PagedResult<Office> = {
  items: [MOCK_OFFICE],
  totalCount: 1,
  page: 1,
  pageSize: 10,
};

describe('OfficesService', () => {
  let service: OfficesService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
    });

    service = TestBed.inject(OfficesService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create', () => {
    expect(service).toBeTruthy();
  });

  // -------------------------------------------------------------------------
  // getOffices()
  // -------------------------------------------------------------------------

  it('getOffices() should send GET to /api/offices with default query params', () => {
    service.getOffices().subscribe((result) => {
      expect(result).toEqual(MOCK_PAGED_OFFICES);
    });

    const req = httpMock.expectOne(r => r.url === OFFICES_URL && r.method === 'GET');
    expect(req.request.params.get('page')).toBe('1');
    expect(req.request.params.get('pageSize')).toBe('10');
    expect(req.request.params.get('search')).toBe('');
    expect(req.request.params.get('sortBy')).toBe('name');
    expect(req.request.params.get('sortDir')).toBe('asc');
    req.flush(MOCK_PAGED_OFFICES);
  });

  it('getOffices() should send custom pagination params when provided', () => {
    service.getOffices(2, 25, 'London', 'address', 'desc').subscribe();

    const req = httpMock.expectOne(r => r.url === OFFICES_URL && r.method === 'GET');
    expect(req.request.params.get('page')).toBe('2');
    expect(req.request.params.get('pageSize')).toBe('25');
    expect(req.request.params.get('search')).toBe('London');
    expect(req.request.params.get('sortBy')).toBe('address');
    expect(req.request.params.get('sortDir')).toBe('desc');
    req.flush({ items: [], totalCount: 0, page: 2, pageSize: 25 });
  });

  it('getOffices() should return empty items list when server returns zero results', () => {
    const empty: PagedResult<Office> = { items: [], totalCount: 0, page: 1, pageSize: 10 };

    service.getOffices().subscribe((result) => {
      expect(result.items).toEqual([]);
      expect(result.totalCount).toBe(0);
    });

    const req = httpMock.expectOne(r => r.url === OFFICES_URL && r.method === 'GET');
    req.flush(empty);
  });

  // -------------------------------------------------------------------------
  // createOffice()
  // -------------------------------------------------------------------------

  it('createOffice() should send POST to /api/offices and return created id', () => {
    const request: CreateOfficeRequest = {
      name: 'New Office',
      address: '1 Main St',
      isActive: true,
      rooms: [],
    };
    const newId = 'new-office-id';

    service.createOffice(request).subscribe((result) => {
      expect(result).toBe(newId);
    });

    const req = httpMock.expectOne(OFFICES_URL);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(request);
    req.flush(newId);
  });

  // -------------------------------------------------------------------------
  // updateOffice()
  // -------------------------------------------------------------------------

  it('updateOffice() should send PUT to /api/offices/{id}', () => {
    const id = 'office-1';
    const request: UpdateOfficeRequest = {
      id,
      name: 'Updated Name',
      address: 'New Address',
      isActive: true,
    };

    service.updateOffice(id, request).subscribe();

    const req = httpMock.expectOne(`${OFFICES_URL}/${id}`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(request);
    req.flush(null);
  });

  // -------------------------------------------------------------------------
  // deactivateOffice()
  // -------------------------------------------------------------------------

  it('deactivateOffice() should send DELETE to /api/offices/{id}', () => {
    const id = 'office-1';

    service.deactivateOffice(id).subscribe();

    const req = httpMock.expectOne(`${OFFICES_URL}/${id}`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });

  // -------------------------------------------------------------------------
  // assignManager()
  // -------------------------------------------------------------------------

  it('assignManager() should send POST to /api/offices/{officeId}/managers with userId body', () => {
    const officeId = 'office-1';
    const userId = 'user-42';

    service.assignManager(officeId, userId).subscribe();

    const req = httpMock.expectOne(`${OFFICES_URL}/${officeId}/managers`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual({ userId });
    req.flush(null);
  });

  // -------------------------------------------------------------------------
  // removeManager()
  // -------------------------------------------------------------------------

  it('removeManager() should send DELETE to /api/offices/{officeId}/managers/{userId}', () => {
    const officeId = 'office-1';
    const userId = 'user-42';

    service.removeManager(officeId, userId).subscribe();

    const req = httpMock.expectOne(`${OFFICES_URL}/${officeId}/managers/${userId}`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });

  // -------------------------------------------------------------------------
  // createRoom()
  // -------------------------------------------------------------------------

  it('createRoom() should send POST to /api/rooms and return created id', () => {
    const request: CreateRoomRequest = {
      officeId: 'office-1',
      name: 'Conference Room A',
      capacity: 10,
      description: 'A large meeting room',
    };
    const newId = 'room-id-1';

    service.createRoom(request).subscribe((result) => {
      expect(result).toBe(newId);
    });

    const req = httpMock.expectOne(ROOMS_URL);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(request);
    req.flush(newId);
  });

  // -------------------------------------------------------------------------
  // updateRoom()
  // -------------------------------------------------------------------------

  it('updateRoom() should send PUT to /api/rooms/{id}', () => {
    const id = 'room-id-1';
    const request: UpdateRoomRequest = {
      id,
      name: 'Updated Room',
      capacity: 15,
      description: 'Updated description',
      isActive: true,
    };

    service.updateRoom(id, request).subscribe();

    const req = httpMock.expectOne(`${ROOMS_URL}/${id}`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(request);
    req.flush(null);
  });

  // -------------------------------------------------------------------------
  // deactivateRoom()
  // -------------------------------------------------------------------------

  it('deactivateRoom() should send DELETE to /api/rooms/{id}', () => {
    const id = 'room-id-1';

    service.deactivateRoom(id).subscribe();

    const req = httpMock.expectOne(`${ROOMS_URL}/${id}`);
    expect(req.request.method).toBe('DELETE');
    req.flush(null);
  });
});
