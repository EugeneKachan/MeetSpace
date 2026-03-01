import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { MatDialog, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatTableModule } from '@angular/material/table';
import { MatSortModule } from '@angular/material/sort';
import { MatPaginatorModule } from '@angular/material/paginator';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';
import { NO_ERRORS_SCHEMA } from '@angular/core';

import { OfficesPageComponent } from './offices-page.component';
import { OfficesService } from '../../../core/services/offices.service';
import { AuthService } from '../../../core/services/auth.service';
import { User as AuthUser } from '../../../models/auth.model';
import { Office } from '../../../models/entities.model';

const ADMIN_AUTH_USER: AuthUser = {
  id: 'admin-id', name: 'Admin User', email: 'admin@test.com', role: 'Admin',
};

const MANAGER_AUTH_USER: AuthUser = {
  id: 'mgr-id', name: 'Manager User', email: 'manager@test.com', role: 'OfficeManager',
};

const MOCK_OFFICES: Office[] = [
  {
    id: 'office-1',
    name: 'London HQ',
    address: '10 Downing St',
    isActive: true,
    rooms: [],
    managers: [],
  },
  {
    id: 'office-2',
    name: 'Berlin Office',
    address: 'Unter den Linden 1',
    isActive: false,
    rooms: [],
    managers: [],
  },
];

