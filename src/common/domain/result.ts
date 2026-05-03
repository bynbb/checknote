export type Result<T, E = Error> =
  | {
      readonly ok: true;
      readonly value: T;
    }
  | {
      readonly ok: false;
      readonly error: E;
    };

export function success<T>(value: T): Result<T> {
  return { ok: true, value };
}

export function failure(error: Error): Result<never> {
  return { ok: false, error };
}
