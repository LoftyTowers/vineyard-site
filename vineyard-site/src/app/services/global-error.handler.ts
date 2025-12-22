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

  private normalizeError(error: unknown): { type: string; message: string; stack?: string } {
    const rejection = (error as { rejection?: unknown })?.rejection;
    const baseError = rejection ?? error;

    if (baseError instanceof Error) {
      return {
        type: baseError.name || 'Error',
        message: baseError.message,
        stack: baseError.stack
      };
    }

    if (typeof baseError === 'string') {
      return {
        type: 'string',
        message: baseError
      };
    }

    return {
      type: typeof baseError,
      message: `${baseError}`
    };
  }
}
