import { Component, OnDestroy, OnInit } from '@angular/core';
import { Observable, Subscription } from 'rxjs';
import { SHARED_IMPORTS } from '../../shared/shared-imports';
import { EditableTextBlockComponent, ImagePickerDialogComponent } from '../../shared/components';
import { AuthService } from '../../services/auth.service';
import { PageService, PageData, PageVersionContent, PageVersionSummary } from '../../services/page.service';
import { PageAutosaveService, AutosaveState } from '../../services/page-autosave.service';
import { ImageListItem } from '../../services/images.service';

type AboutBlock =
  | { type: 'h1'; content: string }
  | { type: 'p'; content: string }
  | { type: 'richText'; contentHtml: string }
  | {
      type: 'image';
      content: {
        imageId?: string;
        src?: string;
        url?: string;
        alt?: string;
        caption?: string;
        variant?: string;
      };
    }
  | {
      type: 'people';
      content: AboutPerson[];
    };

interface AboutPerson {
  id?: string;
  imageUrl: string;
  storageKey?: string;
  name: string;
  text: string;
  sortOrder?: number;
  isActive?: boolean;
}

const isH1Block = (block: AboutBlock): block is Extract<AboutBlock, { type: 'h1' }> =>
  block.type === 'h1';
const isParagraphBlock = (block: AboutBlock): block is Extract<AboutBlock, { type: 'p' }> =>
  block.type === 'p';
const isRichTextBlock = (block: AboutBlock): block is Extract<AboutBlock, { type: 'richText' }> =>
  block.type === 'richText';
const isImageBlock = (block: AboutBlock): block is Extract<AboutBlock, { type: 'image' }> =>
  block.type === 'image';
const isPeopleBlock = (block: AboutBlock): block is Extract<AboutBlock, { type: 'people' }> =>
  block.type === 'people';
@Component({
  selector: 'app-about',
  imports: [...SHARED_IMPORTS, EditableTextBlockComponent, ImagePickerDialogComponent],
  templateUrl: './about.component.html',
  styleUrl: './about.component.scss'
})
export class AboutComponent implements OnInit, OnDestroy {
  isAdmin = false;
  private authSub?: Subscription;
  private previousIsAdmin?: boolean;
  isPreviewing = false;
  isImagePickerOpen = false;
  hasDraft = false;
  isPublishing = false;
  isSavingDraft = false;
  isReverting = false;
  editingPersonIndex: number | null = null;
  private editingPersonSnapshot: AboutPerson | null = null;
  imagePickerTarget: { type: 'hero' } | { type: 'person'; index: number } | null = null;
  autosaveState$!: Observable<AutosaveState>;
  private isAutosaveReady = false;
  versions: PageVersionSummary[] = [];
  selectedVersionId: string | null = null;
  previewVersionNo?: number;
  liveVersionNo?: number;
  heroTitle = '';
  heroImageUrl = '';
  introHtml = '';
  peopleList: AboutPerson[] = [];
  private readonly aboutRoute = 'about';

  constructor(
    private pageService: PageService,
    private auth: AuthService,
    private autosave: PageAutosaveService
  ) {}

  aboutContentBlocks: AboutBlock[] = [];

  ngOnInit(): void {
    this.autosave.reset();
    this.autosaveState$ = this.autosave.state$;
    this.authSub = this.auth.authState$.subscribe(() => {
      this.handleAuthChange();
    });
    this.isAdmin = this.auth.hasRole('Admin') || this.auth.hasRole('Editor');
    this.previousIsAdmin = this.isAdmin;
    if (this.isAdmin) {
      this.loadVersions();
    }
    this.loadContent();
  }

  ngOnDestroy(): void {
    this.authSub?.unsubscribe();
  }

  get isEditMode(): boolean {
    return this.isAdmin && !this.isPreviewing;
  }


  private handleAuthChange(): void {
    const newIsAdmin = this.auth.hasRole('Admin') || this.auth.hasRole('Editor');
    if (this.previousIsAdmin === undefined) {
      this.previousIsAdmin = newIsAdmin;
      this.isAdmin = newIsAdmin;
      return;
    }

    if (newIsAdmin === this.previousIsAdmin) {
      return;
    }

    this.isAdmin = newIsAdmin;
    this.previousIsAdmin = newIsAdmin;

    if (newIsAdmin) {
      this.isAutosaveReady = false;
      this.loadVersions();
      this.loadContent();
      return;
    }

    this.autosave.reset();
    this.hasDraft = false;
    this.isAutosaveReady = false;
    this.isPreviewing = false;
    this.selectedVersionId = null;
    this.previewVersionNo = undefined;
    this.isReverting = false;
    this.loadPublished();
  }

