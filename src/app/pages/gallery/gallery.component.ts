import { Component } from '@angular/core';
import { NgFor } from '@angular/common';

@Component({
  selector: 'app-gallery',
  imports: [NgFor],
  templateUrl: './gallery.component.html',
  styleUrl: './gallery.component.scss'
})
export class GalleryComponent {
  images = [
    'https://via.placeholder.com/400x300?text=Vineyard+1',
    'https://via.placeholder.com/400x300?text=Vineyard+2',
    'https://via.placeholder.com/400x300?text=Vineyard+3',
  ];
}
