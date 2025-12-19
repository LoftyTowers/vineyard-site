import { Component, OnDestroy, OnInit } from '@angular/core';
import { SHARED_IMPORTS } from '../../shared/shared-imports';
import { EditableTextBlockComponent, ImagePickerDialogComponent } from '../../shared/components';
import { AuthService } from '../../services/auth.service';
import { PageService, PageData } from '../../services/page.service';
import { Observable, Subscription } from 'rxjs';
import { PageAutosaveService, AutosaveState } from '../../services/page-autosave.service';
import { ImageListItem } from '../../services/images.service';

type HomeBlock =
  | { type: 'h1' | 'h2' | 'p'; content: string }
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
    };

@Component({
  selector: 'app-home',
  imports: [...SHARED_IMPORTS, EditableTextBlockComponent, ImagePickerDialogComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent implements OnInit, OnDestroy {
  isAdmin = false;
  private authSub?: Subscription;
  combinedContent = '';
  heroTitle = '';
  heroImageUrl = '';
  heroSubtitle = '';
  isImagePickerOpen = false;
  hasDraft = false;
  isPublishing = false;
  isSavingDraft = false;
  private readonly homeRoute = 'home';
  autosaveState$!: Observable<AutosaveState>;
  private isAutosaveReady = false;
  constructor(
    private pageService: PageService,
    private auth: AuthService,
    private autosave: PageAutosaveService
  ) {}

  homeContentBlocks: HomeBlock[] = [];

  ngOnInit(): void {
    this.autosave.reset();
    this.autosaveState$ = this.autosave.state$;
    this.authSub = this.auth.authState$.subscribe(() => {
      this.isAdmin = this.auth.hasRole('Admin') || this.auth.hasRole('Editor');
    });
    this.isAdmin = this.auth.hasRole('Admin') || this.auth.hasRole('Editor');
    this.loadContent();
  }

  ngOnDestroy(): void {
    this.authSub?.unsubscribe();
  }

  private syncHomeContent(): void {
    const isH1Block = (block: HomeBlock): block is { type: 'h1'; content: string } =>
      block.type === 'h1';
    const isH2Block = (block: HomeBlock): block is { type: 'h2'; content: string } =>
      block.type === 'h2';

    const titleBlock = this.homeContentBlocks.find(isH1Block);
    if (titleBlock) {
      this.heroTitle = titleBlock.content;
    }

    const subtitleBlock = this.homeContentBlocks.find(isH2Block);
    if (subtitleBlock) {
      this.heroSubtitle = subtitleBlock.content;
    }

    const imageBlocks = this.homeContentBlocks.filter(block => block.type === 'image') as Array<
      Extract<HomeBlock, { type: 'image' }>
    >;
    const heroBlock =
      imageBlocks.find(block => (block.content.variant || '').toLowerCase() === 'hero') ?? imageBlocks[0];
    if (heroBlock) {
      if (!heroBlock.content.src && heroBlock.content.url) {
        heroBlock.content.src = heroBlock.content.url;
      }
      this.heroImageUrl = heroBlock.content.src || this.heroImageUrl;
    }

    const richTextBlock = this.homeContentBlocks.find(block => block.type === 'richText') as
      | Extract<HomeBlock, { type: 'richText' }>
      | undefined;
    if (richTextBlock?.contentHtml) {
      this.combinedContent = richTextBlock.contentHtml;
    } else {
      this.combinedContent = this.homeContentBlocks
        .filter(block => block.type === 'p')
        .map(block => `<p>${this.escapeHtml((block as { type: 'p'; content: string }).content)}</p>`)
        .join('\n');
    }
    console.debug('Home hero resolved:', {
      heroTitle: this.heroTitle,
      heroSubtitle: this.heroSubtitle,
      heroImageUrl: this.heroImageUrl
    });
  }

  onContentChange(html: string): void {
    this.combinedContent = html;
    this.applyRichTextToBlocks(html);
    this.queueAutosave();
  }

  openImagePicker(): void {
    this.isImagePickerOpen = true;
  }

  closeImagePicker(): void {
    this.isImagePickerOpen = false;
  }

  applyHeroImage(image: ImageListItem): void {
    const imageBlocks = this.homeContentBlocks.filter(block => block.type === 'image') as Array<
      Extract<HomeBlock, { type: 'image' }>
    >;
    const heroBlock =
      imageBlocks.find(block => (block.content.variant || '').toLowerCase() === 'hero') ?? imageBlocks[0];

    if (heroBlock) {
      heroBlock.content.imageId = image.id;
      heroBlock.content.url = image.url;
      heroBlock.content.src = image.url;
    } else {
      this.homeContentBlocks.unshift({
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
    this.queueAutosave();
  }

  onHeadingChange(type: 'h1' | 'h2', value: string): void {
    if (!this.isAutosaveReady) {
      return;
    }

    if (type === 'h1') {
      this.heroTitle = value;
    } else {
      this.heroSubtitle = value;
    }

    const existing = this.homeContentBlocks.find(block => block.type === type) as
      | { type: 'h1' | 'h2'; content: string }
      | undefined;
    if (existing) {
      existing.content = value;
    } else {
      this.homeContentBlocks.unshift({ type, content: value });
    }

    this.queueAutosave();
  }

  private applyRichTextToBlocks(html: string): void {
    const preserved = this.homeContentBlocks.filter(
      block => block.type !== 'p' && block.type !== 'richText'
    );
    this.homeContentBlocks = [
      ...preserved,
      {
        type: 'richText',
        contentHtml: html
      }
    ];
  }

  private escapeHtml(value: string): string {
    return value
      .replace(/&/g, '&amp;')
      .replace(/</g, '&lt;')
      .replace(/>/g, '&gt;')
      .replace(/\"/g, '&quot;')
      .replace(/'/g, '&#39;');
  }

  private queueAutosave(): void {
    const token = this.auth.token;
    if (!this.isAdmin || !this.isAutosaveReady || !token) {
      return;
    }

    const payload = this.buildPayload();
    this.autosave.queueChange(this.homeRoute, payload);
  }

  publishDraft(): void {
    if (!this.hasDraft) {
      return;
    }
    this.isPublishing = true;
    const payload = this.buildPayload();
    this.autosave.saveNow(this.homeRoute, payload).subscribe({
      next: () => {
        this.pageService.publishDraft(this.homeRoute).subscribe({
          next: data => {
            if (Array.isArray(data.blocks)) {
              this.homeContentBlocks = data.blocks as HomeBlock[];
              this.hasDraft = false;
              this.isAutosaveReady = true;
              this.syncHomeContent();
              this.autosave.reset();
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

  saveDraft(): void {
    if (!this.isAdmin || !this.isAutosaveReady) {
      return;
    }
    this.isSavingDraft = true;
    const payload = this.buildPayload();
    this.autosave.saveNow(this.homeRoute, payload).subscribe({
      next: () => {
        this.hasDraft = true;
        this.isSavingDraft = false;
      },
      error: () => {
        this.isSavingDraft = false;
      }
    });
  }

  private loadContent(): void {
    if (this.isAdmin) {
      this.pageService.getDraft(this.homeRoute).subscribe({
        next: data => {
          if (Array.isArray(data.blocks)) {
            this.homeContentBlocks = data.blocks as HomeBlock[];
            this.hasDraft = true;
          }
          this.syncHomeContent();
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

  private loadPublished(): void {
    this.pageService.getPage('').subscribe((data: PageData) => {
      if (Array.isArray(data.blocks)) {
        this.homeContentBlocks = data.blocks as HomeBlock[];
      }
      this.hasDraft = false;
      this.isPublishing = false;
      this.syncHomeContent();
      this.isAutosaveReady = true;
    });
  }

  discardDraft(): void {
    if (!this.hasDraft) {
      return;
    }
    this.pageService.discardDraft(this.homeRoute).subscribe({
      next: () => {
        this.hasDraft = false;
        this.autosave.reset();
        this.loadPublished();
      }
    });
  }

  private buildPayload(): PageData {
    return { blocks: this.homeContentBlocks };
  }
}
