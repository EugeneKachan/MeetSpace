import { Component, OnInit } from '@angular/core';
import { Observable } from 'rxjs';
import { AuthService } from './core/services/auth.service';
import { User } from './models/auth.model';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.scss']
})
export class AppComponent implements OnInit {
  public currentUser$!: Observable<User | null>;
  public sidenavCollapsed: boolean = false;

  constructor(private authService: AuthService) {}

  public ngOnInit(): void {
    this.currentUser$ = this.authService.currentUser$;
  }

  toggleSidenav(): void {
    this.sidenavCollapsed = !this.sidenavCollapsed;
  }

  public logout(): void {
    this.authService.logout();
  }
}
