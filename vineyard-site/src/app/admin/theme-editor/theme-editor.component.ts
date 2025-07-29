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

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.http.get<Record<string, string>>('/api/branding-overrides').subscribe((data) => {
      this.theme = { ...data };
    });
  }

  save(): void {
    this.http.post('/api/branding-overrides', this.theme).subscribe();
  }
}
