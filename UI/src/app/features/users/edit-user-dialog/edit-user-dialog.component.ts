import { Component, Inject } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef, MAT_DIALOG_DATA } from '@angular/material/dialog';
import { UsersService } from '../../../core/services/users.service';
import { User, UpdateUserRequest } from '../../../models/entities.model';
import { USER_ROLES } from '../create-user-dialog/create-user-dialog.component';

@Component({
  selector: 'app-edit-user-dialog',
  templateUrl: './edit-user-dialog.component.html',
  styleUrls: ['./edit-user-dialog.component.scss']
})
export class EditUserDialogComponent {
  public readonly form: FormGroup;
  public readonly roles: string[] = USER_ROLES;
  public isLoading: boolean = false;
  public errorMessage: string | null = null;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<EditUserDialogComponent>,
    @Inject(MAT_DIALOG_DATA) public data: User,
    private usersService: UsersService
  ) {
    this.form = this.fb.group({
      firstName: [data.firstName, [Validators.required, Validators.maxLength(100)]],
      lastName: [data.lastName, [Validators.required, Validators.maxLength(100)]],
      email: [data.email, [Validators.required, Validators.email]],
      role: [data.role, Validators.required],
      isActive: [data.isActive]
    });
  }

  onSubmit(): void {
    if (this.form.invalid) return;

    this.isLoading = true;
    this.errorMessage = null;

    const request: UpdateUserRequest = this.form.value;

    this.usersService.updateUser(this.data.id, request).subscribe({
      next: (user: User): void => {
        this.isLoading = false;
        this.dialogRef.close(user);
      },
      error: (err: { error?: { title?: string; detail?: string } }): void => {
        this.isLoading = false;
        this.errorMessage =
          err.error?.title ?? err.error?.detail ?? 'Failed to update user. Please try again.';
      }
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}
