import { ComponentFixture, TestBed } from '@angular/core/testing';
import { NotFoundComponent } from './not-found.component';

describe('NotFoundComponent', () => {
  let fixture: ComponentFixture<NotFoundComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [NotFoundComponent]
    }).compileComponents();
    fixture = TestBed.createComponent(NotFoundComponent);
  });

  it('renders Page Not Found text', () => {
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Page Not Found');
  });
});
