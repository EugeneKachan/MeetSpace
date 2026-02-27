import { Injectable } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';
import { Router } from '@angular/router';
import { BehaviorSubject, Observable, throwError } from 'rxjs';
import { tap, catchError } from 'rxjs/operators';
import { JwtHelperService } from '@auth0/angular-jwt';
import { environment } from '../../../environments/environment';
import { User, TokenResponse, JwtClaims, UserRole } from '../../models/auth.model';

const TOKEN_KEY = 'access_token';
const REFRESH_KEY = 'refresh_token';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenEndpoint = `${environment.apiUrl}/connect/token`;
  private readonly jwtHelper = new JwtHelperService();
  private currentUserSubject = new BehaviorSubject<User | null>(this.parseStoredToken());

  public currentUser$: Observable<User | null> = this.currentUserSubject.asObservable();

  constructor(private http: HttpClient, private router: Router) {}

  /** POST /connect/token — OAuth2 ROPC grant */
  login(email: string, password: string): Observable<TokenResponse> {
    const body = new URLSearchParams({
      grant_type: 'password',
      client_id: 'meetspase-angular',
      username: email,
      password: password,
      scope: 'openid profile roles api'
    });

    const headers = new HttpHeaders({ 'Content-Type': 'application/x-www-form-urlencoded' });

    return this.http.post<TokenResponse>(this.tokenEndpoint, body.toString(), { headers }).pipe(
      tap(response => this.handleTokenResponse(response)),
      catchError(err => {
        const message = err?.error?.error_description ?? err?.error?.error ?? 'Login failed';
        return throwError(() => new Error(message));
      })
    );
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(REFRESH_KEY);
    this.currentUserSubject.next(null);
    this.router.navigate(['/login']);
  }

  getToken(): string | null {
    return localStorage.getItem(TOKEN_KEY);
  }

  isAuthenticated(): boolean {
    const token = this.getToken();
    if (!token) return false;
    try {
      return !this.jwtHelper.isTokenExpired(token);
    } catch {
      // Token exists in localStorage but isn't a valid JWT — clear it
      localStorage.removeItem(TOKEN_KEY);
      localStorage.removeItem(REFRESH_KEY);
      this.currentUserSubject.next(null);
      return false;
    }
  }

  getCurrentUser(): User | null {
    return this.currentUserSubject.value;
  }

  hasRole(roles: string[]): boolean {
    const user = this.currentUserSubject.value;
    return !!user && roles.includes(user.role);
  }

  // ----------------------------------------------------------------

  private handleTokenResponse(response: TokenResponse): void {
    localStorage.setItem(TOKEN_KEY, response.access_token);
    if (response.refresh_token) {
      localStorage.setItem(REFRESH_KEY, response.refresh_token);
    }
    const user = this.decodeUser(response.access_token);
    this.currentUserSubject.next(user);
  }

  private decodeUser(token: string): User | null {
    try {
      const claims = this.jwtHelper.decodeToken<JwtClaims>(token);
      if (!claims) return null;
      const role = Array.isArray(claims.role) ? (claims.role[0] as UserRole) : (claims.role as UserRole);
      const name = [claims.given_name, claims.family_name].filter(Boolean).join(' ') || claims.name;
      return {
        id: claims.sub,
        email: claims.email,
        name,
        role
      };
    } catch {
      return null;
    }
  }

  private parseStoredToken(): User | null {
    const token = localStorage.getItem(TOKEN_KEY);
    if (!token) return null;
    try {
      if (new JwtHelperService().isTokenExpired(token)) {
        localStorage.removeItem(TOKEN_KEY);
        localStorage.removeItem(REFRESH_KEY);
        return null;
      }
      return this.decodeUser(token);
    } catch {
      localStorage.removeItem(TOKEN_KEY);
      localStorage.removeItem(REFRESH_KEY);
      return null;
    }
  }
}
