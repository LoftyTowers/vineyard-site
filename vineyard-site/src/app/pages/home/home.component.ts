import { Component, OnDestroy, OnInit } from '@angular/core';
import { SHARED_IMPORTS } from '../../shared/shared-imports';
import { EditableTextBlockComponent } from '../../shared/components';
import { AuthService } from '../../services/auth.service';
import { PageService, PageData } from '../../services/page.service';
import { Subscription } from 'rxjs';

type HomeBlock =
  | { type: 'h1' | 'h2' | 'p'; content: string }
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
    const titleBlock = this.homeContentBlocks.find(block => block.type === 'h1');
    if (titleBlock && typeof titleBlock.content === 'string') {
      this.heroTitle = titleBlock.content;
    }

    const subtitleBlock = this.homeContentBlocks.find(block => block.type === 'h2');
    if (subtitleBlock && typeof subtitleBlock.content === 'string') {
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

    this.combinedContent = this.homeContentBlocks
      .filter(block => block.type === 'p')
      .map(block => `<p>${(block as { type: 'p'; content: string }).content}</p>`)
      .join('');
    console.debug('Home hero resolved:', {
      heroTitle: this.heroTitle,
      heroSubtitle: this.heroSubtitle,
      heroImageUrl: this.heroImageUrl
    });
  }
}
