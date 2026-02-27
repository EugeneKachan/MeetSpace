import { TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { RouterTestingModule } from '@angular/router/testing';
import { Router } from '@angular/router';
import { AuthService } from './auth.service';
import { environment } from '../../../environments/environment';

// ---------------------------------------------------------------------------
// JWT test helpers
// ---------------------------------------------------------------------------

function b64url(obj: object): string {
  return btoa(JSON.stringify(obj))
    .replace(/=/g, '')
    .replace(/\+/g, '-')
    .replace(/\//g, '_');
}

function createJwt(
  claims: Record<string, unknown>,
  expiredOffsetSeconds: number = 3600
): string {
  const header = b64url({ alg: 'HS256', typ: 'JWT' });
  const payload = b64url({
    ...claims,
    exp: Math.floor(Date.now() / 1000) + expiredOffsetSeconds,
    iat: Math.floor(Date.now() / 1000) - 10
  });
  return `${header}.${payload}.fakesig`;
}

const VALID_CLAIMS = {
  sub: 'user-123',
  email: 'john.doe@example.com',
  given_name: 'John',
  family_name: 'Doe',
  role: 'Admin',
  name: 'john.doe@example.com'
};

const VALID_JWT = createJwt(VALID_CLAIMS);
const EXPIRED_JWT = createJwt(VALID_CLAIMS, -3600);

// ---------------------------------------------------------------------------
// Tests
// ---------------------------------------------------------------------------

describe('AuthService', () => {
  let service: AuthService;
  let httpMock: HttpTestingController;
  let router: Router;

  beforeEach(() => {
    localStorage.clear();

    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule, RouterTestingModule]
    });

    service = TestBed.inject(AuthService);
    httpMock = TestBed.inject(HttpTestingController);
    router = TestBed.inject(Router);
  });

  afterEach(() => {
    httpMock.verify();
    localStorage.clear();
  });

  // -------------------------------------------------------------------------
  // Construction
  // -------------------------------------------------------------------------

  it('should create', () => {
    expect(service).toBeTruthy();
  });

  it('currentUser$ emits null when no token in localStorage', (done: DoneFn) => {
    service.currentUser$.subscribe((user) => {
      expect(user).toBeNull();
      done();
    });
  });

  // -------------------------------------------------------------------------
  // login()
  // -------------------------------------------------------------------------

  it('login() should POST to the token endpoint and store token', () => {
    const navigateSpy = spyOn(router, 'navigate');

    service.login('john@example.com', 'password123').subscribe({
      next: (response) => {
        expect(response.access_token).toBe(VALID_JWT);
      }
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/connect/token`);
    expect(req.request.method).toBe('POST');
    expect(req.request.headers.get('Content-Type')).toBe('application/x-www-form-urlencoded');

    req.flush({
      access_token: VALID_JWT,
      token_type: 'Bearer',
      expires_in: 3600
    });

    expect(localStorage.getItem('access_token')).toBe(VALID_JWT);
    expect(navigateSpy).not.toHaveBeenCalled();
  });

  it('login() should store refresh_token when present', () => {
    service.login('john@example.com', 'pass').subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/connect/token`);
    req.flush({
      access_token: VALID_JWT,
      token_type: 'Bearer',
      expires_in: 3600,
      refresh_token: 'refresh-abc'
    });

    expect(localStorage.getItem('refresh_token')).toBe('refresh-abc');
  });

  it('login() should update currentUser$ with decoded user on success', (done: DoneFn) => {
    service.login('john@example.com', 'pass').subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/connect/token`);
    req.flush({ access_token: VALID_JWT, token_type: 'Bearer', expires_in: 3600 });

    service.currentUser$.subscribe((user) => {
      if (user) {
        expect(user.id).toBe('user-123');
        expect(user.email).toBe('john.doe@example.com');
        expect(user.role).toBe('Admin');
        done();
      }
    });
  });

  it('login() should throw mapped error on HTTP error with error_description', (done: DoneFn) => {
    service.login('bad@example.com', 'wrong').subscribe({
      error: (err: Error) => {
        expect(err.message).toBe('invalid_grant');
        done();
      }
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/connect/token`);
    req.flush(
      { error: 'invalid_grant', error_description: 'invalid_grant' },
      { status: 400, statusText: 'Bad Request' }
    );
  });

  it('login() should fall back to error field when error_description absent', (done: DoneFn) => {
    service.login('bad@example.com', 'wrong').subscribe({
      error: (err: Error) => {
        expect(err.message).toBe('unauthorized_client');
        done();
      }
    });

    const req = httpMock.expectOne(`${environment.apiUrl}/connect/token`);
    req.flush(
      { error: 'unauthorized_client' },
      { status: 400, statusText: 'Bad Request' }
    );
  });

  // -------------------------------------------------------------------------
  // logout()
  // -------------------------------------------------------------------------

  it('logout() should clear localStorage and navigate to /login', () => {
    localStorage.setItem('access_token', VALID_JWT);
    localStorage.setItem('refresh_token', 'tok');

    const navigateSpy = spyOn(router, 'navigate');

    service.logout();

    expect(localStorage.getItem('access_token')).toBeNull();
    expect(localStorage.getItem('refresh_token')).toBeNull();
    expect(navigateSpy).toHaveBeenCalledWith(['/login']);
  });

  it('logout() should emit null on currentUser$', (done: DoneFn) => {
    localStorage.setItem('access_token', VALID_JWT);
    const navigateSpy = spyOn(router, 'navigate');

    service.logout();
    navigateSpy.and.stub();

    service.currentUser$.subscribe((user) => {
      expect(user).toBeNull();
      done();
    });
  });

  // -------------------------------------------------------------------------
  // getToken()
  // -------------------------------------------------------------------------

  it('getToken() should return token from localStorage', () => {
    localStorage.setItem('access_token', 'abc');
    expect(service.getToken()).toBe('abc');
  });

  it('getToken() should return null when no token', () => {
    expect(service.getToken()).toBeNull();
  });

  // -------------------------------------------------------------------------
  // isAuthenticated()
  // -------------------------------------------------------------------------

  it('isAuthenticated() should return false when no token', () => {
    expect(service.isAuthenticated()).toBeFalse();
  });

  it('isAuthenticated() should return false for expired token', () => {
    localStorage.setItem('access_token', EXPIRED_JWT);
    expect(service.isAuthenticated()).toBeFalse();
  });

  it('isAuthenticated() should return true for valid token', () => {
    localStorage.setItem('access_token', VALID_JWT);
    expect(service.isAuthenticated()).toBeTrue();
  });

  it('isAuthenticated() should return false and clear invalid token string', () => {
    localStorage.setItem('access_token', 'not-a-jwt');
    expect(service.isAuthenticated()).toBeFalse();
    expect(localStorage.getItem('access_token')).toBeNull();
  });

  // -------------------------------------------------------------------------
  // getCurrentUser()
  // -------------------------------------------------------------------------

  it('getCurrentUser() should return null when not logged in', () => {
    expect(service.getCurrentUser()).toBeNull();
  });

  // -------------------------------------------------------------------------
  // hasRole()
  // -------------------------------------------------------------------------

  it('hasRole() should return false when no user', () => {
    expect(service.hasRole(['Admin'])).toBeFalse();
  });

  it('hasRole() should return true when user has the role', (done: DoneFn) => {
    service.login('john@example.com', 'pass').subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/connect/token`);
    req.flush({ access_token: VALID_JWT, token_type: 'Bearer', expires_in: 3600 });

    service.currentUser$.subscribe((user) => {
      if (user) {
        expect(service.hasRole(['Admin'])).toBeTrue();
        expect(service.hasRole(['Employee', 'Admin'])).toBeTrue();
        done();
      }
    });
  });

  it('hasRole() should return false when user does not have the role', (done: DoneFn) => {
    service.login('john@example.com', 'pass').subscribe();

    const req = httpMock.expectOne(`${environment.apiUrl}/connect/token`);
    req.flush({ access_token: VALID_JWT, token_type: 'Bearer', expires_in: 3600 });

    service.currentUser$.subscribe((user) => {
      if (user) {
        expect(service.hasRole(['Employee', 'OfficeManager'])).toBeFalse();
        done();
      }
    });
  });

  // -------------------------------------------------------------------------
  // parseStoredToken (via constructor behaviour)
  // -------------------------------------------------------------------------

  it('should auto-decode stored valid token on construction', () => {
    localStorage.setItem('access_token', VALID_JWT);

    // Re-create the TestBed to trigger fresh service construction with a token in storage
    TestBed.resetTestingModule();
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule, RouterTestingModule]
    });

    const freshService = TestBed.inject(AuthService);
    expect(freshService.getCurrentUser()).not.toBeNull();
  });
});
