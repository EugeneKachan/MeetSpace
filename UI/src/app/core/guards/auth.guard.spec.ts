import { TestBed } from '@angular/core/testing';
import { Router, ActivatedRouteSnapshot } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { authGuard } from './auth.guard';
import { AuthService } from '../services/auth.service';

function buildRoute(roles?: string[]): ActivatedRouteSnapshot {
  const snapshot = new ActivatedRouteSnapshot();
  (snapshot as any).data = roles ? { roles } : {};
  return snapshot;
}

describe('authGuard', () => {
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let router: Router;

  beforeEach(() => {
    authServiceSpy = jasmine.createSpyObj<AuthService>('AuthService', [
      'isAuthenticated',
      'hasRole'
    ]);

    TestBed.configureTestingModule({
      imports: [RouterTestingModule],
      providers: [{ provide: AuthService, useValue: authServiceSpy }]
    });

    router = TestBed.inject(Router);
  });

  it('should redirect to /login and return false when unauthenticated', () => {
    authServiceSpy.isAuthenticated.and.returnValue(false);
    const navigateSpy = spyOn(router, 'navigate');

    const result = TestBed.runInInjectionContext(() =>
      authGuard(buildRoute(), {} as any)
    );

    expect(result).toBeFalse();
    expect(navigateSpy).toHaveBeenCalledWith(['/login']);
  });

  it('should return true when authenticated and no roles required', () => {
    authServiceSpy.isAuthenticated.and.returnValue(true);

    const result = TestBed.runInInjectionContext(() =>
      authGuard(buildRoute(), {} as any)
    );

    expect(result).toBeTrue();
  });

  it('should return true when authenticated and user has a required role', () => {
    authServiceSpy.isAuthenticated.and.returnValue(true);
    authServiceSpy.hasRole.and.returnValue(true);

    const result = TestBed.runInInjectionContext(() =>
      authGuard(buildRoute(['Admin']), {} as any)
    );

    expect(result).toBeTrue();
    expect(authServiceSpy.hasRole).toHaveBeenCalledWith(['Admin']);
  });

  it('should redirect to /unauthorized and return false when role missing', () => {
    authServiceSpy.isAuthenticated.and.returnValue(true);
    authServiceSpy.hasRole.and.returnValue(false);
    const navigateSpy = spyOn(router, 'navigate');

    const result = TestBed.runInInjectionContext(() =>
      authGuard(buildRoute(['Admin']), {} as any)
    );

    expect(result).toBeFalse();
    expect(navigateSpy).toHaveBeenCalledWith(['/unauthorized']);
  });

  it('should return true when route.data.roles is an empty array', () => {
    authServiceSpy.isAuthenticated.and.returnValue(true);

    const result = TestBed.runInInjectionContext(() =>
      authGuard(buildRoute([]), {} as any)
    );

    expect(result).toBeTrue();
  });
});
