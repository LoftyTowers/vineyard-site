import { Component, Input, Output, EventEmitter } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-editable-image-block',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './editable-image-block.component.html',
  styleUrl: './editable-image-block.component.scss'
})
export class EditableImageBlockComponent {
  @Input() src = '';
  @Input() alt = '';
  @Output() srcChange = new EventEmitter<string>();

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
      const file = input.files[0];
      const reader = new FileReader();
      reader.onload = () => {
        this.src = reader.result as string;
        this.srcChange.emit(this.src);
      };
      reader.readAsDataURL(file);
    }
  }
}
