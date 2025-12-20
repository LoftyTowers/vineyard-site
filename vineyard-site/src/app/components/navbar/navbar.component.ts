import {
  Component,
  ElementRef,
  ViewChild,
  AfterViewInit,
} from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';
import { AuthService } from '../../services/auth.service';
import { AsyncPipe } from '@angular/common';
import { Observable } from 'rxjs';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink, AsyncPipe],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss',
})
export class NavbarComponent implements AfterViewInit {
  isMenuOpen = false;
  isAuthenticated$!: Observable<string | null>;

  @ViewChild('mobileMenu') mobileMenuRef!: ElementRef;

  constructor(private router: Router, private auth: AuthService) {
    this.isAuthenticated$ = this.auth.authState$;
    this.router.events.subscribe(() => {
      this.isMenuOpen = false;
    });
  }

  ngAfterViewInit(): void {
    document.addEventListener('click', this.handleClickOutside.bind(this));
  }

  toggleMenu(): void {
    this.isMenuOpen = !this.isMenuOpen;
  }

  handleClickOutside(event: MouseEvent): void {
    if (
      this.isMenuOpen &&
      this.mobileMenuRef &&
      !this.mobileMenuRef.nativeElement.contains(event.target)
    ) {
      this.isMenuOpen = false;
    }
  }

  logout(): void {
    this.auth.logout();
    this.router.navigate(['/']);
    this.isMenuOpen = false;
  }
}
