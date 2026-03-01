import { Component, Inject, OnInit } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MAT_DIALOG_DATA, MatDialogRef } from '@angular/material/dialog';
import { MatSnackBar } from '@angular/material/snack-bar';
import { OfficesService } from '../../../core/services/offices.service';
import { UsersService } from '../../../core/services/users.service';
import { AuthService } from '../../../core/services/auth.service';
import {
  CreateOfficeRequest,
  CreateRoomForOfficeRequest,
  CreateRoomRequest,
  ManagerSummary,
  Office,
  Room,
  UpdateOfficeRequest,
  UpdateRoomRequest,
  User,
  PagedResult,
} from '../../../models/entities.model';

export interface OfficeDialogData {
  office: Office | null;
}

@Component({
  selector: 'app-office-dialog',
  templateUrl: './office-dialog.component.html',
  styleUrls: ['./office-dialog.component.scss'],
})
export class OfficeDialogComponent implements OnInit {
  public readonly isEditMode: boolean;
  /** True when the calling user is Admin — enables office editing and manager assignment. */
  public readonly isAdminMode: boolean;

  /** Main office form */
  public readonly officeForm: FormGroup;
  /** Inline room add/edit form */
  public readonly roomForm: FormGroup;

  /** Rooms list shown in dialog (edit mode = from API; create mode = pending list) */
  public rooms: (Room | CreateRoomForOfficeRequest)[] = [];
  public readonly roomColumns: string[] = ['name', 'capacity', 'description', 'status', 'actions'];

  public roomFormVisible: boolean = false;
  /** id of the room being edited (edit mode), or null when adding */
  public editingRoomId: string | null = null;

  /** Assigned managers for the current office (edit mode only) */
  public managers: ManagerSummary[] = [];
  /** All OfficeManager users available to assign */
  public availableManagers: User[] = [];
  public selectedManagerId: string = '';
  public readonly managerColumns: string[] = ['name', 'email', 'actions'];

  public isSubmitting: boolean = false;
  public errorMessage: string | null = null;
  public hasChanges: boolean = false;

