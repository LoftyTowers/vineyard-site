import { Component, OnDestroy, OnInit } from '@angular/core';
import { SHARED_IMPORTS } from '../../shared/shared-imports';
import { EditableTextBlockComponent } from '../../shared/components';
import { AuthService } from '../../services/auth.service';
import { PageService, PageData } from '../../services/page.service';
import { Subscription } from 'rxjs';

@Component({
  selector: 'app-home',
  imports: [...SHARED_IMPORTS, EditableTextBlockComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent implements OnInit, OnDestroy {
  isAdmin = false;
  private authSub?: Subscription;
  constructor(private pageService: PageService, private auth: AuthService) {}

  homeContentBlocks = [
    { type: 'p', content: 'Tucked away in the quiet countryside of North Essex, Hollywood Farm Vineyard is a small family project rooted in passion, tradition, and legacy.' },
    { type: 'p', content: 'Our family has farmed this land for over a century. Through five generations, it has been passed down, worked on, and cared for each adding something new to the story. When Charles retired, he decided it was time for a different kind of planting: a vineyard.' },
    { type: 'p', content: 'That one decision brought the whole family together. Aunts, uncles, cousins, and kids all got involved. Today, three generations pitch in with planting, pruning, and picking.' },
    { type: 'p', content: 'We have had one hand-picked harvest so far and while we are just getting started, the roots run deep.' },
  ];

  ngOnInit(): void {
    this.authSub = this.auth.authState$.subscribe(() => {
      this.isAdmin = this.auth.hasRole('Admin') || this.auth.hasRole('Editor');
    });
    this.isAdmin = this.auth.hasRole('Admin') || this.auth.hasRole('Editor');
    this.pageService.getPage('').subscribe((data: PageData) => {
      if (Array.isArray(data.blocks)) {
        this.homeContentBlocks = data.blocks as any[];
      }
    });
  }

  ngOnDestroy(): void {
    this.authSub?.unsubscribe();
  }
}