  private loadContent(): void {
    if (this.isEditMode) {
      this.pageService.getDraft(this.aboutRoute).subscribe({
        next: data => {
          if (Array.isArray(data.blocks)) {
            this.aboutContentBlocks = this.ensureFixedStructure(data.blocks as AboutBlock[]);
            this.hasDraft = true;
            this.selectedVersionId = 'draft';
          }
          this.syncAboutContent();
          this.isAutosaveReady = true;
        },
        error: () => {
          this.loadPublished();
        }
      });
    } else {
      this.loadPublished();
    }
  }

  private loadPublished(preserveDraft: boolean = false): void {
    const hadDraft = this.hasDraft;
    this.pageService.getPage(this.aboutRoute).subscribe((data: PageData) => {
      if (Array.isArray(data.blocks)) {
        this.aboutContentBlocks = this.ensureFixedStructure(data.blocks as AboutBlock[]);
      }
      this.hasDraft = preserveDraft ? hadDraft : false;
      this.isPublishing = false;
      this.syncAboutContent();
      this.isAutosaveReady = true;
    });
  }

  private ensureFixedStructure(blocks: AboutBlock[]): AboutBlock[] {
    const imageBlocks = blocks.filter(isImageBlock);
    const heroBlock =
      imageBlocks.find(block => (block.content.variant || '').toLowerCase() === 'hero') ?? imageBlocks[0];
    if (heroBlock && !heroBlock.content.src && heroBlock.content.url) {
      heroBlock.content.src = heroBlock.content.url;
    }

    const titleBlock = blocks.find(isH1Block);
    const richTextBlock = blocks.find(isRichTextBlock);
    const paragraphBlock = blocks.find(isParagraphBlock);
    const peopleBlock = blocks.find(isPeopleBlock);
    const peopleContent = Array.isArray(peopleBlock?.content) ? peopleBlock!.content : [];

    const introHtml = richTextBlock?.contentHtml
      ?? (paragraphBlock?.content ? `<p>${paragraphBlock.content}</p>` : '');

    return [
      heroBlock ?? {
        type: 'image',
        content: { alt: '', caption: '', variant: 'hero' }
      },
      titleBlock ?? { type: 'h1', content: '' },
      { type: 'richText', contentHtml: introHtml },
      { type: 'people', content: peopleContent }
    ];
  }

  private syncAboutContent(): void {
    const titleBlock = this.aboutContentBlocks.find(isH1Block);
    if (titleBlock) {
      this.heroTitle = titleBlock.content;
    }

    const imageBlocks = this.aboutContentBlocks.filter(isImageBlock);
    const heroBlock =
      imageBlocks.find(block => (block.content.variant || '').toLowerCase() === 'hero') ?? imageBlocks[0];
    if (heroBlock) {
      if (!heroBlock.content.src && heroBlock.content.url) {
        heroBlock.content.src = heroBlock.content.url;
      }
      this.heroImageUrl = heroBlock.content.src || this.heroImageUrl;
    }

    const richTextBlock = this.aboutContentBlocks.find(isRichTextBlock);
    this.introHtml = richTextBlock?.contentHtml ?? this.introHtml;

    const peopleBlock = this.aboutContentBlocks.find(isPeopleBlock);
    this.peopleList = Array.isArray(peopleBlock?.content) ? peopleBlock!.content : [];
  }

  onHeadingChange(value: string): void {
    if (!this.isAutosaveReady) {
      return;
    }

    this.heroTitle = value;
    const existing = this.aboutContentBlocks.find(isH1Block);
    if (existing) {
      existing.content = value;
    } else {
      this.aboutContentBlocks.splice(1, 0, { type: 'h1', content: value });
    }

    this.queueAutosave();
  }

  onIntroChange(html: string): void {
    this.introHtml = html;
    const preserved = this.aboutContentBlocks.filter(block => block.type !== 'p' && block.type !== 'richText');
    const peopleBlock = preserved.find(isPeopleBlock);
    const imageBlock = preserved.find(isImageBlock);
    const titleBlock = preserved.find(isH1Block);
    this.aboutContentBlocks = [
      ...(imageBlock ? [imageBlock] : []),
      ...(titleBlock ? [titleBlock] : []),
      { type: 'richText', contentHtml: html },
      ...(peopleBlock ? [peopleBlock] : [])
    ] as AboutBlock[];
    this.queueAutosave();
  }

  openImagePicker(): void {
    if (!this.isEditMode) {
      return;
    }
    this.imagePickerTarget = { type: 'hero' };
    this.isImagePickerOpen = true;
  }

  closeImagePicker(): void {
    this.isImagePickerOpen = false;
    this.imagePickerTarget = null;
  }

