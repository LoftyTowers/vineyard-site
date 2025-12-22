import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NavbarComponent } from './navbar.component';
import { ElementRef } from '@angular/core';
import { Router } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { BehaviorSubject, Observable } from 'rxjs';
import { AuthService } from '../../services/auth.service';

describe('NavbarComponent', () => {
  let component: NavbarComponent;
  let fixture: ComponentFixture<NavbarComponent>;
  let authState$: BehaviorSubject<string | null>;
  let authServiceStub: { authState$: Observable<string | null>; logout: jasmine.Spy };
  let router: Router;

  beforeEach(async () => {
    authState$ = new BehaviorSubject<string | null>(null);
    authServiceStub = {
      authState$: authState$.asObservable(),
      logout: jasmine.createSpy('logout')
    };

    await TestBed.configureTestingModule({
      imports: [RouterTestingModule, NavbarComponent],
      providers: [
        { provide: AuthService, useValue: authServiceStub }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(NavbarComponent);
    component = fixture.componentInstance;
    router = TestBed.inject(Router);
    spyOn(router, 'navigateByUrl').and.callThrough();
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('toggleMenu should switch isMenuOpen', () => {
    expect(component.isMenuOpen).toBeFalse();
    component.toggleMenu();
    expect(component.isMenuOpen).toBeTrue();
    component.toggleMenu();
    expect(component.isMenuOpen).toBeFalse();
  });

  it('handleClickOutside closes menu when clicking outside', () => {
    component.isMenuOpen = true;
    const div = document.createElement('div');
    component.mobileMenuRef = new ElementRef<HTMLElement>(div);
    const event = new MouseEvent('click', { bubbles: true });
    document.dispatchEvent(event); // ensure listener exists
    const target = document.body as EventTarget;
    const clickEvent = new MouseEvent('click');
    Object.defineProperty(clickEvent, 'target', { value: target });
    component.handleClickOutside(clickEvent);
    expect(component.isMenuOpen).toBeFalse();
  });

  it('shows Logout when authenticated and triggers logout on click', () => {
    authState$.next('token');
    fixture.detectChanges();
    const buttons: HTMLButtonElement[] = Array.from(fixture.nativeElement.querySelectorAll('button'));
    const logoutButton = buttons.find(b => (b.textContent || '').includes('Logout'));
    expect(logoutButton).withContext('logout button not found').toBeTruthy();
    logoutButton!.click();
    expect(authServiceStub.logout).toHaveBeenCalled();
    expect(router.navigateByUrl).toHaveBeenCalledWith('/');
  });
});
