import { TestBed } from '@angular/core/testing';
import { Router, ActivatedRouteSnapshot, RouterStateSnapshot, Data } from '@angular/router';
import { RouterTestingModule } from '@angular/router/testing';
import { HttpClientTestingModule } from '@angular/common/http/testing';
import { authGuard } from './auth.guard';
import { AuthService } from './auth.service';

function createToken(role: string | string[]): string {
  const encode = (obj: object) =>
    btoa(JSON.stringify(obj)).replace(/\+/g, '-').replace(/\//g, '_').replace(/=+$/, '');
  const header = encode({ alg: 'none' });
  const payload = encode({ role });
  return `${header}.${payload}.`;
}

describe('authGuard', () => {
  let router: Router;

  function runGuard(data?: Data) {
    const route = new ActivatedRouteSnapshot();
    (route as ActivatedRouteSnapshot & { data: Data }).data = data ?? {};
    return TestBed.runInInjectionContext(
      () => authGuard(route, {} as RouterStateSnapshot)
    );
  }

  beforeEach(() => {
    TestBed.configureTestingModule({
      imports: [RouterTestingModule, HttpClientTestingModule],
      providers: [AuthService]
    });
    router = TestBed.inject(Router);
    spyOn(router, 'navigate').and.returnValue(Promise.resolve(true));
    localStorage.clear();
  });

  afterEach(() => localStorage.clear());

  it('redirects when no token', () => {
    const result = runGuard({ roles: ['Admin'] });
    expect(result).toBeFalse();
    expect(router.navigate).toHaveBeenCalledWith(['/']);
  });

  it('allows when role matches', () => {
    localStorage.setItem('auth_token', createToken('Admin'));
    const result = runGuard({ roles: ['Admin'] });
    expect(result).toBeTrue();
    expect(router.navigate).not.toHaveBeenCalled();
  });

  it('denies when role does not match', () => {
    localStorage.setItem('auth_token', createToken('User'));
    const result = runGuard({ roles: ['Admin'] });
    expect(result).toBeFalse();
  });

  it('allows when any matching role in array', () => {
    localStorage.setItem('auth_token', createToken(['User', 'Admin']));
    const result = runGuard({ roles: ['Admin'] });
    expect(result).toBeTrue();
  });

  it('allows when no roles specified and token exists', () => {
    localStorage.setItem('auth_token', createToken('User'));
    const result = runGuard();
    expect(result).toBeTrue();
  });
});
