import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';

import { AboutComponent } from './about.component';

describe('AboutComponent', () => {
  let component: AboutComponent;
  let fixture: ComponentFixture<AboutComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AboutComponent, HttpClientTestingModule]
    }).compileComponents();

    fixture = TestBed.createComponent(AboutComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create', () => {
    fixture.detectChanges();
    httpMock.expectOne('/api/pages/about').flush({ blocks: [] });
    expect(component).toBeTruthy();
  });

  it('renders merged block content', () => {
    fixture.detectChanges();
    httpMock.expectOne('/api/pages/about').flush({ blocks: [{ type: 'h1', content: 'override title' }] });
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('override title');
  });
});
