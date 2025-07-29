import { Component, OnInit } from '@angular/core';
import { SHARED_IMPORTS } from '../../shared/shared-imports';
import { EditableTextBlockComponent, EditableImageBlockComponent } from '../../shared/components';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../services/auth.service';

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
export class AboutComponent implements OnInit {
  isAdmin = false;
  constructor(private http: HttpClient, private auth: AuthService) {}

  aboutContentBlocks: AboutBlock[] = [
    { type: 'h1', content: 'Our Story' },
    {
      type: 'image',
      content: {
        src: 'assets/temp-images/ReadyForHarvest.jpg',
        alt: 'Ready for Harvest',
        caption: 'Our first harvest, picked by hand in 2024'
      }
    },
    { type: 'h2', content: 'The People Behind the Vines' },
    {
      type: 'people',
      content: [
        {
          imageUrl: 'assets/temp-images/ReadyForHarvest.jpg',
          name: 'Charles',
          text: 'After decades running the farm, Charles planted the first vines the year he retired. He’s the reason any of this exists – and he still walks the rows most mornings, making sure everything’s looking right.'
        },
        // Add more people here
      ]
    },
    {
      type: 'p',
      content: 'Together, we are building something small, meaningful, and completely our own.'
    }
  ];

  ngOnInit(): void {
    this.isAdmin = this.auth.hasRole('Admin') || this.auth.hasRole('Editor');
    this.http.get<Record<string, string>>('/api/overrides/about').subscribe((data) => {
      Object.entries(data).forEach(([key, value]) => {
        const index = parseInt(key.replace('block', ''), 10);
        if (!isNaN(index) && (this.aboutContentBlocks as any)[index]) {
          (this.aboutContentBlocks as any)[index].content = value;
        }
      });
    });
  }
}

