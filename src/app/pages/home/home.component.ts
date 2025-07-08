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
    { type: 'h3', content: '⚠️ AI-generated placeholder text final copy coming soon.' },
    { type: 'h2', content: 'Welcome to Hollywood Farm Vineyard' },
    { type: 'p', content: 'Tucked away in the quiet countryside of North Essex, Hollywood Farm Vineyard is a small family project rooted in passion, tradition, and legacy.' },
    { type: 'p', content: 'Our family has farmed this land for over a century. Through five generations, it has been passed down, worked on, and cared for each adding something new to the story. When Charles retired, he decided it was time for a different kind of planting: a vineyard.' },
    { type: 'p', content: 'That one decision brought the whole family together. Aunts, uncles, cousins, and kids all got involved. Today, three generations pitch in with planting, pruning, and picking.' },
    { type: 'p', content: 'We have had one hand-picked harvest so far and while we are just getting started, the roots run deep.' },
  ];
}
