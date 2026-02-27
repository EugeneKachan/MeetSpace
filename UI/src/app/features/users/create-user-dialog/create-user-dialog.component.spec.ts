import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MatDialogModule } from '@angular/material/dialog';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';
import { NO_ERRORS_SCHEMA } from '@angular/core';

import { CreateUserDialogComponent, USER_ROLES } from './create-user-dialog.component';
import { UsersService } from '../../../core/services/users.service';
import { User } from '../../../models/entities.model';

const CREATED_USER: User = {
  id: 'new-id',
  firstName: 'John',
  lastName: 'Doe',
  email: 'john.doe@example.com',
  role: 'Employee',
  isActive: true,
  createdAt: '2024-01-01T00:00:00Z'
};

describe('CreateUserDialogComponent', () => {
  let component: CreateUserDialogComponent;
  let fixture: ComponentFixture<CreateUserDialogComponent>;
  let usersServiceSpy: jasmine.SpyObj<UsersService>;
  let dialogRefSpy: jasmine.SpyObj<MatDialogRef<CreateUserDialogComponent>>;

  beforeEach(async () => {
    usersServiceSpy = jasmine.createSpyObj<UsersService>('UsersService', [
      'getUsers',
      'createUser',
      'updateUser'
    ]);

    dialogRefSpy = jasmine.createSpyObj<MatDialogRef<CreateUserDialogComponent>>(
      'MatDialogRef',
      ['close']
    );

    await TestBed.configureTestingModule({
      declarations: [CreateUserDialogComponent],
      imports: [
        ReactiveFormsModule,
        NoopAnimationsModule,
        MatDialogModule,
        MatSlideToggleModule,
        MatSelectModule,
        MatFormFieldModule,
        MatInputModule
      ],
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
        { provide: UsersService, useValue: usersServiceSpy },
        { provide: MatDialogRef, useValue: dialogRefSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(CreateUserDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  // -------------------------------------------------------------------------
  // Form initialisation
  // -------------------------------------------------------------------------

  it('should have all expected form controls', () => {
    expect(component.form.contains('firstName')).toBeTrue();
    expect(component.form.contains('lastName')).toBeTrue();
    expect(component.form.contains('email')).toBeTrue();
    expect(component.form.contains('password')).toBeTrue();
    expect(component.form.contains('role')).toBeTrue();
    expect(component.form.contains('isActive')).toBeTrue();
  });

  it('should default role to Employee', () => {
    expect(component.form.get('role')?.value).toBe('Employee');
  });

  it('should default isActive to true', () => {
    expect(component.form.get('isActive')?.value).toBeTrue();
  });

  it('should expose USER_ROLES as roles', () => {
    expect(component.roles).toEqual(USER_ROLES);
  });

  it('should be invalid when firstName is empty', () => {
    component.form.get('firstName')?.setValue('');
    expect(component.form.get('firstName')?.valid).toBeFalse();
  });

  it('should be invalid for a weak password', () => {
    component.form.get('password')?.setValue('password');
    expect(component.form.get('password')?.valid).toBeFalse();
  });

  it('should be valid for a strong password', () => {
    component.form.get('password')?.setValue('Password1!');
    expect(component.form.get('password')?.valid).toBeTrue();
  });

  // -------------------------------------------------------------------------
  // onSubmit()
  // -------------------------------------------------------------------------

  it('onSubmit() should do nothing when form is invalid', () => {
    component.form.get('firstName')?.setValue('');
    component.onSubmit();
    expect(usersServiceSpy.createUser).not.toHaveBeenCalled();
  });

  it('onSubmit() should call usersService.createUser() with form values', () => {
    fillValidForm(component);
    usersServiceSpy.createUser.and.returnValue(of(CREATED_USER));

    component.onSubmit();

    expect(usersServiceSpy.createUser).toHaveBeenCalledWith(component.form.value);
  });

  it('onSubmit() should close dialog with user on success', fakeAsync(() => {
    fillValidForm(component);
    usersServiceSpy.createUser.and.returnValue(of(CREATED_USER));

    component.onSubmit();
    tick();

    expect(dialogRefSpy.close).toHaveBeenCalledWith(CREATED_USER);
    expect(component.isLoading).toBeFalse();
  }));

  it('onSubmit() should set errorMessage on failure', fakeAsync(() => {
    fillValidForm(component);
    usersServiceSpy.createUser.and.returnValue(
      throwError(() => ({ error: { detail: 'Email already in use' } }))
    );

    component.onSubmit();
    tick();

    expect(component.errorMessage).toBe('Email already in use');
    expect(component.isLoading).toBeFalse();
  }));

  it('onSubmit() should fall back to title when error has no detail', fakeAsync(() => {
    fillValidForm(component);
    usersServiceSpy.createUser.and.returnValue(
      throwError(() => ({ error: { title: 'Validation Failed' } }))
    );

    component.onSubmit();
    tick();

    expect(component.errorMessage).toBe('Validation Failed');
  }));

  // -------------------------------------------------------------------------
  // onCancel()
  // -------------------------------------------------------------------------

  it('onCancel() should close dialog without a value', () => {
    component.onCancel();
    expect(dialogRefSpy.close).toHaveBeenCalledWith();
  });
});

// ---------------------------------------------------------------------------
// Helper
// ---------------------------------------------------------------------------

function fillValidForm(component: CreateUserDialogComponent): void {
  component.form.get('firstName')?.setValue('John');
  component.form.get('lastName')?.setValue('Doe');
  component.form.get('email')?.setValue('john.doe@example.com');
  component.form.get('password')?.setValue('Password1!');
  component.form.get('role')?.setValue('Employee');
  component.form.get('isActive')?.setValue(true);
}
