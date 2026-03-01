import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MatDialog } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule } from '@angular/material/sort';
import { MatPaginatorModule } from '@angular/material/paginator';
import { of, throwError } from 'rxjs';
import { NO_ERRORS_SCHEMA } from '@angular/core';

import { UserManagementComponent } from './user-management.component';
import { UsersService } from '../../../core/services/users.service';
import { User, PagedResult } from '../../../models/entities.model';

const MOCK_USERS: User[] = [
  {
    id: 'u1',
    firstName: 'Alice',
    lastName: 'Brown',
    email: 'alice@example.com',
    role: 'Admin',
    isActive: true,
    createdAt: '2024-01-01T00:00:00Z'
  },
  {
    id: 'u2',
    firstName: 'Bob',
    lastName: 'Green',
    email: 'bob@example.com',
    role: 'Employee',
    isActive: false,
    createdAt: '2024-02-01T00:00:00Z'
  }
];

const MOCK_PAGED_USERS: PagedResult<User> = {
  items: MOCK_USERS,
  totalCount: 2,
  page: 1,
  pageSize: 10,
};

describe('UserManagementComponent', () => {
  let component: UserManagementComponent;
  let fixture: ComponentFixture<UserManagementComponent>;
  let usersServiceSpy: jasmine.SpyObj<UsersService>;
  let dialogSpy: jasmine.SpyObj<MatDialog>;
  let snackBarSpy: jasmine.SpyObj<MatSnackBar>;

  beforeEach(async () => {
    usersServiceSpy = jasmine.createSpyObj<UsersService>('UsersService', [
      'getUsers',
      'createUser',
      'updateUser'
    ]);
    usersServiceSpy.getUsers.and.returnValue(of(MOCK_PAGED_USERS));

    dialogSpy = jasmine.createSpyObj<MatDialog>('MatDialog', ['open']);
    snackBarSpy = jasmine.createSpyObj<MatSnackBar>('MatSnackBar', ['open']);

    await TestBed.configureTestingModule({
      declarations: [UserManagementComponent],
      imports: [
        NoopAnimationsModule,
        MatTableModule,
        MatSortModule,
        MatPaginatorModule
      ],
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
        { provide: UsersService, useValue: usersServiceSpy },
        { provide: MatDialog, useValue: dialogSpy },
        { provide: MatSnackBar, useValue: snackBarSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(UserManagementComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  // -------------------------------------------------------------------------
  // Initial load
  // -------------------------------------------------------------------------

  it('should call getUsers() on init', () => {
    expect(usersServiceSpy.getUsers).toHaveBeenCalled();
  });

  it('should populate dataSource.data with result.items', () => {
    expect(component.dataSource.data).toEqual(MOCK_PAGED_USERS.items);
  });

  it('should set totalCount from result.totalCount', () => {
    expect(component.totalCount).toBe(MOCK_PAGED_USERS.totalCount);
  });

  it('should set isLoading=false after successful load', () => {
    expect(component.isLoading).toBeFalse();
  });

  it('should set loadError and isLoading=false on load failure', () => {
    usersServiceSpy.getUsers.and.returnValue(throwError(() => new Error('Network error')));

    (component as any).loadUsers();

    expect(component.loadError).toBe('Failed to load users. Please refresh the page.');
    expect(component.isLoading).toBeFalse();
  });

  it('should clear loadError before each request', () => {
    component.loadError = 'Old error';
    usersServiceSpy.getUsers.and.returnValue(of(MOCK_PAGED_USERS));

    (component as any).loadUsers();

    expect(component.loadError).toBeNull();
  });

  // -------------------------------------------------------------------------
  // onSearchInput()
  // -------------------------------------------------------------------------

  it('onSearchInput() should not throw', () => {
    expect(() => {
      component.onSearchInput({ target: { value: 'alice' } } as unknown as Event);
    }).not.toThrow();
  });

  it('onSearchInput() should debounce and reload after 300ms', fakeAsync(() => {
    usersServiceSpy.getUsers.calls.reset();

    component.onSearchInput({ target: { value: 'alice' } } as unknown as Event);
    tick(299);
    expect(usersServiceSpy.getUsers).not.toHaveBeenCalled();

    tick(1);
    expect(usersServiceSpy.getUsers).toHaveBeenCalled();
  }));

  it('onSearchInput() should coalesce rapid keystrokes into a single reload', fakeAsync(() => {
    usersServiceSpy.getUsers.calls.reset();

    component.onSearchInput({ target: { value: 'a' } } as unknown as Event);
    tick(100);
    component.onSearchInput({ target: { value: 'al' } } as unknown as Event);
    tick(100);
    component.onSearchInput({ target: { value: 'ali' } } as unknown as Event);
    tick(300);

    expect(usersServiceSpy.getUsers).toHaveBeenCalledTimes(1);
  }));

  // -------------------------------------------------------------------------
  // openCreateUserDialog()
  // -------------------------------------------------------------------------

  it('openCreateUserDialog() should open the dialog', () => {
    const fakeRef = jasmine.createSpyObj('MatDialogRef', ['afterClosed']);
    fakeRef.afterClosed.and.returnValue(of(undefined));
    dialogSpy.open.and.returnValue(fakeRef);

    component.openCreateUserDialog();

    expect(dialogSpy.open).toHaveBeenCalled();
  });

  it('openCreateUserDialog() should reload and show snackbar when user is created', fakeAsync(() => {
    const newUser: User = {
      id: 'u3', firstName: 'Carol', lastName: 'White',
      email: 'carol@example.com', role: 'OfficeManager', isActive: true, createdAt: '2024-03-01T00:00:00Z'
    };
    const fakeRef = jasmine.createSpyObj('MatDialogRef', ['afterClosed']);
    fakeRef.afterClosed.and.returnValue(of(newUser));
    dialogSpy.open.and.returnValue(fakeRef);
    usersServiceSpy.getUsers.calls.reset();

    component.openCreateUserDialog();
    tick();

    expect(usersServiceSpy.getUsers).toHaveBeenCalled();
    expect(snackBarSpy.open).toHaveBeenCalledWith(
      `User ${newUser.firstName} ${newUser.lastName} created successfully.`,
      'Dismiss',
      jasmine.any(Object)
    );
  }));

  it('openCreateUserDialog() should NOT reload when dialog is cancelled', fakeAsync(() => {
    const fakeRef = jasmine.createSpyObj('MatDialogRef', ['afterClosed']);
    fakeRef.afterClosed.and.returnValue(of(undefined));
    dialogSpy.open.and.returnValue(fakeRef);
    usersServiceSpy.getUsers.calls.reset();

    component.openCreateUserDialog();
    tick();

    expect(usersServiceSpy.getUsers).not.toHaveBeenCalled();
  }));

  // -------------------------------------------------------------------------
  // openEditUserDialog()
  // -------------------------------------------------------------------------

  it('openEditUserDialog() should open the dialog with the user as data', () => {
    const fakeRef = jasmine.createSpyObj('MatDialogRef', ['afterClosed']);
    fakeRef.afterClosed.and.returnValue(of(undefined));
    dialogSpy.open.and.returnValue(fakeRef);

    component.openEditUserDialog(MOCK_USERS[0]);

    expect(dialogSpy.open).toHaveBeenCalled();
    const callArgs = dialogSpy.open.calls.mostRecent().args;
    expect(callArgs[1]?.data).toEqual(MOCK_USERS[0]);
  });

  it('openEditUserDialog() should reload and show snackbar when user is updated', fakeAsync(() => {
    const updatedUser: User = { ...MOCK_USERS[0], firstName: 'AliceUpdated' };
    const fakeRef = jasmine.createSpyObj('MatDialogRef', ['afterClosed']);
    fakeRef.afterClosed.and.returnValue(of(updatedUser));
    dialogSpy.open.and.returnValue(fakeRef);
    usersServiceSpy.getUsers.calls.reset();

    component.openEditUserDialog(MOCK_USERS[0]);
    tick();

    expect(usersServiceSpy.getUsers).toHaveBeenCalled();
    expect(snackBarSpy.open).toHaveBeenCalledWith(
      `User ${updatedUser.firstName} ${updatedUser.lastName} updated successfully.`,
      'Dismiss',
      jasmine.any(Object)
    );
  }));

  it('openEditUserDialog() should NOT reload when dialog is cancelled', fakeAsync(() => {
    const fakeRef = jasmine.createSpyObj('MatDialogRef', ['afterClosed']);
    fakeRef.afterClosed.and.returnValue(of(undefined));
    dialogSpy.open.and.returnValue(fakeRef);
    usersServiceSpy.getUsers.calls.reset();

    component.openEditUserDialog(MOCK_USERS[0]);
    tick();

    expect(usersServiceSpy.getUsers).not.toHaveBeenCalled();
  }));

  // -------------------------------------------------------------------------
  // displayedColumns
  // -------------------------------------------------------------------------

  it('displayedColumns should include the expected column names', () => {
    expect(component.displayedColumns).toContain('fullName');
    expect(component.displayedColumns).toContain('email');
    expect(component.displayedColumns).toContain('role');
    expect(component.displayedColumns).toContain('actions');
  });
});
