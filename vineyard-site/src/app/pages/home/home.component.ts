import { Component, OnDestroy, OnInit } from '@angular/core';
import { SHARED_IMPORTS } from '../../shared/shared-imports';
import { EditableTextBlockComponent } from '../../shared/components';
import { AuthService } from '../../services/auth.service';
import { PageService, PageData } from '../../services/page.service';
import { Subscription } from 'rxjs';

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
  imports: [...SHARED_IMPORTS, EditableTextBlockComponent],
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
  constructor(private pageService: PageService, private auth: AuthService) {}

  homeContentBlocks: HomeBlock[] = [];

  ngOnInit(): void {
    this.authSub = this.auth.authState$.subscribe(() => {
      this.isAdmin = this.auth.hasRole('Admin') || this.auth.hasRole('Editor');
    });
    this.isAdmin = this.auth.hasRole('Admin') || this.auth.hasRole('Editor');
    this.pageService.getPage('').subscribe((data: PageData) => {
      console.debug('Home page blocks from API:', data?.blocks);
      if (Array.isArray(data.blocks)) {
        this.homeContentBlocks = data.blocks as HomeBlock[];
      }
      this.syncHomeContent();
    });
    this.syncHomeContent();
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
}
