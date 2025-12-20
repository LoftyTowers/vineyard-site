import { Component, EventEmitter, HostListener, Input, OnChanges, Output } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ImagesService, ImageListItem } from '../../../services/images.service';

@Component({
  selector: 'app-image-picker-dialog',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './image-picker-dialog.component.html',
  styleUrl: './image-picker-dialog.component.scss'
})
export class ImagePickerDialogComponent implements OnChanges {
  @Input() isOpen = false;
  @Output() closed = new EventEmitter<void>();
  @Output() confirmed = new EventEmitter<ImageListItem>();

  images: ImageListItem[] = [];
  selectedId = '';
  isLoading = false;
  errorMessage = '';

  constructor(private imagesService: ImagesService) {}

  ngOnChanges(): void {
    if (this.isOpen) {
      this.loadImages();
    }
  }

  @HostListener('document:keydown.escape')
  handleEscape(): void {
    if (this.isOpen) {
      this.close();
    }
  }

  close(): void {
    this.closed.emit();
  }

  selectImage(imageId: string): void {
    this.selectedId = imageId;
  }

  confirm(): void {
    const selected = this.images.find(image => image.id === this.selectedId);
    if (selected) {
      this.confirmed.emit(selected);
    }
  }

  private loadImages(): void {
    this.isLoading = true;
    this.errorMessage = '';
    this.imagesService.getImages().subscribe({
      next: images => {
        this.images = images ?? [];
        this.isLoading = false;
        if (!this.selectedId && this.images.length > 0) {
          this.selectedId = this.images[0].id;
        }
      },
      error: () => {
        this.errorMessage = 'Failed to load images.';
        this.isLoading = false;
      }
    });
  }
}
