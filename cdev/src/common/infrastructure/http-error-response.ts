import { ApplicationError, ApplicationErrorType } from '@cdev/common/domain';

interface ProblemDetails {
  readonly title?: unknown;
  readonly detail?: unknown;
  readonly errors?: unknown;
}

export function createApplicationErrorFromHttpResponse(
  status: number,
  body: unknown,
  fallbackMessage = 'API request failed',
): ApplicationError {
  const problemDetails = readProblemDetails(body);
  const nestedError = readFirstNestedError(problemDetails?.errors);
  const code = nestedError?.code ?? readText(problemDetails?.title) ?? `Http.${status}`;
  const message = nestedError?.message ?? readText(problemDetails?.detail) ?? `${fallbackMessage}: ${status}`;

  return new ApplicationError({
    code,
    message,
    type: mapStatusToErrorType(status),
    cause: body,
  });
}

function readProblemDetails(value: unknown): ProblemDetails | null {
  return value && typeof value === 'object' ? value as ProblemDetails : null;
}

function readText(value: unknown): string | null {
  return typeof value === 'string' && value.trim() ? value : null;
}

function readFirstNestedError(errors: unknown): { readonly code: string; readonly message: string } | null {
  if (!Array.isArray(errors)) {
    return null;
  }

  for (const error of errors) {
    if (!error || typeof error !== 'object') {
      continue;
    }

    const candidate = error as { readonly code?: unknown; readonly description?: unknown };
    const code = readText(candidate.code);
    const message = readText(candidate.description);

    if (code && message) {
      return { code, message };
    }
  }

  return null;
}

function mapStatusToErrorType(status: number): ApplicationErrorType {
  switch (status) {
    case 400:
      return 'validation';
    case 404:
      return 'not-found';
    case 409:
      return 'conflict';
    default:
      return status >= 500 ? 'unexpected' : 'problem';
  }
}
