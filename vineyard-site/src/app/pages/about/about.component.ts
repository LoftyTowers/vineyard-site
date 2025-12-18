import { Component, OnDestroy, OnInit } from '@angular/core';
import { SHARED_IMPORTS } from '../../shared/shared-imports';
import { EditableTextBlockComponent, EditableImageBlockComponent } from '../../shared/components';
import { AuthService } from '../../services/auth.service';
import { PageService, PageData } from '../../services/page.service';
import { Subscription } from 'rxjs';

type AboutBlock =
  | { type: 'h1' | 'h2' | 'h3' | 'p' | 'notice'; content: string }
  | {
      type: 'image';
      content: {
        src: string;
        alt: string;
        caption?: string;
      };
    }
  | {
      type: 'people';
      content: {
        imageUrl: string;
        name: string;
        text: string;
      }[];
    };


@Component({
  selector: 'app-about',
  imports: [...SHARED_IMPORTS, EditableTextBlockComponent, EditableImageBlockComponent],
  templateUrl: './about.component.html',
  styleUrl: './about.component.scss'
})
export class AboutComponent implements OnInit, OnDestroy {
  isAdmin = false;
  private authSub?: Subscription;
  constructor(private pageService: PageService, private auth: AuthService) {}

  aboutContentBlocks: AboutBlock[] = [];

  ngOnInit(): void {
    this.authSub = this.auth.authState$.subscribe(() => {
      this.isAdmin = this.auth.hasRole('Admin') || this.auth.hasRole('Editor');
    });
    this.isAdmin = this.auth.hasRole('Admin') || this.auth.hasRole('Editor');
    this.pageService.getPage('about').subscribe((data: PageData) => {
      if (Array.isArray(data.blocks)) {
        this.aboutContentBlocks = data.blocks as AboutBlock[];
        this.normalizeImageBlocks(this.aboutContentBlocks);
      }
    });
  }

  ngOnDestroy(): void {
    this.authSub?.unsubscribe();
  }

  private normalizeImageBlocks(blocks: AboutBlock[]): void {
    for (const block of blocks) {
      if (block.type === 'image') {
        if (!block.content.src && (block.content as any).url) {
          block.content.src = (block.content as any).url;
        }
      }
    }
  }
}
