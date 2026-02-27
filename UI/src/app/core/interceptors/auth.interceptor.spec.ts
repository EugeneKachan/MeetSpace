import { TestBed } from '@angular/core/testing';
import {
  HttpClientTestingModule,
  HttpTestingController
} from '@angular/common/http/testing';
import {
  HTTP_INTERCEPTORS,
  HttpClient,
  HttpErrorResponse
} from '@angular/common/http';
import { RouterTestingModule } from '@angular/router/testing';
import { AuthInterceptor } from './auth.interceptor';
import { AuthService } from '../services/auth.service';
import { environment } from '../../../environments/environment';

describe('AuthInterceptor', () => {
  let httpMock: HttpTestingController;
  let httpClient: HttpClient;
  let authServiceSpy: jasmine.SpyObj<AuthService>;

  beforeEach(() => {
    authServiceSpy = jasmine.createSpyObj<AuthService>('AuthService', [
      'getToken',
      'logout',
      'isAuthenticated',
      'hasRole',
      'getCurrentUser'
    ]);

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule, RouterTestingModule],
      providers: [
        { provide: AuthService, useValue: authServiceSpy },
        {
          provide: HTTP_INTERCEPTORS,
          useClass: AuthInterceptor,
          multi: true
        }
      ]
    });

    httpMock = TestBed.inject(HttpTestingController);
    httpClient = TestBed.inject(HttpClient);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should add Authorization header for API URL when token exists', () => {
    authServiceSpy.getToken.and.returnValue('test-token');

    httpClient.get(`${environment.apiUrl}/api/users`).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/api/users`);
    expect(req.request.headers.get('Authorization')).toBe('Bearer test-token');
    req.flush([]);
  });

  it('should NOT add Authorization header when no token', () => {
    authServiceSpy.getToken.and.returnValue(null);

    httpClient.get(`${environment.apiUrl}/api/users`).subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/api/users`);
    expect(req.request.headers.has('Authorization')).toBeFalse();
    req.flush([]);
  });

  it('should NOT add Authorization header for external URLs even with token', () => {
    authServiceSpy.getToken.and.returnValue('test-token');

    httpClient.get('https://external-api.com/data').subscribe();

    const req = httpMock.expectOne('https://external-api.com/data');
    expect(req.request.headers.has('Authorization')).toBeFalse();
    req.flush({});
  });

  it('should call authService.logout() on 401 response', () => {
    authServiceSpy.getToken.and.returnValue('expired-token');
    authServiceSpy.logout.and.stub();

    httpClient.get(`${environment.apiUrl}/api/users`).subscribe({
      error: () => { /* expected */ }
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/api/users`);
    req.flush('Unauthorized', { status: 401, statusText: 'Unauthorized' });

    expect(authServiceSpy.logout).toHaveBeenCalledTimes(1);
  });

  it('should re-throw the error after 401 handling', (done: DoneFn) => {
    authServiceSpy.getToken.and.returnValue('expired-token');
    authServiceSpy.logout.and.stub();

    httpClient.get(`${environment.apiUrl}/api/users`).subscribe({
      error: (err: HttpErrorResponse) => {
        expect(err.status).toBe(401);
        done();
      }
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/api/users`);
    req.flush('Unauthorized', { status: 401, statusText: 'Unauthorized' });
  });

  it('should NOT call logout() for non-401 errors', () => {
    authServiceSpy.getToken.and.returnValue('token');
    authServiceSpy.logout.and.stub();

    httpClient.get(`${environment.apiUrl}/api/users`).subscribe({
      error: () => { /* expected */ }
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/api/users`);
    req.flush('Server Error', { status: 500, statusText: 'Internal Server Error' });

    expect(authServiceSpy.logout).not.toHaveBeenCalled();
  });
});
