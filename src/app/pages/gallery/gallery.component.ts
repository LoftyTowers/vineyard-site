import { Component } from '@angular/core';
import { NgFor } from '@angular/common';

@Component({
  selector: 'app-gallery',
  imports: [NgFor],
  templateUrl: './gallery.component.html',
  styleUrl: './gallery.component.scss'
})
export class GalleryComponent {
  galleryContentBlocks = [
    { type: 'h1', content: 'Gallery' },
    { type: 'h2', content: 'A few moments from our journey so far.' },
    { type: 'p', content: 'Here are a few snapshots of the family, the vineyard, and the land we’re growing into.' },
    { type: 'img', content: 'assets/temp-images/ASunriseAtTheVineyard.jpg' },
    { type: 'img', content: 'assets/temp-images/HarvestComplete.jpg' },
    { type: 'img', content: 'assets/temp-images/HelloWoof.jpg' },
    { type: 'img', content: 'assets/temp-images/JustPlanted.jpg' },
    { type: 'img', content: 'assets/temp-images/PickedGrapesInSunset.jpg' },
    { type: 'img', content: 'assets/temp-images/ReadyForHarvest.jpg' },
    { type: 'img', content: 'assets/temp-images/TheMistyVineyard.jpg' },
    { type: 'img', content: 'assets/temp-images/TheMoonlitMist.jpg' }
  ];
}
