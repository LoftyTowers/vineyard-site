import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { SHARED_IMPORTS } from '../../shared/shared-imports';
import { MatSnackBar } from '@angular/material/snack-bar';

interface AuditEntry {
  id: string;
  user?: { username: string };
  action: string;
  entityType: string;
  timestamp: string;
}

@Component({
  selector: 'app-activity-log',
  imports: [...SHARED_IMPORTS],
  templateUrl: './activity-log.component.html',
  styleUrl: './activity-log.component.scss'
})
export class ActivityLogComponent implements OnInit {
  logs: AuditEntry[] = [];
  loading = false;

  constructor(private http: HttpClient, private snackBar: MatSnackBar) {}

  ngOnInit(): void {
    this.loading = true;
    this.http.get<AuditEntry[]>('/api/audit').subscribe({
      next: (data) => {
        this.logs = data;
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.snackBar.open('Failed to load activity log', 'Close', { duration: 3000 });
      }
    });
  }
}
