import { Component } from '@angular/core';
import { SHARED_IMPORTS } from '../../shared/shared-imports';

@Component({
  selector: 'app-about',
  imports: [...SHARED_IMPORTS],
  templateUrl: './about.component.html',
  styleUrl: './about.component.scss'
})
export class AboutComponent {
  aboutContentBlocks = [
    { type: 'h1', content: 'Our Story' },
    { type: 'h3', content: '⚠️ AI-generated placeholder text final copy coming soon.' },
    { type: 'h2', content: 'The People Behind the Vines' },
    {
      type: 'p',
      content:
        'After decades running the farm, Charles planted the first vines the year he retired. He is the reason any of this exists — and he still walks the rows most mornings, making sure everything is looking right.'
    },
    {
      type: 'h3',
      content:
        'More to follow...'
    },
    {
      type: 'p',
      content:
        'Together, we are building something small, meaningful, and completely our own.'
    }
  ];
}
