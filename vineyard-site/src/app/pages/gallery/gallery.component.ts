import { Component, OnDestroy, OnInit } from '@angular/core';
import { SHARED_IMPORTS } from '../../shared/shared-imports';
import { EditableTextBlockComponent, EditableImageBlockComponent } from '../../shared/components';
import { AuthService } from '../../services/auth.service';
import { PageService, PageData } from '../../services/page.service';
import { Subscription } from 'rxjs';

export type GalleryBlock =
  | { type: 'h1' | 'h2' | 'p'; content: string }
  | {
      type: 'image';
      content: {
        src: string;
        url?: string;
        alt: string;
        caption?: string;
      };
    };

@Component({
  selector: 'app-gallery',
  standalone: true,
  imports: [...SHARED_IMPORTS, EditableTextBlockComponent, EditableImageBlockComponent],
  templateUrl: './gallery.component.html',
  styleUrl: './gallery.component.scss'
})
export class GalleryComponent implements OnInit, OnDestroy {
  isAdmin = false;
  private authSub?: Subscription;
  constructor(private pageService: PageService, private auth: AuthService) {}
  galleryContentBlocks: GalleryBlock[] = [];

  ngOnInit(): void {
    this.authSub = this.auth.authState$.subscribe(() => {
      this.isAdmin = this.auth.hasRole('Admin') || this.auth.hasRole('Editor');
    });
    this.isAdmin = this.auth.hasRole('Admin') || this.auth.hasRole('Editor');
    this.pageService.getPage('gallery').subscribe((data: PageData) => {
      if (Array.isArray(data.blocks)) {
        this.galleryContentBlocks = data.blocks as GalleryBlock[];
        this.normalizeImageBlocks(this.galleryContentBlocks);
      }
    });
  }

  ngOnDestroy(): void {
    this.authSub?.unsubscribe();
  }

  private normalizeImageBlocks(blocks: GalleryBlock[]): void {
    for (const block of blocks) {
      if (block?.type === 'image' && block.content) {
        if (!block.content.src && block.content.url) {
          block.content.src = block.content.url;
        }
      }
    }
  }
}
