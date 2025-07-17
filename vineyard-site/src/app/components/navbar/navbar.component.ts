import {
  Component,
  ElementRef,
  ViewChild,
  AfterViewInit,
} from '@angular/core';
import { Router, RouterLink } from '@angular/router';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-navbar',
  standalone: true,
  imports: [CommonModule, RouterLink],
  templateUrl: './navbar.component.html',
  styleUrl: './navbar.component.scss',
})
export class NavbarComponent implements AfterViewInit {
  isMenuOpen = false;

  @ViewChild('mobileMenu') mobileMenuRef!: ElementRef;

  constructor(private router: Router) {
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
}
