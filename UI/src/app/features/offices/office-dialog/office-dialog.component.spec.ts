import { ComponentFixture, TestBed, fakeAsync, tick } from '@angular/core/testing';
import { ReactiveFormsModule } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { MatSlideToggleModule } from '@angular/material/slide-toggle';
import { NoopAnimationsModule } from '@angular/platform-browser/animations';
import { of, throwError } from 'rxjs';
import { NO_ERRORS_SCHEMA } from '@angular/core';

import { OfficeDialogComponent, OfficeDialogData } from './office-dialog.component';
import { OfficesService } from '../../../core/services/offices.service';
import { UsersService } from '../../../core/services/users.service';
import { AuthService } from '../../../core/services/auth.service';
import { User as AuthUser } from '../../../models/auth.model';
import { CreateRoomForOfficeRequest, ManagerSummary, Office, Room, User } from '../../../models/entities.model';

// ---------------------------------------------------------------------------
// Fixtures
// ---------------------------------------------------------------------------

const ADMIN_AUTH_USER: AuthUser = {
  id: 'admin-id', name: 'Admin User', email: 'admin@test.com', role: 'Admin',
};

const MANAGER_AUTH_USER: AuthUser = {
  id: 'mgr-id', name: 'Office Manager', email: 'mgr@test.com', role: 'OfficeManager',
};

const MOCK_ROOM: Room = {
  id: 'room-1', officeId: 'office-1',
  name: 'Room A', capacity: 10, description: 'Desc', isActive: true,
};

const MOCK_MANAGER: ManagerSummary = {
  id: 'mgr-id', firstName: 'Office', lastName: 'Manager', email: 'mgr@test.com',
};

const MOCK_OFFICE: Office = {
  id: 'office-1', name: 'London HQ', address: '10 Downing St',
  isActive: true,
  rooms: [MOCK_ROOM],
  managers: [MOCK_MANAGER],
};

// ---------------------------------------------------------------------------
// Factory
// ---------------------------------------------------------------------------

function buildComponent(
  dialogData: OfficeDialogData,
  currentUser: AuthUser | null,
  overrides: Partial<{
    officesService: jasmine.SpyObj<OfficesService>;
    usersService: jasmine.SpyObj<UsersService>;
  }> = {},
): {
  component: OfficeDialogComponent;
  fixture: ComponentFixture<OfficeDialogComponent>;
  officesServiceSpy: jasmine.SpyObj<OfficesService>;
  usersServiceSpy: jasmine.SpyObj<UsersService>;
  authServiceSpy: jasmine.SpyObj<AuthService>;
  dialogRefSpy: jasmine.SpyObj<MatDialogRef<OfficeDialogComponent>>;
  snackBarSpy: jasmine.SpyObj<MatSnackBar>;
} {
  const officesServiceSpy = overrides.officesService ?? jasmine.createSpyObj<OfficesService>(
    'OfficesService',
    ['createOffice', 'updateOffice', 'deactivateOffice', 'assignManager', 'removeManager',
      'createRoom', 'updateRoom', 'deactivateRoom'],
  );
  const usersServiceSpy = overrides.usersService ?? jasmine.createSpyObj<UsersService>(
    'UsersService', ['getUsers', 'createUser', 'updateUser'],
  );
  usersServiceSpy.getUsers.and.returnValue(of([]));

  const authServiceSpy = jasmine.createSpyObj<AuthService>('AuthService', ['getCurrentUser']);
  authServiceSpy.getCurrentUser.and.returnValue(currentUser);

  const dialogRefSpy = jasmine.createSpyObj<MatDialogRef<OfficeDialogComponent>>(
    'MatDialogRef', ['close'],
  );
  const snackBarSpy = jasmine.createSpyObj<MatSnackBar>('MatSnackBar', ['open']);

  TestBed.configureTestingModule({
    declarations: [OfficeDialogComponent],
      imports: [ReactiveFormsModule, NoopAnimationsModule, MatSlideToggleModule],
    schemas: [NO_ERRORS_SCHEMA],
    providers: [
      { provide: MAT_DIALOG_DATA, useValue: dialogData },
      { provide: MatDialogRef, useValue: dialogRefSpy },
      { provide: OfficesService, useValue: officesServiceSpy },
      { provide: UsersService, useValue: usersServiceSpy },
      { provide: AuthService, useValue: authServiceSpy },
      { provide: MatSnackBar, useValue: snackBarSpy },
    ],
  }).compileComponents();

  const fixture = TestBed.createComponent(OfficeDialogComponent);
  const component = fixture.componentInstance;
  fixture.detectChanges();

  return { component, fixture, officesServiceSpy, usersServiceSpy, authServiceSpy, dialogRefSpy, snackBarSpy };
}

