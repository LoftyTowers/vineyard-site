import { ComponentFixture, TestBed } from '@angular/core/testing';

import { NavbarComponent } from './navbar.component';
import { RouterTestingModule } from '@angular/router/testing';

describe('NavbarComponent', () => {
  let component: NavbarComponent;
  let fixture: ComponentFixture<NavbarComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [RouterTestingModule, NavbarComponent]
    })
    .compileComponents();

    fixture = TestBed.createComponent(NavbarComponent);
    component = fixture.componentInstance;
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
    component.mobileMenuRef = { nativeElement: div } as any;
    const event = new MouseEvent('click', { bubbles: true });
    document.dispatchEvent(event); // ensure listener exists
    component.handleClickOutside(new MouseEvent('click', { target: document.body } as any));
    expect(component.isMenuOpen).toBeFalse();
  });
});
