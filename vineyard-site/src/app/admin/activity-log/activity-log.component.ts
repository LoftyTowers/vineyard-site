import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { SHARED_IMPORTS } from '../../shared/shared-imports';

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

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.http.get<AuditEntry[]>('/api/audit').subscribe((data) => (this.logs = data));
  }
}
