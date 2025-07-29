import { Component, EventEmitter, OnInit, Output } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { SHARED_IMPORTS } from '../../shared/shared-imports';

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

  @Output() revert = new EventEmitter<string>();

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.http
      .get<OverrideInfo[]>('/api/overrides/history/home/block0')
      .subscribe((data) => (this.overrides = data));
  }

  onRevert(id: string): void {
    this.revert.emit(id);
  }
}
