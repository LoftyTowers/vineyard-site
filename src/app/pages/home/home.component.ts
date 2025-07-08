import { Component } from '@angular/core';
import { NgFor } from '@angular/common';

@Component({
  selector: 'app-home',
  imports: [NgFor],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent {
  homeContentBlocks = [
    { type: 'p', content: 'Welcome to our vineyard. Lorem ipsum dolor sit amet...' },
    { type: 'p', content: 'We are a small family-owned vineyard based in...' },
    { type: 'h2', content: 'Our Story' },
    { type: 'p', content: 'It all started with a love for wine and the land...' },
    // Add more as needed
  ];
}
