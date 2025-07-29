import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { QuillModule } from 'ngx-quill';
import { ColorPickerDirective, ColorPickerComponent } from 'ngx-color-picker';

export const SHARED_IMPORTS = [
  CommonModule,
  FormsModule,
  QuillModule,
  ColorPickerDirective,
  ColorPickerComponent
];