  applySelectedImage(image: ImageListItem): void {
    if (!this.imagePickerTarget) {
      return;
    }

    if (this.imagePickerTarget.type === 'person') {
      const person = this.peopleList[this.imagePickerTarget.index];
      if (person) {
        person.imageUrl = image.url;
        this.queueAutosave();
      }
      this.isImagePickerOpen = false;
      this.imagePickerTarget = null;
      return;
    }

    const imageBlocks = this.aboutContentBlocks.filter(isImageBlock);
    const heroBlock =
      imageBlocks.find(block => (block.content.variant || '').toLowerCase() === 'hero') ?? imageBlocks[0];

    if (heroBlock) {
      heroBlock.content.imageId = image.id;
      heroBlock.content.url = image.url;
      heroBlock.content.src = image.url;
    } else {
      this.aboutContentBlocks.unshift({
        type: 'image',
        content: {
          imageId: image.id,
          url: image.url,
          src: image.url,
          variant: 'hero'
        }
      });
    }

    this.heroImageUrl = image.url;
    this.isImagePickerOpen = false;
    this.imagePickerTarget = null;
    this.queueAutosave();
  }

  openPersonImagePicker(index: number): void {
    if (!this.isEditMode) {
      return;
    }
    this.imagePickerTarget = { type: 'person', index };
    this.isImagePickerOpen = true;
  }


  addPerson(): void {
    if (!this.isEditMode) {
      return;
    }
    const newPerson: AboutPerson = {
      id: this.createPersonId(),
      name: 'New person',
      text: 'Add bio',
      imageUrl: '',
      sortOrder: this.peopleList.length + 1,
      isActive: true
    };
    this.peopleList.push(newPerson);
    const peopleBlock = this.aboutContentBlocks.find(isPeopleBlock);
    if (peopleBlock) {
      peopleBlock.content = this.peopleList;
    }
    this.updateSortOrders();
    this.editingPersonIndex = this.peopleList.length - 1;
    this.queueAutosave();
  }

  editPerson(index: number): void {
    if (!this.isEditMode) {
      return;
    }
    const person = this.peopleList[index];
    if (!person) {
      return;
    }
    this.editingPersonIndex = index;
    this.editingPersonSnapshot = { ...person };
  }

  publishEditingPerson(): void {
    this.editingPersonIndex = null;
    this.editingPersonSnapshot = null;
  }

  discardEditingPerson(): void {
    if (this.editingPersonIndex === null || !this.editingPersonSnapshot) {
      this.editingPersonIndex = null;
      this.editingPersonSnapshot = null;
      return;
    }

    this.peopleList[this.editingPersonIndex] = { ...this.editingPersonSnapshot };
    const peopleBlock = this.aboutContentBlocks.find(isPeopleBlock);
    if (peopleBlock) {
      peopleBlock.content = this.peopleList;
    }
    this.queueAutosave();
    this.editingPersonIndex = null;
    this.editingPersonSnapshot = null;
  }

  markPersonRemoved(index: number): void {
    if (!this.isEditMode) {
      return;
    }
    const person = this.peopleList[index];
    if (!person) {
      return;
    }
    person.isActive = false;
    this.queueAutosave();
  }

  restorePerson(index: number): void {
    if (!this.isEditMode) {
      return;
    }
    const person = this.peopleList[index];
    if (!person) {
      return;
    }
    person.isActive = true;
    this.queueAutosave();
  }

  onPersonFieldChange(): void {
    this.queueAutosave();
  }

  movePerson(index: number, direction: 'up' | 'down'): void {
    if (!this.isEditMode) {
      return;
    }
    const newIndex = direction === 'up' ? index - 1 : index + 1;
    if (newIndex < 0 || newIndex >= this.peopleList.length) {
      return;
    }
    const [moved] = this.peopleList.splice(index, 1);
    this.peopleList.splice(newIndex, 0, moved);
    this.updateSortOrders();
    this.editingPersonIndex = newIndex;
    const peopleBlock = this.aboutContentBlocks.find(block => block.type === 'people') as
      | Extract<AboutBlock, { type: 'people' }>
      | undefined;
    if (peopleBlock) {
      peopleBlock.content = this.peopleList;
    }
    this.queueAutosave();
  }

  private updateSortOrders(): void {
    for (let i = 0; i < this.peopleList.length; i++) {
      this.peopleList[i].sortOrder = i + 1;
    }
  }

