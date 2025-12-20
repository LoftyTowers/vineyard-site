import { ComponentFixture, TestBed } from '@angular/core/testing';
import { BehaviorSubject, of } from 'rxjs';

import { AboutComponent } from './about.component';
import { AuthService } from '../../services/auth.service';
import { PageService } from '../../services/page.service';
import { PageAutosaveService } from '../../services/page-autosave.service';
import { ImagesService } from '../../services/images.service';

class AuthServiceStub {
  authState$ = new BehaviorSubject<string | null>(null);
  token: string | null = null;

  setAdmin(isAdmin: boolean) {
    this.token = isAdmin ? 'token' : null;
    this.authState$.next(this.token);
  }

  hasRole(role: string): boolean {
    if (!this.token) {
      return false;
    }
    return role === 'Admin' || role === 'Editor';
  }
}

describe('AboutComponent', () => {
  let component: AboutComponent;
  let fixture: ComponentFixture<AboutComponent>;
  let authStub: AuthServiceStub;
  let pageServiceSpy: jasmine.SpyObj<PageService>;
  let autosaveSpy: jasmine.SpyObj<PageAutosaveService>;
  let imagesServiceSpy: jasmine.SpyObj<ImagesService>;

  beforeEach(async () => {
    authStub = new AuthServiceStub();
    pageServiceSpy = jasmine.createSpyObj<PageService>('PageService', [
      'getPage',
      'getDraft',
      'autosaveDraft',
      'getVersions',
      'getVersionContent',
      'rollbackVersion'
    ]);
    autosaveSpy = jasmine.createSpyObj<PageAutosaveService>('PageAutosaveService', [
      'reset',
      'queueChange',
      'saveNow',
      'destroy'
    ], {
      state$: new BehaviorSubject({ status: 'idle' })
    });
    imagesServiceSpy = jasmine.createSpyObj<ImagesService>('ImagesService', ['getImages']);
    imagesServiceSpy.getImages.and.returnValue(of([]));

    pageServiceSpy.getPage.and.returnValue(of({ blocks: [] }));
    pageServiceSpy.getDraft.and.returnValue(of({ blocks: [] }));
    pageServiceSpy.getVersions.and.returnValue(of([]));
    pageServiceSpy.getVersionContent.and.returnValue(of({ contentJson: { blocks: [] }, versionNo: 1 }));

    await TestBed.configureTestingModule({
      imports: [AboutComponent],
      providers: [
        { provide: AuthService, useValue: authStub },
        { provide: PageService, useValue: pageServiceSpy },
        { provide: PageAutosaveService, useValue: autosaveSpy },
        { provide: ImagesService, useValue: imagesServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(AboutComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('renders merged block content', () => {
    pageServiceSpy.getPage.and.returnValue(of({
      blocks: [
        { type: 'image', content: { url: 'https://example.com/hero.jpg', variant: 'hero' } },
        { type: 'h1', content: 'override title' },
        { type: 'richText', contentHtml: '<p>Intro</p>' },
        { type: 'people', content: [] }
      ]
    }));
    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('override title');
  });
});
