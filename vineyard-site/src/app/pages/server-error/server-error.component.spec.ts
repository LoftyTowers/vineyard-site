import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute } from '@angular/router';
import { ServerErrorComponent } from './server-error.component';

describe('ServerErrorComponent', () => {
  let component: ServerErrorComponent;
  let fixture: ComponentFixture<ServerErrorComponent>;

  beforeEach(async () => {
    const routeStub = {
      snapshot: { queryParamMap: { get: (key: string) => key === 'message' ? 'Test Message' : null } }
    } as Partial<ActivatedRoute>;

    await TestBed.configureTestingModule({
      imports: [ServerErrorComponent],
      providers: [{ provide: ActivatedRoute, useValue: routeStub }]
    }).compileComponents();

    fixture = TestBed.createComponent(ServerErrorComponent);
    component = fixture.componentInstance;
  });

  it('shows message from query param', () => {
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('Test Message');
  });
});
