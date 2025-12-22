import { ComponentFixture, TestBed } from '@angular/core/testing';
import { BehaviorSubject, of } from 'rxjs';

import { HomeComponent } from './home.component';
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

describe('HomeComponent', () => {
  let component: HomeComponent;
  let fixture: ComponentFixture<HomeComponent>;
  let authStub: AuthServiceStub;
  let pageServiceSpy: jasmine.SpyObj<PageService>;
  let autosaveSpy: jasmine.SpyObj<PageAutosaveService>;
  let imagesServiceSpy: jasmine.SpyObj<ImagesService>;

  beforeEach(async () => {
    authStub = new AuthServiceStub();
    pageServiceSpy = jasmine.createSpyObj<PageService>('PageService', [
      'getPage',
      'getDraft',
      'updateHeroImage',
      'autosaveDraft',
      'publishDraft',
      'discardDraft',
      'getVersions',
      'getVersionContent'
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
    autosaveSpy.saveNow.and.returnValue(of(void 0));

    await TestBed.configureTestingModule({
      imports: [HomeComponent],
      providers: [
        { provide: AuthService, useValue: authStub },
        { provide: PageService, useValue: pageServiceSpy },
        { provide: PageAutosaveService, useValue: autosaveSpy },
        { provide: ImagesService, useValue: imagesServiceSpy }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(HomeComponent);
    component = fixture.componentInstance;
  });

  it('should create', () => {
    fixture.detectChanges();
    expect(component).toBeTruthy();
  });

  it('renders merged block content', () => {
    pageServiceSpy.getPage.and.returnValue(of({ blocks: [{ type: 'p', content: 'override text' }] }));

    fixture.detectChanges();
    expect(fixture.nativeElement.textContent).toContain('override text');
  });

  it('reloads published content when logging out', () => {
    pageServiceSpy.getDraft.and.returnValue(of({ blocks: [{ type: 'p', content: 'draft text' }] }));
    pageServiceSpy.getPage.and.returnValue(of({ blocks: [{ type: 'p', content: 'live text' }] }));

    authStub.setAdmin(true);
    fixture.detectChanges();
    expect(component['homeContentBlocks'].some(b => (b as { content?: string }).content === 'draft text')).toBeTrue();

    authStub.setAdmin(false);
    expect(pageServiceSpy.getPage).toHaveBeenCalled();
    expect(component['homeContentBlocks'].some(b => (b as { content?: string }).content === 'live text')).toBeTrue();
  });

  it('re-enables draft actions after discard and new edits', () => {
    authStub.setAdmin(true);
    component['isAdmin'] = true;
    component['isAutosaveReady'] = true;
    component['hasDraft'] = false;
    component['combinedContent'] = 'initial';

    component.onContentChange('updated');

    expect(component['hasDraft']).toBeTrue();
    expect(autosaveSpy.queueChange).toHaveBeenCalledWith('home', { blocks: jasmine.any(Array) });
  });
});
