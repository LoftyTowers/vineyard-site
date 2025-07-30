import { HTTP_INTERCEPTORS, HttpClient, HttpErrorResponse } from '@angular/common/http';
import { HttpClientTestingModule, HttpTestingController } from '@angular/common/http/testing';
import { TestBed } from '@angular/core/testing';
import { Router } from '@angular/router';
import { ErrorInterceptor } from './error.interceptor';

describe('ErrorInterceptor', () => {
  let http: HttpClient;
  let httpMock: HttpTestingController;
  let router: jasmine.SpyObj<Router>;

  beforeEach(() => {
    router = jasmine.createSpyObj('Router', ['navigate']);
    TestBed.configureTestingModule({
      imports: [HttpClientTestingModule],
      providers: [
        { provide: Router, useValue: router },
        { provide: HTTP_INTERCEPTORS, useClass: ErrorInterceptor, multi: true }
      ]
    });
    http = TestBed.inject(HttpClient);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('navigates to /error and rethrows the error', () => {
    let receivedError: HttpErrorResponse | undefined;

    http.get('/data').subscribe({
      next: () => fail('should error'),
      error: err => (receivedError = err)
    });

    const req = httpMock.expectOne('/data');
    req.flush('boom', { status: 500, statusText: 'Server Error' });

    expect(router.navigate).toHaveBeenCalledWith(['/error'], { queryParams: { message: 'Server Error' } });
    expect(receivedError).toBeTruthy();
    expect(receivedError instanceof HttpErrorResponse).toBeTrue();
    expect(receivedError?.status).toBe(500);
  });
});