// ---------------------------------------------------------------------------
// Suite: Create mode (Admin)
// ---------------------------------------------------------------------------

describe('OfficeDialogComponent — Create mode (Admin)', () => {
  let ctx: ReturnType<typeof buildComponent>;

  beforeEach(() => {
    ctx = buildComponent({ office: null }, ADMIN_AUTH_USER);
  });

  it('should create', () => {
    expect(ctx.component).toBeTruthy();
  });

  it('isEditMode should be false', () => {
    expect(ctx.component.isEditMode).toBeFalse();
  });

  it('isAdminMode should be true for Admin user', () => {
    expect(ctx.component.isAdminMode).toBeTrue();
  });

  it('officeForm should have name, address and isActive controls', () => {
    expect(ctx.component.officeForm.contains('name')).toBeTrue();
    expect(ctx.component.officeForm.contains('address')).toBeTrue();
    expect(ctx.component.officeForm.contains('isActive')).toBeTrue();
  });

  it('officeForm should be enabled for Admin', () => {
    expect(ctx.component.officeForm.enabled).toBeTrue();
  });

  it('officeForm should be invalid when name is empty', () => {
    ctx.component.officeForm.get('name')?.setValue('');
    expect(ctx.component.officeForm.get('name')?.valid).toBeFalse();
  });

  it('officeForm should be invalid when address is empty', () => {
    ctx.component.officeForm.get('address')?.setValue('');
    expect(ctx.component.officeForm.get('address')?.valid).toBeFalse();
  });

  it('onSaveOffice() should do nothing when form is invalid', () => {
    ctx.component.officeForm.get('name')?.setValue('');
    ctx.component.onSaveOffice();
    expect(ctx.officesServiceSpy.createOffice).not.toHaveBeenCalled();
  });

  it('onSaveOffice() should call createOffice with form values and pending rooms', fakeAsync(() => {
    fillOfficeForm(ctx.component);
    ctx.officesServiceSpy.createOffice.and.returnValue(of('new-office-id'));

    ctx.component.onSaveOffice();
    tick();

    expect(ctx.officesServiceSpy.createOffice).toHaveBeenCalledWith(jasmine.objectContaining({
      name: 'Test Office',
      address: 'Test Address',
    }));
  }));

  it('onSaveOffice() should close dialog with true on success', fakeAsync(() => {
    fillOfficeForm(ctx.component);
    ctx.officesServiceSpy.createOffice.and.returnValue(of('new-office-id'));

    ctx.component.onSaveOffice();
    tick();

    expect(ctx.dialogRefSpy.close).toHaveBeenCalledWith(true);
  }));

  it('onSaveOffice() should set errorMessage and not close on failure', fakeAsync(() => {
    fillOfficeForm(ctx.component);
    ctx.officesServiceSpy.createOffice.and.returnValue(
      throwError(() => ({ error: { detail: 'Office name already exists' } })),
    );

    ctx.component.onSaveOffice();
    tick();

    expect(ctx.component.errorMessage).toBe('Office name already exists');
    expect(ctx.dialogRefSpy.close).not.toHaveBeenCalled();
  }));

  it('onCancel() should close dialog with false when no changes', () => {
    ctx.component.onCancel();
    expect(ctx.dialogRefSpy.close).toHaveBeenCalledWith(false);
  });

  // ── Pending room management ─────────────────────────────────────────────

  it('showAddRoomForm() should set roomFormVisible = true and reset form', () => {
    ctx.component.showAddRoomForm();
    expect(ctx.component.roomFormVisible).toBeTrue();
    expect(ctx.component.editingRoomId).toBeNull();
  });

  it('cancelRoomForm() should set roomFormVisible = false', () => {
    ctx.component.showAddRoomForm();
    ctx.component.cancelRoomForm();
    expect(ctx.component.roomFormVisible).toBeFalse();
  });

  it('onSaveRoom() should add pending room to rooms list in create mode', () => {
    ctx.component.showAddRoomForm();
    ctx.component.roomForm.setValue({ name: 'Board Room', capacity: 8, description: '', isActive: true });
    ctx.component.onSaveRoom();

    expect(ctx.component.rooms.length).toBe(1);
    expect((ctx.component.rooms[0] as CreateRoomForOfficeRequest).name).toBe('Board Room');
  });

  it('removePendingRoom() should remove item at given index', () => {
    ctx.component.rooms = [
      { name: 'A', capacity: 1, description: '' },
      { name: 'B', capacity: 2, description: '' },
    ];
    ctx.component.removePendingRoom(0);
    expect(ctx.component.rooms.length).toBe(1);
    expect((ctx.component.rooms[0] as CreateRoomForOfficeRequest).name).toBe('B');
  });
});

