import { Component, OnInit } from '@angular/core';
import { AuthService } from '../../core/services/auth.service';
import { User } from '../../models/auth.model';

@Component({
  selector: 'app-dashboard',
  templateUrl: './dashboard.component.html',
  styleUrls: ['./dashboard.component.scss']
})
export class DashboardComponent implements OnInit {
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
