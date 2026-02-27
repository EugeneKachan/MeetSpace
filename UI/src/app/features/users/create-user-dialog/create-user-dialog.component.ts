import { Component } from '@angular/core';
import { FormBuilder, FormGroup, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { UsersService } from '../../../core/services/users.service';
import { CreateUserRequest, User } from '../../../models/entities.model';

export const USER_ROLES = ['Employee', 'OfficeManager', 'Admin'];

@Component({
  selector: 'app-create-user-dialog',
  templateUrl: './create-user-dialog.component.html',
  styleUrls: ['./create-user-dialog.component.scss']
})
export class CreateUserDialogComponent {
  public readonly form: FormGroup;
  public readonly roles: string[] = USER_ROLES;
  public isLoading: boolean = false;
  public errorMessage: string | null = null;
  public hidePassword: boolean = true;

  constructor(
    private fb: FormBuilder,
    private dialogRef: MatDialogRef<CreateUserDialogComponent>,
    private usersService: UsersService
  ) {
    this.form = this.fb.group({
      firstName: ['', [Validators.required, Validators.maxLength(100)]],
      lastName: ['', [Validators.required, Validators.maxLength(100)]],
      email: ['', [Validators.required, Validators.email]],
      password: [
        '',
        [
          Validators.required,
          Validators.minLength(8),
          Validators.pattern(/^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$/)
        ]
      ],
      role: ['Employee', Validators.required],
      isActive: [true]
    });
  }

  onSubmit(): void {
    if (this.form.invalid) return;

    this.isLoading = true;
    this.errorMessage = null;

    const request: CreateUserRequest = this.form.value;

    this.usersService.createUser(request).subscribe({
      next: (user: User): void => {
        this.isLoading = false;
        this.dialogRef.close(user);
      },
      error: (err: { error?: { detail?: string; title?: string } }): void => {
        this.isLoading = false;
        this.errorMessage =
          err.error?.detail ?? err.error?.title ?? 'Failed to create user. Please try again.';
      }
    });
  }

  onCancel(): void {
    this.dialogRef.close();
  }
}
