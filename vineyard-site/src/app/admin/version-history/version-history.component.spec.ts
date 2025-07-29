import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { VersionHistoryComponent } from './version-history.component';

describe('VersionHistoryComponent', () => {
  let component: VersionHistoryComponent;
  let fixture: ComponentFixture<VersionHistoryComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [VersionHistoryComponent, HttpClientTestingModule]
    }).compileComponents();

    fixture = TestBed.createComponent(VersionHistoryComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create', () => {
    fixture.detectChanges();
    httpMock.expectOne('/api/overrides/history/home/block0').flush([]);
    expect(component).toBeTruthy();
  });

  it('renders history rows', () => {
    fixture.detectChanges();
    const data = [
      { id: '1', timestamp: '2025-07-29T00:00:00Z', changedBy: { username: 'a' }, note: 'n1' },
      { id: '2', timestamp: '2025-07-30T00:00:00Z', changedBy: { username: 'b' }, note: 'n2' }
    ];
    httpMock.expectOne('/api/overrides/history/home/block0').flush(data);
    fixture.detectChanges();
    const rows = fixture.nativeElement.querySelectorAll('tbody tr');
    expect(rows.length).toBe(2);
  });

  it('emits revert event when button clicked', () => {
    fixture.detectChanges();
    const data = [
      { id: '1', timestamp: '2025-07-29T00:00:00Z' }
    ];
    httpMock.expectOne('/api/overrides/history/home/block0').flush(data);
    fixture.detectChanges();
    let emitted: string | undefined;
    component.revert.subscribe(v => emitted = v);
    const button = fixture.nativeElement.querySelector('button');
    button.click();
    expect(emitted).toBe('1');
  });
});
