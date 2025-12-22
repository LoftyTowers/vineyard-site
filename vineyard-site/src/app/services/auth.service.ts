import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject } from 'rxjs';
import { tap } from 'rxjs/operators';
import { jwtDecode } from 'jwt-decode';

interface LoginResponse { token: string; }

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenKey = 'auth_token';
  private authState = new BehaviorSubject<string | null>(localStorage.getItem(this.tokenKey));

  constructor(private http: HttpClient) {}

  login(username: string, password: string) {
    return this.http
      .post<LoginResponse>('/api/auth/login', { username, password })
      .pipe(
        tap(res => {
          localStorage.setItem(this.tokenKey, res.token);
          this.authState.next(res.token);
        })
      );
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
    this.authState.next(null);
  }

  get token(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  get authState$() {
    return this.authState.asObservable();
  }

  get roles(): string[] {
    const token = this.token;
    if (!token) return [];
    try {
      const decoded = jwtDecode<Record<string, unknown>>(token);
      const role = decoded['role'];
      if (!role) return [];
      return Array.isArray(role)
        ? role.filter((entry): entry is string => typeof entry === 'string')
        : typeof role === 'string'
          ? [role]
          : [];
    } catch {
      return [];
    }
  }

  hasRole(role: string): boolean {
    return this.roles.includes(role);
  }
}
