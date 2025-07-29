import { Component, OnInit } from '@angular/core';
import { SHARED_IMPORTS } from '../../shared/shared-imports';
import { EditableTextBlockComponent, EditableImageBlockComponent } from '../../shared/components';
import { HttpClient } from '@angular/common/http';

export type GalleryBlock =
  | { type: 'h1' | 'h2' | 'p'; content: string }
  | {
      type: 'image';
      content: {
        src: string;
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
export class GalleryComponent implements OnInit {
  isAdmin = false;
  constructor(private http: HttpClient) {}
  galleryContentBlocks: GalleryBlock[] = [
    { type: 'h1', content: 'Gallery' },
    { type: 'h2', content: 'A few moments from our journey so far.' },
    {
      type: 'p',
      content: 'Here are a few snapshots of the family, the vineyard, and the land we’re growing into.'
    },
    {
      type: 'image',
      content: {
        src: 'assets/temp-images/ASunriseAtTheVineyard.jpg',
        alt: 'Sunrise at the vineyard',
        caption: 'Morning light over the first row'
      }
    },
    {
      type: 'image',
      content: {
        src: 'assets/temp-images/HarvestComplete.jpg',
        alt: 'Harvest complete',
        caption: 'The last crate picked before sunset'
      }
    },
    {
      type: 'image',
      content: {
        src: 'assets/temp-images/HelloWoof.jpg',
        alt: 'The family dog',
        caption: 'One of the friendliest members of the crew'
      }
    },
    {
      type: 'image',
      content: {
        src: 'assets/temp-images/JustPlanted.jpg',
        alt: 'Just planted',
        caption: 'The first baby vines going into the soil'
      }
    },
    {
      type: 'image',
      content: {
        src: 'assets/temp-images/PickedGrapesInSunset.jpg',
        alt: 'Picked grapes in sunset',
        caption: 'Fresh grapes and golden light — a perfect pairing'
      }
    },
    {
      type: 'image',
      content: {
        src: 'assets/temp-images/ReadyForHarvest.jpg',
        alt: 'Ready for harvest',
        caption: 'The week before we picked our first crop'
      }
    },
    {
      type: 'image',
      content: {
        src: 'assets/temp-images/TheMistyVineyard.jpg',
        alt: 'The misty vineyard',
        caption: 'Early autumn fog across the rows'
      }
    },
    {
      type: 'image',
      content: {
        src: 'assets/temp-images/TheMoonlitMist.jpg',
        alt: 'The moonlit mist',
        caption: 'Taken on a quiet evening just before harvest'
      }
    }
  ];

  ngOnInit(): void {
    this.http.get<Record<string, string>>('/api/overrides/gallery').subscribe((data) => {
      Object.entries(data).forEach(([key, value]) => {
        const index = parseInt(key.replace('block', ''), 10);
        if (!isNaN(index) && this.galleryContentBlocks[index]) {
          (this.galleryContentBlocks[index] as any).content = (this.galleryContentBlocks[index] as any).content || '';
          if (typeof this.galleryContentBlocks[index].content === 'object' && 'src' in this.galleryContentBlocks[index].content) {
            (this.galleryContentBlocks[index].content as any).src = value;
          } else {
            (this.galleryContentBlocks[index] as any).content = value;
          }
        }
      });
    });
  }
}