  constructor(
    @Inject(MAT_DIALOG_DATA) public readonly data: OfficeDialogData,
    private dialogRef: MatDialogRef<OfficeDialogComponent>,
    private fb: FormBuilder,
    private officesService: OfficesService,
    private usersService: UsersService,
    private authService: AuthService,
    private snackBar: MatSnackBar,
  ) {
    this.isEditMode = data.office !== null;
    this.isAdminMode = authService.getCurrentUser()?.role === 'Admin';

    this.officeForm = this.fb.group({
      name: [data.office?.name ?? '', [Validators.required, Validators.maxLength(200)]],
      address: [data.office?.address ?? '', [Validators.required, Validators.maxLength(500)]],
      isActive: [data.office?.isActive ?? true],
    });

    if (!this.isAdminMode) {
      this.officeForm.disable();
    }

    this.roomForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(200)]],
      capacity: [1, [Validators.required, Validators.min(1), Validators.max(500)]],
      description: ['', [Validators.maxLength(1000)]],
      isActive: [true],
    });
  }

  public ngOnInit(): void {
    if (this.isEditMode && this.data.office) {
      this.rooms = [...(this.data.office.rooms ?? [])];
      this.managers = [...(this.data.office.managers ?? [])];
    }
    if (this.isAdminMode) {
      this.loadAvailableManagers();
    }
  }

  // ─── Available managers ─────────────────────────────────────────────────────

  private loadAvailableManagers(): void {
    this.usersService.getUsers(1, 1000).subscribe({
      next: (result: PagedResult<User>): void => {
        const assignedIds = new Set(this.managers.map((m) => m.id));
        this.availableManagers = result.items.filter(
          (u) => u.role === 'OfficeManager' && u.isActive && !assignedIds.has(u.id),
        );
      },
    });
  }

  // ─── Office form ────────────────────────────────────────────────────────────

  public onSaveOffice(): void {
    if (this.officeForm.invalid || !this.isAdminMode) return;

    this.isSubmitting = true;
    this.errorMessage = null;

    if (this.isEditMode) {
      this.saveExistingOffice();
    } else {
      this.createNewOffice();
    }
  }

  private createNewOffice(): void {
    const v = this.officeForm.value;
    const request: CreateOfficeRequest = {
      name: v.name,
      address: v.address,
      isActive: v.isActive,
      rooms: this.rooms as CreateRoomForOfficeRequest[],
    };

    this.officesService.createOffice(request).subscribe({
      next: (): void => {
        this.isSubmitting = false;
        this.dialogRef.close(true);
      },
      error: (err: { error?: { detail?: string; title?: string } }): void => {
        this.isSubmitting = false;
        this.errorMessage =
          err.error?.detail ?? err.error?.title ?? 'Failed to create office. Please try again.';
      },
    });
  }

  private saveExistingOffice(): void {
    const v = this.officeForm.value;
    const request: UpdateOfficeRequest = {
      id: this.data.office!.id,
      name: v.name,
      address: v.address,
      isActive: v.isActive,
    };

    this.officesService.updateOffice(this.data.office!.id, request).subscribe({
      next: (): void => {
        this.isSubmitting = false;
        this.snackBar.open('Office details saved.', 'Dismiss', { duration: 3000 });
        this.dialogRef.close(true);
      },
      error: (err: { error?: { detail?: string; title?: string } }): void => {
        this.isSubmitting = false;
        this.errorMessage =
          err.error?.detail ?? err.error?.title ?? 'Failed to update office. Please try again.';
      },
    });
  }

  public onCancel(): void {
    this.dialogRef.close(this.hasChanges);
  }

  // ─── Manager assignment ──────────────────────────────────────────────────────

  public assignManager(): void {
    if (!this.selectedManagerId || !this.data.office) return;

    this.officesService.assignManager(this.data.office.id, this.selectedManagerId).subscribe({
      next: (): void => {
        const user = this.availableManagers.find((u) => u.id === this.selectedManagerId);
        if (user) {
          const summary: ManagerSummary = {
            id: user.id,
            firstName: user.firstName,
            lastName: user.lastName,
            email: user.email,
          };
          this.managers = [...this.managers, summary];
          this.availableManagers = this.availableManagers.filter((u) => u.id !== this.selectedManagerId);
        }
        this.selectedManagerId = '';
        this.hasChanges = true;
        this.snackBar.open('Manager assigned.', 'Dismiss', { duration: 3000 });
      },
      error: (): void => {
        this.snackBar.open('Failed to assign manager.', 'Dismiss', {
          duration: 4000,
          panelClass: ['error-snackbar'],
        });
      },
    });
  }

  public removeManager(manager: ManagerSummary): void {
    if (!this.data.office) return;

    this.officesService.removeManager(this.data.office.id, manager.id).subscribe({
      next: (): void => {
        this.managers = this.managers.filter((m) => m.id !== manager.id);
        this.availableManagers = [...this.availableManagers, { id: manager.id, firstName: manager.firstName, lastName: manager.lastName, email: manager.email, role: 'OfficeManager', isActive: true, createdAt: '' }];
        this.hasChanges = true;
        this.snackBar.open('Manager removed.', 'Dismiss', { duration: 3000 });
      },
      error: (): void => {
        this.snackBar.open('Failed to remove manager.', 'Dismiss', {
          duration: 4000,
          panelClass: ['error-snackbar'],
        });
      },
    });
  }

  // ─── Room form ───────────────────────────────────────────────────────────────

  public showAddRoomForm(): void {
    this.editingRoomId = null;
    this.roomForm.reset({ name: '', capacity: 1, description: '', isActive: true });
    this.roomFormVisible = true;
  }

  public showEditRoomForm(room: Room): void {
    this.editingRoomId = room.id;
    this.roomForm.setValue({
      name: room.name,
      capacity: room.capacity,
      description: room.description ?? '',
      isActive: room.isActive,
    });
    this.roomFormVisible = true;
  }

  public cancelRoomForm(): void {
    this.roomFormVisible = false;
    this.editingRoomId = null;
    this.roomForm.reset({ name: '', capacity: 1, description: '', isActive: true });
  }

  public onSaveRoom(): void {
    if (this.roomForm.invalid) return;

    const v = this.roomForm.value;

    if (!this.isEditMode) {
      const pending: CreateRoomForOfficeRequest = {
        name: v.name,
        capacity: v.capacity,
        description: v.description ?? '',
      };
      this.rooms = [...this.rooms, pending];
      this.cancelRoomForm();
      return;
    }

    this.roomForm.disable();

    if (this.editingRoomId) {
      const request: UpdateRoomRequest = {
        id: this.editingRoomId,
        name: v.name,
        capacity: v.capacity,
        description: v.description ?? '',
        isActive: v.isActive,
      };
      this.officesService.updateRoom(this.editingRoomId, request).subscribe({
        next: (): void => {
          this.rooms = this.rooms.map((r) =>
            (r as Room).id === this.editingRoomId ? { ...(r as Room), ...request } : r,
          );
          this.hasChanges = true;
          this.snackBar.open('Room updated.', 'Dismiss', { duration: 3000 });
          this.roomForm.enable();
          this.cancelRoomForm();
        },
        error: (): void => {
          this.snackBar.open('Failed to update room. Please try again.', 'Dismiss', {
            duration: 4000,
            panelClass: ['error-snackbar'],
          });
          this.roomForm.enable();
        },
      });
    } else {
      const request: CreateRoomRequest = {
        officeId: this.data.office!.id,
        name: v.name,
        capacity: v.capacity,
        description: v.description ?? '',
      };
      this.officesService.createRoom(request).subscribe({
        next: (newId: string): void => {
          const newRoom: Room = {
            id: newId,
            officeId: this.data.office!.id,
            name: v.name,
            capacity: v.capacity,
            description: v.description ?? '',
            isActive: true,
          };
          this.rooms = [...this.rooms, newRoom];
          this.hasChanges = true;
          this.snackBar.open('Room added.', 'Dismiss', { duration: 3000 });
          this.roomForm.enable();
          this.cancelRoomForm();
        },
        error: (): void => {
          this.snackBar.open('Failed to add room. Please try again.', 'Dismiss', {
            duration: 4000,
            panelClass: ['error-snackbar'],
          });
          this.roomForm.enable();
        },
      });
    }
  }

  public deactivateRoom(room: Room): void {
    this.officesService.deactivateRoom(room.id).subscribe({
      next: (): void => {
        this.rooms = this.rooms.map((r) =>
          (r as Room).id === room.id ? { ...(r as Room), isActive: false } : r,
        );
        this.hasChanges = true;
        this.snackBar.open(`Room "${room.name}" deactivated.`, 'Dismiss', { duration: 3000 });
      },
      error: (): void => {
        this.snackBar.open('Failed to deactivate room.', 'Dismiss', {
          duration: 4000,
          panelClass: ['error-snackbar'],
        });
      },
    });
  }

  public removePendingRoom(index: number): void {
    this.rooms = this.rooms.filter((_, i) => i !== index);
  }

  public isRoom(r: Room | CreateRoomForOfficeRequest): r is Room {
    return 'id' in r;
  }

  public getRoomStatusText(r: Room | CreateRoomForOfficeRequest): string {
    if (!this.isRoom(r)) return 'Pending';
    return r.isActive ? 'Active' : 'Inactive';
  }

  public getRoomStatusClass(r: Room | CreateRoomForOfficeRequest): string {
    if (!this.isRoom(r)) return 'status-badge pending';
    return r.isActive ? 'status-badge active' : 'status-badge inactive';
  }

  public onEditRoomClick(r: Room | CreateRoomForOfficeRequest): void {
    if (this.isRoom(r)) { this.showEditRoomForm(r); }
  }

  public onDeactivateRoomClick(r: Room | CreateRoomForOfficeRequest): void {
    if (this.isRoom(r)) { this.deactivateRoom(r); }
  }
}