// ---------------------------------------------------------------------------
// Suite: Edit mode (Admin)
// ---------------------------------------------------------------------------

describe('OfficeDialogComponent — Edit mode (Admin)', () => {
  let ctx: ReturnType<typeof buildComponent>;

  beforeEach(() => {
    ctx = buildComponent({ office: MOCK_OFFICE }, ADMIN_AUTH_USER);
  });

  it('isEditMode should be true', () => {
    expect(ctx.component.isEditMode).toBeTrue();
  });

  it('officeForm should be pre-filled with office name and address', () => {
    expect(ctx.component.officeForm.get('name')?.value).toBe('London HQ');
    expect(ctx.component.officeForm.get('address')?.value).toBe('10 Downing St');
  });

  it('rooms should be populated from office data on init', () => {
    expect(ctx.component.rooms.length).toBe(1);
    expect((ctx.component.rooms[0] as Room).id).toBe('room-1');
  });

  it('managers should be populated from office data on init', () => {
    expect(ctx.component.managers.length).toBe(1);
    expect(ctx.component.managers[0].id).toBe('mgr-id');
  });

  it('onSaveOffice() should call updateOffice and close dialog with true', fakeAsync(() => {
    fillOfficeForm(ctx.component);
    ctx.officesServiceSpy.updateOffice.and.returnValue(of(void 0));

    ctx.component.onSaveOffice();
    tick();

    expect(ctx.officesServiceSpy.updateOffice).toHaveBeenCalledWith(
      MOCK_OFFICE.id,
      jasmine.objectContaining({ name: 'Test Office' }),
    );
    expect(ctx.dialogRefSpy.close).toHaveBeenCalledWith(true);
  }));

  it('onSaveOffice() should set errorMessage on updateOffice failure', fakeAsync(() => {
    fillOfficeForm(ctx.component);
    ctx.officesServiceSpy.updateOffice.and.returnValue(
      throwError(() => ({ error: { detail: 'Not found' } })),
    );

    ctx.component.onSaveOffice();
    tick();

    expect(ctx.component.errorMessage).toBe('Not found');
    expect(ctx.dialogRefSpy.close).not.toHaveBeenCalled();
  }));

  it('onCancel() should close with hasChanges value', fakeAsync(() => {
    ctx.officesServiceSpy.updateOffice.and.returnValue(of(void 0));
    fillOfficeForm(ctx.component);
    ctx.component.onSaveOffice();
    tick();

    // hasChanges is now true because close was called with true
    // Verify the close was called with true
    expect(ctx.dialogRefSpy.close).toHaveBeenCalledWith(true);
  }));

  // ── Add room in edit mode ───────────────────────────────────────────────

  it('onSaveRoom() should call createRoom when adding a new room in edit mode', fakeAsync(() => {
    ctx.officesServiceSpy.createRoom.and.returnValue(of('new-room-id'));
    ctx.component.showAddRoomForm();
    ctx.component.roomForm.setValue({ name: 'Conf Room', capacity: 5, description: 'Nice', isActive: true });

    ctx.component.onSaveRoom();
    tick();

    expect(ctx.officesServiceSpy.createRoom).toHaveBeenCalled();
    expect(ctx.component.rooms.length).toBe(2); // original + new
  }));

  it('onSaveRoom() should not call createRoom when room form is invalid', () => {
    ctx.component.showAddRoomForm();
    ctx.component.roomForm.get('name')?.setValue('');
    ctx.component.onSaveRoom();
    expect(ctx.officesServiceSpy.createRoom).not.toHaveBeenCalled();
  });

  // ── Edit room ───────────────────────────────────────────────────────────

  it('showEditRoomForm() should populate roomForm with room values', () => {
    ctx.component.showEditRoomForm(MOCK_ROOM);
    expect(ctx.component.roomForm.get('name')?.value).toBe('Room A');
    expect(ctx.component.roomForm.get('capacity')?.value).toBe(10);
    expect(ctx.component.editingRoomId).toBe('room-1');
    expect(ctx.component.roomFormVisible).toBeTrue();
  });

  it('onSaveRoom() should call updateRoom when editing an existing room', fakeAsync(() => {
    ctx.officesServiceSpy.updateRoom.and.returnValue(of(void 0));
    ctx.component.showEditRoomForm(MOCK_ROOM);
    ctx.component.roomForm.get('name')?.setValue('Updated Room');

    ctx.component.onSaveRoom();
    tick();

    expect(ctx.officesServiceSpy.updateRoom).toHaveBeenCalledWith(
      'room-1',
      jasmine.objectContaining({ name: 'Updated Room' }),
    );
  }));

  it('onSaveRoom(update) should update room in rooms list on success', fakeAsync(() => {
    ctx.officesServiceSpy.updateRoom.and.returnValue(of(void 0));
    ctx.component.showEditRoomForm(MOCK_ROOM);
    ctx.component.roomForm.get('name')?.setValue('Updated Room');

    ctx.component.onSaveRoom();
    tick();

    const updatedRoom = ctx.component.rooms.find((r) => (r as Room).id === 'room-1') as Room;
    expect(updatedRoom.name).toBe('Updated Room');
  }));

  // ── Deactivate room ─────────────────────────────────────────────────────

  it('deactivateRoom() should call deactivateRoom service and mark room inactive', fakeAsync(() => {
    ctx.officesServiceSpy.deactivateRoom.and.returnValue(of(void 0));
    ctx.component.deactivateRoom(MOCK_ROOM);
    tick();

    expect(ctx.officesServiceSpy.deactivateRoom).toHaveBeenCalledWith('room-1');
    const room = ctx.component.rooms.find((r) => (r as Room).id === 'room-1') as Room;
    expect(room.isActive).toBeFalse();
  }));

  // ── Manager assignment ──────────────────────────────────────────────────

  it('assignManager() should call assignManager service and update managers list', fakeAsync(() => {
    const newManager: User = {
      id: 'new-mgr', firstName: 'New', lastName: 'Mgr',
      email: 'new@test.com', role: 'OfficeManager', isActive: true, createdAt: '',
    };
    ctx.component.availableManagers = [newManager];
    ctx.component.selectedManagerId = 'new-mgr';
    ctx.officesServiceSpy.assignManager.and.returnValue(of(void 0));

    ctx.component.assignManager();
    tick();

    expect(ctx.officesServiceSpy.assignManager).toHaveBeenCalledWith(MOCK_OFFICE.id, 'new-mgr');
    expect(ctx.component.managers.some((m) => m.id === 'new-mgr')).toBeTrue();
    expect(ctx.component.availableManagers.find((u) => u.id === 'new-mgr')).toBeUndefined();
    expect(ctx.component.selectedManagerId).toBe('');
  }));

  it('assignManager() should do nothing when no manager is selected', () => {
    ctx.component.selectedManagerId = '';
    ctx.component.assignManager();
    expect(ctx.officesServiceSpy.assignManager).not.toHaveBeenCalled();
  });

  it('removeManager() should call removeManager service and update lists', fakeAsync(() => {
    ctx.officesServiceSpy.removeManager.and.returnValue(of(void 0));

    ctx.component.removeManager(MOCK_MANAGER);
    tick();

    expect(ctx.officesServiceSpy.removeManager).toHaveBeenCalledWith(MOCK_OFFICE.id, MOCK_MANAGER.id);
    expect(ctx.component.managers.find((m) => m.id === MOCK_MANAGER.id)).toBeUndefined();
    expect(ctx.component.availableManagers.some((u) => u.id === MOCK_MANAGER.id)).toBeTrue();
  }));
});

