import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs/operators';
import { jwtDecode } from 'jwt-decode';

interface LoginResponse { token: string; }

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly tokenKey = 'auth_token';

  constructor(private http: HttpClient) {}

  login(username: string, password: string) {
    return this.http
      .post<LoginResponse>('/api/auth/login', { username, password })
      .pipe(tap(res => localStorage.setItem(this.tokenKey, res.token)));
  }

  logout(): void {
    localStorage.removeItem(this.tokenKey);
  }

  get token(): string | null {
    return localStorage.getItem(this.tokenKey);
  }

  get roles(): string[] {
    const token = this.token;
    if (!token) return [];
    try {
      const decoded: any = jwtDecode(token);
      const role = decoded['role'];
      if (!role) return [];
      return Array.isArray(role) ? role : [role];
    } catch {
      return [];
    }
  }

  hasRole(role: string): boolean {
    return this.roles.includes(role);
  }
}
