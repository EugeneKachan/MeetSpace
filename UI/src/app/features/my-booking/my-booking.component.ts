import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';
import { User } from '../../models/auth.model';

@Component({
  selector: 'app-my-booking',
  templateUrl: './my-booking.component.html',
  styleUrls: ['./my-booking.component.scss']
})
export class MyBookingComponent implements OnInit {
  public user: User | null = null;

  private readonly roleColor: Record<string, string> = {
    Admin: 'warn',
    OfficeManager: 'accent',
    Employee: 'primary'
  };

  constructor(private authService: AuthService) {}

  ngOnInit(): void {
    this.user = this.authService.getCurrentUser();
  }

  getRoleColor(): string {
    return this.user ? (this.roleColor[this.user.role] ?? 'primary') : 'primary';
  }
}
