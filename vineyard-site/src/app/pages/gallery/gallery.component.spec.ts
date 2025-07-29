import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';

import { GalleryComponent } from './gallery.component';

describe('GalleryComponent', () => {
  let component: GalleryComponent;
  let fixture: ComponentFixture<GalleryComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [GalleryComponent, HttpClientTestingModule]
    }).compileComponents();

    fixture = TestBed.createComponent(GalleryComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create', () => {
    fixture.detectChanges();
    httpMock.expectOne('/api/overrides/gallery').flush({});
    expect(component).toBeTruthy();
  });

  it('renders merged block content', () => {
    fixture.detectChanges();
    httpMock.expectOne('/api/overrides/gallery').flush({ block0: 'new gallery' });
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('new gallery');
  });
});
