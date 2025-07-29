import { ComponentFixture, TestBed } from '@angular/core/testing';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { ThemeEditorComponent } from './theme-editor.component';

describe('ThemeEditorComponent', () => {
  let component: ThemeEditorComponent;
  let fixture: ComponentFixture<ThemeEditorComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ThemeEditorComponent, HttpClientTestingModule]
    }).compileComponents();

    fixture = TestBed.createComponent(ThemeEditorComponent);
    component = fixture.componentInstance;
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should create', () => {
    fixture.detectChanges();
    httpMock.expectOne('/api/branding-overrides').flush({});
    expect(component).toBeTruthy();
  });

  it('should POST updated theme on save', () => {
    fixture.detectChanges();
    httpMock.expectOne('/api/branding-overrides').flush({ primary: '#fff' });

    component.theme['primary'] = '#000';
    component.save();

    const req = httpMock.expectOne('/api/branding-overrides');
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(component.theme);
    req.flush({});
  });
});
