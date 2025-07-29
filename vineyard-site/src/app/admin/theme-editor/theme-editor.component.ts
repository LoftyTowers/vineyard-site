import { Component, OnInit } from '@angular/core';
import { SHARED_IMPORTS } from '../../shared/shared-imports';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../services/auth.service';
import { MatSnackBar } from '@angular/material/snack-bar';

@Component({
  selector: 'app-theme-editor',
  imports: [...SHARED_IMPORTS],
  templateUrl: './theme-editor.component.html',
  styleUrl: './theme-editor.component.scss'
})
export class ThemeEditorComponent implements OnInit {
  theme: Record<string, string> = {};
  colorKeys = ['primary', 'secondary', 'accent', 'background', 'navbar', 'navbar-border', 'contrast'];
  fontKeys = ['heading font', 'body font'];
  fonts = [
    '"Playfair Display", serif',
    'Lora, serif',
    'Arial, sans-serif',
    'Helvetica, sans-serif',
    'Georgia, serif'
  ];

  statusOptions = ['draft', 'published'];
  status = this.statusOptions[0];
  isAdmin = false;
  loading = false;
  constructor(private http: HttpClient, private auth: AuthService, private snackBar: MatSnackBar) {}

  ngOnInit(): void {
    this.isAdmin = this.auth.hasRole('Admin');
    this.loading = true;
    this.http.get<Record<string, string>>('/api/branding-overrides').subscribe({
      next: (data) => {
        this.theme = { ...data };
        this.loading = false;
      },
      error: () => {
        this.loading = false;
        this.snackBar.open('Failed to load overrides', 'Close', { duration: 3000 });
      }
    });
  }

  save(): void {
    const note = window.prompt('Add a note describing these changes:');
    if (note === null) {
      return;
    }
    const payload = { theme: this.theme, status: this.status, note };
    this.loading = true;
    this.http.post('/api/branding-overrides', payload).subscribe({
      next: () => {
        this.loading = false;
        this.snackBar.open('Overrides saved successfully', 'Close', { duration: 3000 });
      },
      error: () => {
        this.loading = false;
        this.snackBar.open('Failed to save overrides', 'Close', { duration: 3000 });
      }
    });
  }

  publish(): void {
    this.loading = true;
    this.http.post('/api/branding-overrides/publish', {}).subscribe({
      next: () => {
        this.loading = false;
        this.snackBar.open('Published successfully', 'Close', { duration: 3000 });
      },
      error: () => {
        this.loading = false;
        this.snackBar.open('Publish failed', 'Close', { duration: 3000 });
      }
    });
  }

  revert(): void {
    this.loading = true;
    this.http.post('/api/branding-overrides/revert', {}).subscribe({
      next: () => {
        this.loading = false;
        this.snackBar.open('Reverted successfully', 'Close', { duration: 3000 });
      },
      error: () => {
        this.loading = false;
        this.snackBar.open('Revert failed', 'Close', { duration: 3000 });
      }
    });
  }
}
