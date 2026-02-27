import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { MatDialogModule, MatDialog } from '@angular/material/dialog';
import { MatSnackBarModule, MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule } from '@angular/material/sort';
import { MatPaginatorModule } from '@angular/material/paginator';
import { of, throwError } from 'rxjs';
import { NO_ERRORS_SCHEMA } from '@angular/core';

import { UserManagementComponent } from './user-management.component';
import { UsersService } from '../../../core/services/users.service';
import { User } from '../../../models/entities.model';

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
    usersServiceSpy.getUsers.and.returnValue(of(MOCK_USERS));

    dialogSpy = jasmine.createSpyObj<MatDialog>('MatDialog', ['open']);
    snackBarSpy = jasmine.createSpyObj<MatSnackBar>('MatSnackBar', ['open']);

    await TestBed.configureTestingModule({
      declarations: [UserManagementComponent],
      imports: [
        NoopAnimationsModule,
        MatDialogModule,
        MatSnackBarModule,
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
  // loadUsers()
  // -------------------------------------------------------------------------

  it('loadUsers() should call usersService.getUsers() on init', () => {
    expect(usersServiceSpy.getUsers).toHaveBeenCalled();
  });

  it('loadUsers() should populate dataSource with returned users', () => {
    expect(component.dataSource.data).toEqual(MOCK_USERS);
  });

  it('loadUsers() should set isLoading=false on success', () => {
    expect(component.isLoading).toBeFalse();
  });

  it('loadUsers() should set loadError and isLoading=false on failure', () => {
    usersServiceSpy.getUsers.and.returnValue(throwError(() => new Error('Network error')));

    component.loadUsers();

    expect(component.loadError).toBe('Failed to load users. Please refresh the page.');
    expect(component.isLoading).toBeFalse();
  });

  it('loadUsers() should clear loadError before each request', () => {
    component.loadError = 'Old error';
    usersServiceSpy.getUsers.and.returnValue(of(MOCK_USERS));

    component.loadUsers();

    expect(component.loadError).toBeNull();
  });

  // -------------------------------------------------------------------------
  // applyFilter()
  // -------------------------------------------------------------------------

  it('applyFilter() should update dataSource.filter with trimmed lowercase value', () => {
    const inputEvent = { target: { value: '  Alice  ' } } as unknown as Event;
    component.applyFilter(inputEvent);
    expect(component.dataSource.filter).toBe('alice');
  });

  it('applyFilter() should clear filter when input is empty', () => {
    component.dataSource.filter = 'existing';
    const inputEvent = { target: { value: '' } } as unknown as Event;
    component.applyFilter(inputEvent);
    expect(component.dataSource.filter).toBe('');
  });

  // -------------------------------------------------------------------------
  // openCreateUserDialog()
  // -------------------------------------------------------------------------

  it('openCreateUserDialog() should open dialog and prepend new user to dataSource', fakeAsync(() => {
    const newUser: User = {
      id: 'u3',
      firstName: 'Carol',
      lastName: 'White',
      email: 'carol@example.com',
      role: 'OfficeManager',
      isActive: true,
      createdAt: '2024-03-01T00:00:00Z'
    };

    const dialogRefSpy = jasmine.createSpyObj('MatDialogRef', ['afterClosed']);
    dialogRefSpy.afterClosed.and.returnValue(of(newUser));
    dialogSpy.open.and.returnValue(dialogRefSpy);

    component.openCreateUserDialog();
    tick();

    expect(component.dataSource.data[0]).toEqual(newUser);
    expect(component.dataSource.data.length).toBe(MOCK_USERS.length + 1);
  }));

  it('openCreateUserDialog() should NOT modify dataSource when dialog is cancelled', fakeAsync(() => {
    const dialogRefSpy = jasmine.createSpyObj('MatDialogRef', ['afterClosed']);
    dialogRefSpy.afterClosed.and.returnValue(of(undefined));
    dialogSpy.open.and.returnValue(dialogRefSpy);

    component.openCreateUserDialog();
    tick();

    expect(component.dataSource.data.length).toBe(MOCK_USERS.length);
  }));

  // -------------------------------------------------------------------------
  // openEditUserDialog()
  // -------------------------------------------------------------------------

  it('openEditUserDialog() should patch user in-place in dataSource', fakeAsync(() => {
    const updatedUser: User = { ...MOCK_USERS[0], firstName: 'AliceUpdated' };

    const dialogRefSpy = jasmine.createSpyObj('MatDialogRef', ['afterClosed']);
    dialogRefSpy.afterClosed.and.returnValue(of(updatedUser));
    dialogSpy.open.and.returnValue(dialogRefSpy);

    component.openEditUserDialog(MOCK_USERS[0]);
    tick();

    const updated = component.dataSource.data.find(u => u.id === 'u1');
    expect(updated?.firstName).toBe('AliceUpdated');
  }));

  it('openEditUserDialog() should NOT modify data when dialog cancelled', fakeAsync(() => {
    const originalData = [...component.dataSource.data];

    const dialogRefSpy = jasmine.createSpyObj('MatDialogRef', ['afterClosed']);
    dialogRefSpy.afterClosed.and.returnValue(of(undefined));
    dialogSpy.open.and.returnValue(dialogRefSpy);

    component.openEditUserDialog(MOCK_USERS[0]);
    tick();

    expect(component.dataSource.data).toEqual(originalData);
  }));

  it('displayedColumns should include the expected column names', () => {
    expect(component.displayedColumns).toContain('fullName');
    expect(component.displayedColumns).toContain('email');
    expect(component.displayedColumns).toContain('role');
    expect(component.displayedColumns).toContain('actions');
  });
});