describe('OfficesPageComponent', () => {
  let component: OfficesPageComponent;
  let fixture: ComponentFixture<OfficesPageComponent>;
  let officesServiceSpy: jasmine.SpyObj<OfficesService>;
  let authServiceSpy: jasmine.SpyObj<AuthService>;
  let dialogSpy: jasmine.SpyObj<MatDialog>;
  let snackBarSpy: jasmine.SpyObj<MatSnackBar>;

  function createComponent(currentUser: AuthUser | null = ADMIN_AUTH_USER): void {
    authServiceSpy.getCurrentUser.and.returnValue(currentUser);
    fixture = TestBed.createComponent(OfficesPageComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  }

  beforeEach(async () => {
    officesServiceSpy = jasmine.createSpyObj<OfficesService>('OfficesService', [
      'getOffices',
      'deactivateOffice',
    ]);
    officesServiceSpy.getOffices.and.returnValue(of(MOCK_OFFICES));

    authServiceSpy = jasmine.createSpyObj<AuthService>('AuthService', ['getCurrentUser']);

    dialogSpy = jasmine.createSpyObj<MatDialog>('MatDialog', ['open']);

    snackBarSpy = jasmine.createSpyObj<MatSnackBar>('MatSnackBar', ['open']);

    await TestBed.configureTestingModule({
      declarations: [OfficesPageComponent],
      imports: [
        NoopAnimationsModule,
        MatTableModule,
        MatSortModule,
        MatPaginatorModule,
      ],
      schemas: [NO_ERRORS_SCHEMA],
      providers: [
        { provide: OfficesService, useValue: officesServiceSpy },
        { provide: AuthService, useValue: authServiceSpy },
        { provide: MatDialog, useValue: dialogSpy },
        { provide: MatSnackBar, useValue: snackBarSpy },
      ],
    }).compileComponents();
  });

  it('should create', () => {
    createComponent();
    expect(component).toBeTruthy();
  });

  // -------------------------------------------------------------------------
  // Role detection
  // -------------------------------------------------------------------------

  it('should set isAdmin = true when current user is Admin', () => {
    createComponent(ADMIN_AUTH_USER);
    expect(component.isAdmin).toBeTrue();
  });

  it('should set isAdmin = false when current user is OfficeManager', () => {
    createComponent(MANAGER_AUTH_USER);
    expect(component.isAdmin).toBeFalse();
  });

  it('should set isAdmin = false when no user is logged in', () => {
    createComponent(null);
    expect(component.isAdmin).toBeFalse();
  });

  // -------------------------------------------------------------------------
  // loadOffices()
  // -------------------------------------------------------------------------

  it('should load offices on init', () => {
    createComponent();
    expect(officesServiceSpy.getOffices).toHaveBeenCalled();
    expect(component.dataSource.data).toEqual(MOCK_OFFICES);
    expect(component.isLoading).toBeFalse();
  });

  it('should set loadError on load failure', () => {
    officesServiceSpy.getOffices.and.returnValue(throwError(() => new Error('Network error')));
    createComponent();

    expect(component.loadError).toBeTruthy();
    expect(component.isLoading).toBeFalse();
  });

  it('loadOffices() should reset loadError before reloading', () => {
    officesServiceSpy.getOffices.and.returnValue(throwError(() => new Error('err')));
    createComponent();
    expect(component.loadError).toBeTruthy();

    officesServiceSpy.getOffices.and.returnValue(of(MOCK_OFFICES));
    component.loadOffices();

    expect(component.loadError).toBeNull();
    expect(component.dataSource.data).toEqual(MOCK_OFFICES);
  });

  // -------------------------------------------------------------------------
  // applyFilter()
  // -------------------------------------------------------------------------

  it('applyFilter() should apply trimmed lowercase filter to dataSource', () => {
    createComponent();
    const event = { target: { value: '  London  ' } } as unknown as Event;
    component.applyFilter(event);

    expect(component.dataSource.filter).toBe('london');
  });

  // -------------------------------------------------------------------------
  // openCreateOfficeDialog()
  // -------------------------------------------------------------------------

  it('openCreateOfficeDialog() should open OfficeDialogComponent', () => {
    createComponent();
    const fakeDialogRef = { afterClosed: () => of(false) } as MatDialogRef<any>;
    dialogSpy.open.and.returnValue(fakeDialogRef);

    component.openCreateOfficeDialog();

    expect(dialogSpy.open).toHaveBeenCalled();
  });

  it('openCreateOfficeDialog() should reload on success and show snackbar', fakeAsync(() => {
    createComponent();
    const fakeDialogRef = { afterClosed: () => of(true) } as MatDialogRef<any>;
    dialogSpy.open.and.returnValue(fakeDialogRef);
    officesServiceSpy.getOffices.calls.reset();

    component.openCreateOfficeDialog();
    tick();

    expect(officesServiceSpy.getOffices).toHaveBeenCalled();
    expect(snackBarSpy.open).toHaveBeenCalled();
  }));

  it('openCreateOfficeDialog() should not reload when dialog cancelled', fakeAsync(() => {
    createComponent();
    const fakeDialogRef = { afterClosed: () => of(false) } as MatDialogRef<any>;
    dialogSpy.open.and.returnValue(fakeDialogRef);
    officesServiceSpy.getOffices.calls.reset();

    component.openCreateOfficeDialog();
    tick();

    expect(officesServiceSpy.getOffices).not.toHaveBeenCalled();
  }));

  // -------------------------------------------------------------------------
  // openEditOfficeDialog()
  // -------------------------------------------------------------------------

  it('openEditOfficeDialog() should open dialog with office data', () => {
    createComponent();
    const fakeDialogRef = { afterClosed: () => of(false) } as MatDialogRef<any>;
    dialogSpy.open.and.returnValue(fakeDialogRef);

    component.openEditOfficeDialog(MOCK_OFFICES[0]);

    const callArgs = dialogSpy.open.calls.mostRecent().args;
    expect((callArgs[1]?.data as any)?.office).toEqual(MOCK_OFFICES[0]);
  });

  it('openEditOfficeDialog() should reload and show snackbar when changes were saved', fakeAsync(() => {
    createComponent();
    const fakeDialogRef = { afterClosed: () => of(true) } as MatDialogRef<any>;
    dialogSpy.open.and.returnValue(fakeDialogRef);
    officesServiceSpy.getOffices.calls.reset();

    component.openEditOfficeDialog(MOCK_OFFICES[0]);
    tick();

    expect(officesServiceSpy.getOffices).toHaveBeenCalled();
    expect(snackBarSpy.open).toHaveBeenCalled();
  }));

  // -------------------------------------------------------------------------
  // deactivateOffice()
  // -------------------------------------------------------------------------

  it('deactivateOffice() should update row isActive to false on success', fakeAsync(() => {
    createComponent();
    spyOn(window, 'confirm').and.returnValue(true);
    officesServiceSpy.deactivateOffice = jasmine.createSpy().and.returnValue(of(void 0));

    component.deactivateOffice(MOCK_OFFICES[0]);
    tick();

    const updated = component.dataSource.data.find((o) => o.id === 'office-1');
    expect(updated?.isActive).toBeFalse();
    expect(snackBarSpy.open).toHaveBeenCalled();
  }));

  it('deactivateOffice() should show error snackbar on failure', fakeAsync(() => {
    createComponent();
    spyOn(window, 'confirm').and.returnValue(true);
    officesServiceSpy.deactivateOffice = jasmine.createSpy().and.returnValue(
      throwError(() => new Error('Server error')),
    );

    component.deactivateOffice(MOCK_OFFICES[0]);
    tick();

    expect(snackBarSpy.open).toHaveBeenCalled();
    // Original data should be unchanged
    const original = component.dataSource.data.find((o) => o.id === 'office-1');
    expect(original?.isActive).toBeTrue();
  }));

  it('deactivateOffice() should do nothing when confirm is cancelled', () => {
    createComponent();
    spyOn(window, 'confirm').and.returnValue(false);
    officesServiceSpy.deactivateOffice = jasmine.createSpy();

    component.deactivateOffice(MOCK_OFFICES[0]);

    expect(officesServiceSpy.deactivateOffice).not.toHaveBeenCalled();
  });
});
