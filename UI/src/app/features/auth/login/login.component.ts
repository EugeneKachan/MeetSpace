import { Component, OnInit } from '@angular/core';
import { AbstractControl, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss']
})
export class LoginComponent implements OnInit {
  public form!: FormGroup;
  public isLoading: boolean = false;
  public errorMessage: string | null = null;
  public hidePassword: boolean = true;

  constructor(
    private fb: FormBuilder,
    private authService: AuthService,
    private router: Router
  ) {}

  ngOnInit(): void {
    if (this.authService.isAuthenticated()) {
      this.router.navigate(['/dashboard']);
      return;
    }

    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required, Validators.minLength(6)]]
    });
  }

  public get email(): AbstractControl { return this.form.get('email')!; }
  public get password(): AbstractControl { return this.form.get('password')!; }

  onSubmit(): void {
    if (this.form.invalid) return;

    this.isLoading = true;
    this.errorMessage = null;

    const { email, password } = this.form.value;

    this.authService.login(email, password).subscribe({
      next: (): void => {
        this.router.navigate(['/dashboard']);
      },
      error: (err: Error): void => {
        this.errorMessage = err.message;
        this.isLoading = false;
      }
    });
  }
}
