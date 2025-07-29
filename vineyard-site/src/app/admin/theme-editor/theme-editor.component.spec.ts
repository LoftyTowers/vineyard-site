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
});
