import { Injectable } from '@angular/core';
import {
  HttpErrorResponse,
  HttpEvent,
  HttpHandler,
  HttpInterceptor,
  HttpRequest,
  HttpResponse
} from '@angular/common/http';
import { Observable, throwError } from 'rxjs';
import { catchError, finalize, tap } from 'rxjs/operators';

@Injectable()
export class ApiLoggingInterceptor implements HttpInterceptor {
  intercept(req: HttpRequest<unknown>, next: HttpHandler): Observable<HttpEvent<unknown>> {
    const correlationId = crypto.randomUUID ? crypto.randomUUID() : `cid-${Math.random().toString(36).slice(2)}`;
    const started = performance.now();
    let status: number | string | undefined;
    let logged = false;

    const request = req.clone({
      setHeaders: {
        'X-Correlation-Id': correlationId
      }
    });

    this.log('info', '[API ->]', {
      correlationId,
      method: request.method,
      url: request.urlWithParams
    });

    return next.handle(request).pipe(
      tap(event => {
        if (event instanceof HttpResponse) {
          status = event.status;
        }
      }),
      catchError((error: HttpErrorResponse) => {
        status = error.status || 'error';
        logged = true;
        this.log('error', '[API <-]', {
          correlationId,
          method: request.method,
          url: request.urlWithParams,
          status,
          durationMs: Math.round(performance.now() - started),
          error: this.describeError(error)
        });
        return throwError(() => error);
      }),
      finalize(() => {
        if (logged) {
          return;
        }
        this.log('info', '[API <-]', {
          correlationId,
          method: request.method,
          url: request.urlWithParams,
          status: status ?? 'completed',
          durationMs: Math.round(performance.now() - started)
        });
      })
    );
  }

  private describeError(error: HttpErrorResponse) {
    const contentType = error.headers?.get('content-type') || '';
    if (contentType.includes('application/problem+json') && error.error) {
      const problem = error.error;
      return {
        type: problem.type,
        title: problem.title,
        status: problem.status,
        detail: problem.detail,
        traceId: problem.traceId || problem.extensions?.traceId
      };
    }

    return {
      message: error.message,
      status: error.status,
      statusText: error.statusText
    };
  }

  private log(level: 'info' | 'error', message: string, data: Record<string, unknown>) {
    const logger = level === 'error' ? console.error : console.info;
    logger(message, data);
  }
}