  private createPersonId(): string {
    if (typeof crypto !== 'undefined' && typeof crypto.randomUUID === 'function') {
      return crypto.randomUUID();
    }

    const bytes = new Uint8Array(16);
    if (typeof crypto !== 'undefined' && typeof crypto.getRandomValues === 'function') {
      crypto.getRandomValues(bytes);
    } else {
      for (let i = 0; i < bytes.length; i++) {
        bytes[i] = Math.floor(Math.random() * 256);
      }
    }

    bytes[6] = (bytes[6] & 0x0f) | 0x40;
    bytes[8] = (bytes[8] & 0x3f) | 0x80;
    const hex = Array.from(bytes, b => b.toString(16).padStart(2, '0')).join('');
    return `${hex.slice(0, 8)}-${hex.slice(8, 12)}-${hex.slice(12, 16)}-${hex.slice(16, 20)}-${hex.slice(20)}`;
  }


  private queueAutosave(): void {
    const token = this.auth.token;
    if (!this.isEditMode || !this.isAutosaveReady || !token) {
      return;
    }

    const payload = this.buildPayload();
    this.hasDraft = true;
    if (!this.isPreviewing) {
      this.selectedVersionId = 'draft';
    }
    this.autosave.queueChange(this.aboutRoute, payload);
  }

  togglePreview(): void {
    if (!this.isAdmin) {
      return;
    }
    this.isPreviewing = !this.isPreviewing;
    if (!this.isPreviewing && !this.isAutosaveReady) {
      this.loadContent();
    }
  }

  onVersionChange(versionId: string): void {
    if (!this.isAdmin) {
      return;
    }

    if (!versionId) {
      this.selectedVersionId = null;
      this.previewVersionNo = undefined;
      this.isPreviewing = false;
      this.autosave.reset();
      this.isAutosaveReady = false;
      this.loadPublished(true);
      return;
    }

    if (versionId === 'draft') {
      this.selectedVersionId = 'draft';
      this.isPreviewing = false;
      this.isAutosaveReady = false;
      this.autosave.reset();
      this.loadContent();
      return;
    }

    this.selectedVersionId = versionId;
    this.autosave.reset();
    this.isPreviewing = true;
    this.isAutosaveReady = false;
    this.isReverting = false;

    this.pageService.getVersionContent(this.aboutRoute, versionId).subscribe({
      next: (data: PageVersionContent) => {
        if (Array.isArray(data.contentJson.blocks)) {
          this.aboutContentBlocks = this.ensureFixedStructure(data.contentJson.blocks as AboutBlock[]);
        }
        this.previewVersionNo = data.versionNo;
        this.syncAboutContent();
      },
      error: () => {
        this.selectedVersionId = null;
        this.previewVersionNo = undefined;
        this.isPreviewing = false;
        this.loadContent();
      }
    });
  }

  publishDraft(): void {
    if (!this.hasDraft) {
      return;
    }
    this.isPublishing = true;
    const payload = this.buildPayload();
    this.autosave.saveNow(this.aboutRoute, payload).subscribe({
      next: () => {
        this.pageService.publishDraft(this.aboutRoute).subscribe({
          next: data => {
            if (Array.isArray(data.blocks)) {
              this.aboutContentBlocks = this.ensureFixedStructure(data.blocks as AboutBlock[]);
              this.hasDraft = false;
              this.isAutosaveReady = true;
              this.syncAboutContent();
              this.autosave.reset();
              this.loadVersions();
              this.selectedVersionId = null;
              this.previewVersionNo = this.liveVersionNo;
            }
            this.isPublishing = false;
          },
          error: () => {
            this.isPublishing = false;
          }
        });
      },
      error: () => {
        this.isPublishing = false;
      }
    });
  }

  discardDraft(): void {
    if (!this.hasDraft) {
      return;
    }
    this.pageService.discardDraft(this.aboutRoute).subscribe({
      next: () => {
        this.hasDraft = false;
        this.autosave.reset();
        this.loadPublished();
      }
    });
  }

  revertToSelectedVersion(): void {
    if (!this.isAdmin || !this.selectedVersionId) {
      return;
    }

    const hadDraft = this.hasDraft;
    this.isReverting = true;
    this.autosave.reset();
    this.pageService.rollbackVersion(this.aboutRoute, this.selectedVersionId).subscribe({
      next: data => {
        if (Array.isArray(data.blocks)) {
          this.aboutContentBlocks = this.ensureFixedStructure(data.blocks as AboutBlock[]);
        }
        this.selectedVersionId = null;
        this.previewVersionNo = undefined;
        this.isPreviewing = false;
        this.isReverting = false;
        this.isAutosaveReady = true;
        this.hasDraft = hadDraft;
        this.syncAboutContent();
        this.loadVersions();
      },
      error: () => {
        this.isReverting = false;
      }
    });
  }

  private loadVersions(): void {
    this.pageService.getVersions(this.aboutRoute).subscribe({
      next: data => {
        this.versions = data;
        this.liveVersionNo = data.length > 0 ? data[0].versionNo : undefined;
      }
    });
  }

  private buildPayload(): PageData {
    return { blocks: this.aboutContentBlocks };
  }
}
