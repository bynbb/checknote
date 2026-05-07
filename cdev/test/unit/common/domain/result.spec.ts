import { ApplicationError, isApplicationError } from '../../../../src/common/domain/application-error';
import { failure, success } from '../../../../src/common/domain/result';

const ok = success('saved');
equal(true, ok.ok, 'success result should be ok');

if (ok.ok) {
  equal('saved', ok.value, 'success result value');
}

const error = new ApplicationError({
  code: 'Todos.EmptyTitle',
  message: 'Todo title cannot be empty.',
  type: 'validation',
});
const failed = failure(error);
equal(false, failed.ok, 'failure result should not be ok');

if (!failed.ok) {
  const failedError = failed.error;
  equal(true, isApplicationError(failedError), 'failure result should carry application error');

  if (!isApplicationError(failedError)) {
    throw new Error('failure result should carry application error');
  }

  equal('Todos.EmptyTitle', failedError.code, 'application error code');
  equal('validation', failedError.type, 'application error type');
}

console.log('Cdev unit tests passed.');

function equal<T>(expected: T, actual: T, description: string): void {
  if (!Object.is(expected, actual)) {
    throw new Error(`${description}: expected ${String(expected)}, got ${String(actual)}`);
  }
}