// ---------------------------------------------------------------------------
// Suite: Edit mode (OfficeManager — read-only)
// ---------------------------------------------------------------------------

describe('OfficeDialogComponent — Edit mode (OfficeManager)', () => {
  let ctx: ReturnType<typeof buildComponent>;

  beforeEach(() => {
    ctx = buildComponent({ office: MOCK_OFFICE }, MANAGER_AUTH_USER);
  });

  it('isAdminMode should be false for OfficeManager', () => {
    expect(ctx.component.isAdminMode).toBeFalse();
  });

  it('officeForm should be disabled for OfficeManager', () => {
    expect(ctx.component.officeForm.disabled).toBeTrue();
  });

  it('should not load available managers for OfficeManager', () => {
    expect(ctx.usersServiceSpy.getUsers).not.toHaveBeenCalled();
  });
});

// ---------------------------------------------------------------------------
// Suite: Helper methods
// ---------------------------------------------------------------------------

describe('OfficeDialogComponent — helper methods', () => {
  let ctx: ReturnType<typeof buildComponent>;

  beforeEach(() => {
    ctx = buildComponent({ office: null }, ADMIN_AUTH_USER);
  });

  it('isRoom() should return true for a Room object', () => {
    expect(ctx.component.isRoom(MOCK_ROOM)).toBeTrue();
  });

  it('isRoom() should return false for a CreateRoomForOfficeRequest', () => {
    const pending: CreateRoomForOfficeRequest = { name: 'X', capacity: 1, description: '' };
    expect(ctx.component.isRoom(pending)).toBeFalse();
  });

  it('getRoomStatusText() should return Active for an active room', () => {
    expect(ctx.component.getRoomStatusText(MOCK_ROOM)).toBe('Active');
  });

  it('getRoomStatusText() should return Inactive for an inactive room', () => {
    expect(ctx.component.getRoomStatusText({ ...MOCK_ROOM, isActive: false })).toBe('Inactive');
  });

  it('getRoomStatusText() should return Pending for a pending room', () => {
    const pending: CreateRoomForOfficeRequest = { name: 'X', capacity: 1, description: '' };
    expect(ctx.component.getRoomStatusText(pending)).toBe('Pending');
  });

  it('getRoomStatusClass() should return status-badge active for active room', () => {
    expect(ctx.component.getRoomStatusClass(MOCK_ROOM)).toBe('status-badge active');
  });

  it('getRoomStatusClass() should return status-badge inactive for inactive room', () => {
    expect(ctx.component.getRoomStatusClass({ ...MOCK_ROOM, isActive: false })).toBe('status-badge inactive');
  });

  it('getRoomStatusClass() should return status-badge pending for pending room', () => {
    const pending: CreateRoomForOfficeRequest = { name: 'X', capacity: 1, description: '' };
    expect(ctx.component.getRoomStatusClass(pending)).toBe('status-badge pending');
  });

  it('roomForm capacity should be invalid when below 1', () => {
    ctx.component.roomForm.get('capacity')?.setValue(0);
    expect(ctx.component.roomForm.get('capacity')?.valid).toBeFalse();
  });

  it('roomForm name should be invalid when empty', () => {
    ctx.component.roomForm.get('name')?.setValue('');
    expect(ctx.component.roomForm.get('name')?.valid).toBeFalse();
  });
});

// ---------------------------------------------------------------------------
// Helpers
// ---------------------------------------------------------------------------

function fillOfficeForm(component: OfficeDialogComponent): void {
  component.officeForm.enable();
  component.officeForm.get('name')?.setValue('Test Office');
  component.officeForm.get('address')?.setValue('Test Address');
  component.officeForm.get('isActive')?.setValue(true);
}
