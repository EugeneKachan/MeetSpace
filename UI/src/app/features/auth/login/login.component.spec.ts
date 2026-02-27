import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { Router } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { of, throwError } from 'rxjs';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { NO_ERRORS_SCHEMA } from '@angular/core';

import { LoginComponent } from './login.component';
import { AuthService } from '../../../core/services/auth.service';
import { TokenResponse } from '../../../models/auth.model';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let router: Router;

  const mockTokenResponse: TokenResponse = {
    access_token: 'token-abc',
    token_type: 'Bearer',
    expires_in: 3600
  };

  beforeEach(async () => {
    authServiceSpy = jasmine.createSpyObj<AuthService>('AuthService', [
      'login',
      'logout',
      'isAuthenticated',
      'hasRole',
      'getCurrentUser',
      'getToken'
    ]);
    authServiceSpy.isAuthenticated.and.returnValue(false);

    await TestBed.configureTestingModule({
      declarations: [LoginComponent],
      imports: [
        ReactiveFormsModule,
        RouterTestingModule,
        NoopAnimationsModule
      ],
      schemas: [NO_ERRORS_SCHEMA],
      providers: [{ provide: AuthService, useValue: authServiceSpy }]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  // -------------------------------------------------------------------------
  // Redirect when already authenticated
  // -------------------------------------------------------------------------

  it('ngOnInit() should navigate to /dashboard when already authenticated', fakeAsync(() => {
    authServiceSpy.isAuthenticated.and.returnValue(true);
    const navigateSpy = spyOn(router, 'navigate');

    component.ngOnInit();

    expect(navigateSpy).toHaveBeenCalledWith(['/dashboard']);
  }));

  // -------------------------------------------------------------------------
  // Form initialisation
  // -------------------------------------------------------------------------

  it('should initialise form with email and password controls', () => {
    expect(component.form.contains('email')).toBeTrue();
    expect(component.form.contains('password')).toBeTrue();
  });

  it('should expose email and password accessors', () => {
    expect(component.email).toBeTruthy();
    expect(component.password).toBeTruthy();
  });

  it('email control should be invalid when empty', () => {
    component.email.setValue('');
    expect(component.email.valid).toBeFalse();
  });

  it('email control should be invalid for non-email value', () => {
    component.email.setValue('not-an-email');
    expect(component.email.valid).toBeFalse();
  });

  it('email control should be valid for proper email', () => {
    component.email.setValue('user@example.com');
    expect(component.email.valid).toBeTrue();
  });

  it('password control should be invalid when too short', () => {
    component.password.setValue('12345');
    expect(component.password.valid).toBeFalse();
  });

  it('password control should be valid for 6+ chars', () => {
    component.password.setValue('123456');
    expect(component.password.valid).toBeTrue();
  });

  // -------------------------------------------------------------------------
  // onSubmit()
  // -------------------------------------------------------------------------

  it('onSubmit() should do nothing when form is invalid', () => {
    component.email.setValue('');
    component.password.setValue('');
    component.onSubmit();
    expect(authServiceSpy.login).not.toHaveBeenCalled();
  });

  it('onSubmit() should call authService.login() with form values', () => {
    authServiceSpy.login.and.returnValue(of(mockTokenResponse));
    component.email.setValue('user@example.com');
    component.password.setValue('password123');

    component.onSubmit();

    expect(authServiceSpy.login).toHaveBeenCalledWith('user@example.com', 'password123');
  });

  it('onSubmit() should set isLoading=true when form is submitted', () => {
    authServiceSpy.login.and.returnValue(of(mockTokenResponse));
    spyOn(router, 'navigate');
    component.email.setValue('user@example.com');
    component.password.setValue('password123');

    expect(component.isLoading).toBeFalse();
    component.onSubmit();
    // isLoading stays true on success — the component navigates away rather than resetting it
    expect(component.isLoading).toBeTrue();
  });

  it('onSubmit() should navigate to /dashboard on success', fakeAsync(() => {
    authServiceSpy.login.and.returnValue(of(mockTokenResponse));
    const navigateSpy = spyOn(router, 'navigate');

    component.email.setValue('user@example.com');
    component.password.setValue('password123');
    component.onSubmit();
    tick();

    expect(navigateSpy).toHaveBeenCalledWith(['/dashboard']);
  }));

  it('onSubmit() should set errorMessage and clear isLoading on error', fakeAsync(() => {
    authServiceSpy.login.and.returnValue(throwError(() => new Error('Invalid credentials')));

    component.email.setValue('user@example.com');
    component.password.setValue('password123');
    component.onSubmit();
    tick();

    expect(component.errorMessage).toBe('Invalid credentials');
    expect(component.isLoading).toBeFalse();
  }));

  it('onSubmit() should clear errorMessage before a new attempt', fakeAsync(() => {
    component.errorMessage = 'Previous error';
    authServiceSpy.login.and.returnValue(of(mockTokenResponse));
    spyOn(router, 'navigate');

    component.email.setValue('user@example.com');
    component.password.setValue('password123');
    component.onSubmit();
    tick();

    expect(component.errorMessage).toBeNull();
  }));
});
