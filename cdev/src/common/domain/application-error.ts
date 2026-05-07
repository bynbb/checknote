export type ApplicationErrorType = 'failure' | 'validation' | 'problem' | 'not-found' | 'conflict' | 'unexpected';

interface ApplicationErrorOptions {
  readonly code: string;
  readonly message: string;
  readonly type: ApplicationErrorType;
  readonly cause?: unknown;
}

export class ApplicationError extends Error {
  readonly code: string;
  readonly type: ApplicationErrorType;
  override readonly cause?: unknown;

  constructor(options: ApplicationErrorOptions) {
    super(options.message);
    this.name = 'ApplicationError';
    this.code = options.code;
    this.type = options.type;
    this.cause = options.cause;
  }
}

export function isApplicationError(error: unknown): error is ApplicationError {
  return error instanceof ApplicationError;
}
