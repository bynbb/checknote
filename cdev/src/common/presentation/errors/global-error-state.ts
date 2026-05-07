import { Injectable, signal } from '@angular/core';
import { ErrorReporter } from '@cdev/common/application';
import { isApplicationError } from '@cdev/common/domain';

export interface PresentedError {
  readonly code: string;
  readonly message: string;
}

@Injectable()
export class GlobalErrorState implements ErrorReporter {
  readonly current = signal<PresentedError | null>(null);

  report(error: unknown): void {
    this.current.set(normalizeError(error));
  }

  clear(): void {
    this.current.set(null);
  }
}

function normalizeError(error: unknown): PresentedError {
  const unwrapped = unwrapError(error);

  if (isApplicationError(unwrapped)) {
    return {
      code: unwrapped.code,
      message: unwrapped.message,
    };
  }

  if (unwrapped instanceof Error) {
    return {
      code: 'Client.Unexpected',
      message: unwrapped.message || 'An unexpected client error occurred.',
    };
  }

  return {
    code: 'Client.Unexpected',
    message: 'An unexpected client error occurred.',
  };
}

function unwrapError(error: unknown): unknown {
  if (error && typeof error === 'object' && 'rejection' in error) {
    return (error as { readonly rejection?: unknown }).rejection;
  }

  return error;
}
