import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';

@Injectable({ providedIn: 'root' })
export class ThemeService {
  constructor(private http: HttpClient) {}

  async loadTheme(): Promise<void> {
    try {
      const theme = await firstValueFrom(
        this.http.get<Record<string, string>>('/api/theme')
      );
      this.applyTheme(theme);
    } catch (err) {
      console.error('Failed to load theme', err);
    }
  }

  applyTheme(theme: Record<string, string>): void {
    const root = document.documentElement;
    for (const [key, value] of Object.entries(theme)) {
      const cssKey = key.replace(/\s+/g, '-');
      if (key.includes('font')) {
        root.style.setProperty(`--${cssKey}`, value);
      } else {
        root.style.setProperty(`--${cssKey}-color`, value);
      }
    }
  }
}
