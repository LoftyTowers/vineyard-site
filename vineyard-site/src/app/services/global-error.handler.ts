import { ErrorHandler, Injectable } from '@angular/core';
import { Router } from '@angular/router';
import { environment } from '../../environments/environment';

@Injectable()
export class GlobalErrorHandler implements ErrorHandler {
  constructor(private router: Router) {}

  handleError(error: unknown): void {
    const route = this.router.url;
    const isProd = environment.production;
    const normalized = this.normalizeError(error);

    console.error('[Client Error]', {
      route,
      type: normalized.type,
      message: normalized.message
    });

    if (!isProd && normalized.stack) {
      console.error(normalized.stack);
    }
  }

  private normalizeError(error: unknown) {
    const rejection: any = (error as any)?.rejection;
    const baseError: any = rejection ?? error;
    return {
      type: baseError?.name ?? typeof baseError,
      message: baseError?.message ?? baseError?.toString?.() ?? 'Unknown error',
      stack: baseError?.stack
    };
  }
}
