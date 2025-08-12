import { Component, OnDestroy, OnInit } from '@angular/core';
import { SHARED_IMPORTS } from '../../shared/shared-imports';
import { HealthService, HealthStatus } from '../../services/health.service';
import { CONFIG } from '../../config';

@Component({
  selector: 'app-status',
  imports: [...SHARED_IMPORTS],
  templateUrl: './status.component.html',
  styleUrl: './status.component.scss'
})
export class StatusComponent implements OnInit, OnDestroy {
  status?: HealthStatus;
  loading = false;
  private intervalId?: any;

  constructor(private health: HealthService) {}

  ngOnInit(): void {
    if (CONFIG.healthCheck.enabled) {
      this.ping();
      this.intervalId = setInterval(() => this.ping(), CONFIG.healthCheck.intervalMs);
    }
  }

  ngOnDestroy(): void {
    if (this.intervalId) {
      clearInterval(this.intervalId);
    }
  }

  ping(): void {
    this.loading = true;
    this.health.check().subscribe({
      next: res => {
        this.status = res;
        this.loading = false;
      },
      error: () => {
        this.status = { status: 'Unavailable', checkedAt: new Date().toISOString() };
        this.loading = false;
      }
    });
  }
}
