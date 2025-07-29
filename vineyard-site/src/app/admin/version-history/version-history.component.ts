import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { SHARED_IMPORTS } from '../../shared/shared-imports';
import { MatSnackBar } from '@angular/material/snack-bar';

interface OverrideInfo {
  id: string;
  timestamp: string;
  changedBy?: { username: string };
  note?: string;
}

@Component({
  selector: 'app-version-history',
  imports: [...SHARED_IMPORTS],
  templateUrl: './version-history.component.html',
  styleUrl: './version-history.component.scss'
})
export class VersionHistoryComponent implements OnInit {
  overrides: OverrideInfo[] = [];
  loading = false;

  @Output() revert = new EventEmitter<string>();

  constructor(private http: HttpClient, private snackBar: MatSnackBar) {}

  ngOnInit(): void {
    this.loading = true;
    this.http
      .get<OverrideInfo[]>('/api/overrides/history/home/block0')
      .subscribe({
        next: (data) => {
          this.overrides = data;
          this.loading = false;
        },
        error: () => {
          this.loading = false;
          this.snackBar.open('Failed to load history', 'Close', { duration: 3000 });
        }
      });
  }

  onRevert(id: string): void {
    this.revert.emit(id);
  }
}
