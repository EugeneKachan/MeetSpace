import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { MatSelectModule } from '@angular/material/select';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';
import { NO_ERRORS_SCHEMA } from '@angular/core';

import { EditUserDialogComponent } from './edit-user-dialog.component';
import { UsersService } from '../../../core/services/users.service';
import { User } from '../../../models/entities.model';
import { USER_ROLES } from '../create-user-dialog/create-user-dialog.component';

const EXISTING_USER: User = {
  id: 'user-42',
  firstName: 'Alice',
  lastName: 'Brown',
  email: 'alice.brown@example.com',
  role: 'OfficeManager',
  isActive: true,
  createdAt: '2024-01-15T00:00:00Z'
};

const UPDATED_USER: User = {
  ...EXISTING_USER,
  firstName: 'Alicia',
  role: 'Employee'
};

describe('EditUserDialogComponent', () => {
  let component: EditUserDialogComponent;
  let fixture: ComponentFixture<EditUserDialogComponent>;
  let usersServiceSpy: jasmine.SpyObj<UsersService>;
  let dialogRefSpy: jasmine.SpyObj<MatDialogRef<EditUserDialogComponent>>;

  beforeEach(async () => {
    usersServiceSpy = jasmine.createSpyObj<UsersService>('UsersService', [
      'getUsers',
      'createUser',
      'updateUser'
    ]);

    dialogRefSpy = jasmine.createSpyObj<MatDialogRef<EditUserDialogComponent>>(
      'MatDialogRef',
      ['close']
    );

    await TestBed.configureTestingModule({
      declarations: [EditUserDialogComponent],
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
        { provide: MatDialogRef, useValue: dialogRefSpy },
        { provide: MAT_DIALOG_DATA, useValue: EXISTING_USER }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(EditUserDialogComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  // -------------------------------------------------------------------------
  // Form pre-population
  // -------------------------------------------------------------------------

  it('should pre-populate form with injected user data', () => {
    expect(component.form.get('firstName')?.value).toBe(EXISTING_USER.firstName);
    expect(component.form.get('lastName')?.value).toBe(EXISTING_USER.lastName);
    expect(component.form.get('email')?.value).toBe(EXISTING_USER.email);
    expect(component.form.get('role')?.value).toBe(EXISTING_USER.role);
    expect(component.form.get('isActive')?.value).toBe(EXISTING_USER.isActive);
  });

  it('should expose the injected user via data property', () => {
    expect(component.data).toEqual(EXISTING_USER);
  });

  it('should expose USER_ROLES as roles', () => {
    expect(component.roles).toEqual(USER_ROLES);
  });

  it('should not contain a password field', () => {
    expect(component.form.contains('password')).toBeFalse();
  });

  // -------------------------------------------------------------------------
  // Form validation
  // -------------------------------------------------------------------------

  it('should be invalid when firstName is cleared', () => {
    component.form.get('firstName')?.setValue('');
    expect(component.form.get('firstName')?.valid).toBeFalse();
  });

  it('should be invalid for a malformed email', () => {
    component.form.get('email')?.setValue('not-an-email');
    expect(component.form.get('email')?.valid).toBeFalse();
  });

  // -------------------------------------------------------------------------
  // onSubmit()
  // -------------------------------------------------------------------------

  it('onSubmit() should do nothing when form is invalid', () => {
    component.form.get('firstName')?.setValue('');
    component.onSubmit();
    expect(usersServiceSpy.updateUser).not.toHaveBeenCalled();
  });

  it('onSubmit() should call usersService.updateUser() with user id and form values', () => {
    usersServiceSpy.updateUser.and.returnValue(of(UPDATED_USER));

    component.onSubmit();

    expect(usersServiceSpy.updateUser).toHaveBeenCalledWith(
      EXISTING_USER.id,
      component.form.value
    );
  });

  it('onSubmit() should close dialog with updated user on success', fakeAsync(() => {
    usersServiceSpy.updateUser.and.returnValue(of(UPDATED_USER));

    component.onSubmit();
    tick();

    expect(dialogRefSpy.close).toHaveBeenCalledWith(UPDATED_USER);
    expect(component.isLoading).toBeFalse();
  }));

  it('onSubmit() should set errorMessage using detail on failure', fakeAsync(() => {
    usersServiceSpy.updateUser.and.returnValue(
      throwError(() => ({ error: { detail: 'Conflict detected' } }))
    );

    component.onSubmit();
    tick();

    expect(component.errorMessage).toBe('Conflict detected');
    expect(component.isLoading).toBeFalse();
  }));

  it('onSubmit() should fall back to title when detail absent', fakeAsync(() => {
    usersServiceSpy.updateUser.and.returnValue(
      throwError(() => ({ error: { title: 'Validation error' } }))
    );

    component.onSubmit();
    tick();

    expect(component.errorMessage).toBe('Validation error');
  }));

  it('onSubmit() should fall back to generic message when no error detail or title', fakeAsync(() => {
    usersServiceSpy.updateUser.and.returnValue(throwError(() => ({ error: {} })));

    component.onSubmit();
    tick();

    expect(component.errorMessage).toBe('Failed to update user. Please try again.');
  }));

  // -------------------------------------------------------------------------
  // onCancel()
  // -------------------------------------------------------------------------

  it('onCancel() should close dialog without a value', () => {
    component.onCancel();
    expect(dialogRefSpy.close).toHaveBeenCalledWith();
  });
});
