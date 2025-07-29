import { Component, OnInit } from '@angular/core';
import { SHARED_IMPORTS } from '../../shared/shared-imports';
import { EditableTextBlockComponent } from '../../shared/components';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../services/auth.service';

@Component({
  selector: 'app-home',
  imports: [...SHARED_IMPORTS, EditableTextBlockComponent],
  templateUrl: './home.component.html',
  styleUrl: './home.component.scss'
})
export class HomeComponent implements OnInit {
  isAdmin = false;
  constructor(private http: HttpClient, private auth: AuthService) {}

  homeContentBlocks = [
    { type: 'p', content: 'Tucked away in the quiet countryside of North Essex, Hollywood Farm Vineyard is a small family project rooted in passion, tradition, and legacy.' },
    { type: 'p', content: 'Our family has farmed this land for over a century. Through five generations, it has been passed down, worked on, and cared for each adding something new to the story. When Charles retired, he decided it was time for a different kind of planting: a vineyard.' },
    { type: 'p', content: 'That one decision brought the whole family together. Aunts, uncles, cousins, and kids all got involved. Today, three generations pitch in with planting, pruning, and picking.' },
    { type: 'p', content: 'We have had one hand-picked harvest so far and while we are just getting started, the roots run deep.' },
  ];

  ngOnInit(): void {
    this.isAdmin = this.auth.hasRole('Admin') || this.auth.hasRole('Editor');
    this.http.get<Record<string, string>>('/api/overrides/home').subscribe((data) => {
      Object.entries(data).forEach(([key, value]) => {
        const index = parseInt(key.replace('block', ''), 10);
        if (!isNaN(index) && this.homeContentBlocks[index]) {
          this.homeContentBlocks[index].content = value;
        }
      });
    });
  }
}
