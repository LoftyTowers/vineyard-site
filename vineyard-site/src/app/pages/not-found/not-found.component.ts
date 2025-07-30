import { Component } from '@angular/core';
import { SHARED_IMPORTS } from '../../shared/shared-imports';

@Component({
  selector: 'app-not-found',
  standalone: true,
  imports: [...SHARED_IMPORTS],
  templateUrl: './not-found.component.html',
  styleUrl: './not-found.component.scss'
})
export class NotFoundComponent {}
