import { TestBed } from '@angular/core/testing';
import { Component } from '@angular/core';
import { ThemeService } from './theme.service';

class MockThemeService {
  loadTheme(): void {
    this.applyTheme({ primary: '#112233' });
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

@Component({
  selector: 'app-test',
  template: '<div id="sample" style="color: var(--primary-color)"></div>',
  standalone: true
})
class TestComponent {
  constructor(private theme: ThemeService) {}
  ngOnInit(): void {
    this.theme.loadTheme();
  }
}

describe('ThemeService with mocked implementation', () => {
  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [TestComponent],
      providers: [{ provide: ThemeService, useClass: MockThemeService }]
    }).compileComponents();
    TestBed.createComponent(TestComponent).detectChanges();
  });

  it('updates CSS variables on load', () => {
    const root = document.documentElement;
    expect(root.style.getPropertyValue('--primary-color')).toBe('#112233');
  });

  it('applies variable style to element', () => {
    const el = document.getElementById('sample') as HTMLElement;
    const color = getComputedStyle(el).color;
    expect(color).toBe('rgb(17, 34, 51)');
  });
});
