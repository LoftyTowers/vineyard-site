import { Component } from '@angular/core';
import { SHARED_IMPORTS } from '../../shared/shared-imports';
import { ActivatedRoute } from '@angular/router';

@Component({
  selector: 'app-server-error',
  standalone: true,
  imports: [...SHARED_IMPORTS],
  templateUrl: './server-error.component.html',
  styleUrl: './server-error.component.scss'
})
export class ServerErrorComponent {
  message = 'An unexpected error occurred';
  constructor(private route: ActivatedRoute) {
    const msg = this.route.snapshot.queryParamMap.get('message');
    if (msg) this.message = msg;
  }
}
