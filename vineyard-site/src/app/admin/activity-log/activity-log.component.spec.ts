import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ActivityLogComponent } from './activity-log.component';

describe('ActivityLogComponent', () => {
  let component: ActivityLogComponent;
  let fixture: ComponentFixture<ActivityLogComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ActivityLogComponent, HttpClientTestingModule]
    }).compileComponents();

    fixture = TestBed.createComponent(ActivityLogComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create', () => {
    fixture.detectChanges();
    httpMock.expectOne('/api/audit').flush([]);
    expect(component).toBeTruthy();
  });
});
