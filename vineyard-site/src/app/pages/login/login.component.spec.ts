import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';
import { Router } from '@angular/router';

import { LoginComponent } from './login.component';
import { AuthService } from '../../services/auth.service';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let auth: jasmine.SpyObj<AuthService>;
  let router: Router;

  beforeEach(async () => {
    const authSpy = jasmine.createSpyObj('AuthService', ['login']);
    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        { provide: AuthService, useValue: authSpy },
        { provide: Router, useValue: { navigate: jasmine.createSpy('navigate') } }
      ]
    }).compileComponents();

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    auth = TestBed.inject(AuthService) as jasmine.SpyObj<AuthService>;
    router = TestBed.inject(Router);
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('calls auth service on submit', () => {
    auth.login.and.returnValue(of({ token: 'abc' } as any));
    component.username = 'user';
    component.password = 'pass';
    component.onSubmit();
    expect(auth.login).toHaveBeenCalledWith('user', 'pass');
    expect((router.navigate as jasmine.Spy).calls.any()).toBeTrue();
  });
});
