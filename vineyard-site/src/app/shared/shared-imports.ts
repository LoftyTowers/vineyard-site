import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { QuillModule } from 'ngx-quill';
import { ColorPickerDirective, ColorPickerComponent } from 'ngx-color-picker';
import { MatProgressSpinnerModule } from '@angular/material/progress-spinner';
import { MatSnackBarModule } from '@angular/material/snack-bar';

export const SHARED_IMPORTS = [
  CommonModule,
  FormsModule,
  QuillModule,
  ColorPickerDirective,
  ColorPickerComponent,
  MatProgressSpinnerModule,
  MatSnackBarModule
];
