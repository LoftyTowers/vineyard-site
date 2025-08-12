import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

export interface HealthStatus {
  status: string;
  checkedAt: string;
}

@Injectable({ providedIn: 'root' })
export class HealthService {
  constructor(private http: HttpClient) {}

  check(): Observable<HealthStatus> {
    return this.http.get<HealthStatus>('/api/health');
  }
}
