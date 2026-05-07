import { ApplicationError, ApplicationErrorType } from '@cdev/common/domain';

interface ProblemDetails {
  readonly title?: unknown;
  readonly detail?: unknown;
  readonly status?: unknown;
  readonly errors?: unknown;
}

export async function ensureOk(response: Response, fallbackMessage: string): Promise<void> {
  if (response.ok) {
    return;
  }

  throw await createApplicationError(response, fallbackMessage);
}

async function createApplicationError(response: Response, fallbackMessage: string): Promise<ApplicationError> {
  const problemDetails = await readProblemDetails(response);
  const nestedError = readFirstNestedError(problemDetails?.errors);
  const code = nestedError?.code ?? readText(problemDetails?.title) ?? `Http.${response.status}`;
  const message = nestedError?.message ?? readText(problemDetails?.detail) ?? `${fallbackMessage}: ${response.status}`;

  return new ApplicationError({
    code,
    message,
    type: mapStatusToErrorType(response.status),
    cause: problemDetails,
  });
}

async function readProblemDetails(response: Response): Promise<ProblemDetails | null> {
  const contentType = response.headers.get('content-type') ?? '';

  if (!contentType.includes('application/problem+json') && !contentType.includes('application/json')) {
    return null;
  }

  try {
    const body = (await response.json()) as unknown;
    return body && typeof body === 'object' ? (body as ProblemDetails) : null;
  } catch {
    return null;
  }
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
