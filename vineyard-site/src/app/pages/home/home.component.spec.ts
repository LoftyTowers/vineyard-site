import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';

import { HomeComponent } from './home.component';

describe('HomeComponent', () => {
  let component: HomeComponent;
  let fixture: ComponentFixture<HomeComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [HomeComponent, HttpClientTestingModule]
    }).compileComponents();

    fixture = TestBed.createComponent(HomeComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create', () => {
    fixture.detectChanges();
    httpMock.expectOne('/api/pages/').flush({ blocks: [] });
    expect(component).toBeTruthy();
  });

  it('renders merged block content', () => {
    fixture.detectChanges();
    httpMock.expectOne('/api/pages/').flush({ blocks: [{ type: 'p', content: 'override text' }] });
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('override text');
  });
});
