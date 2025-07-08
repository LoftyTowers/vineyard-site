import { Component } from '@angular/core';
import { NgFor } from '@angular/common';

@Component({
  selector: 'app-about',
  imports: [NgFor],
  templateUrl: './about.component.html',
  styleUrl: './about.component.scss'
})
export class AboutComponent {
  aboutContentBlocks = [
    { type: 'h1', content: 'Our Story' },
    {
      type: 'p',
      content:
        'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Sed euismod ligula non magna fringilla, sit amet laoreet neque fermentum.'
    },
    {
      type: 'p',
      content:
        'Quisque vitae turpis auctor, posuere libero sed, porttitor magna. Nullam vel odio sit amet tortor bibendum pulvinar.'
    },
    { type: 'p', content: 'Morbi at nisi at urna consectetur feugiat. Aliquam erat volutpat.' },
    { type: 'h2', content: 'Our Values' },
    {
      type: 'p',
      content:
        'Integrity, sustainability, and a passion for crafting exceptional wines guide everything we do.'
    }
  ];
}
