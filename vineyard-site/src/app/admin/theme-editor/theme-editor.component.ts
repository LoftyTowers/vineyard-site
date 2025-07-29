import { Component, OnInit } from '@angular/core';
import { SHARED_IMPORTS } from '../../shared/shared-imports';
import { HttpClient } from '@angular/common/http';

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

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.http.get<Record<string, string>>('/api/branding-overrides').subscribe((data) => {
      this.theme = { ...data };
    });
  }

  save(): void {
    const note = window.prompt('Add a note describing these changes:');
    if (note === null) {
      return;
    }
    const payload = { theme: this.theme, status: this.status, note };
    this.http.post('/api/branding-overrides', payload).subscribe({
      next: () => window.alert('Overrides saved successfully'),
      error: () => window.alert('Failed to save overrides')
    });
  }

  publish(): void {
    this.http.post('/api/branding-overrides/publish', {}).subscribe({
      next: () => window.alert('Published successfully'),
      error: () => window.alert('Publish failed')
    });
  }

  revert(): void {
    this.http.post('/api/branding-overrides/revert', {}).subscribe({
      next: () => window.alert('Reverted successfully'),
      error: () => window.alert('Revert failed')
    });
  }
}
