import { Component, Input, Output, EventEmitter } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { QuillModule } from 'ngx-quill';

@Component({
  selector: 'app-editable-text-block',
  standalone: true,
  imports: [FormsModule, QuillModule],
  templateUrl: './editable-text-block.component.html',
  styleUrl: './editable-text-block.component.scss'
})
export class EditableTextBlockComponent {
  @Input() content = '';
  @Output() contentChange = new EventEmitter<string>();
}
