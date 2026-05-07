import {
  confirmTodoDeletion,
  createDeleteTodoConfirmationRequest,
  deleteTodoAfterConfirmation,
} from '../../../../../../src/modules/todos/presentation/todos/delete-todo-confirmation';

const request = createDeleteTodoConfirmationRequest({ id: 42, title: 'Ship the slice' });

equal('Delete todo?', request.title, 'delete confirmation title');
equal('Delete "Ship the slice"? This cannot be undone.', request.message, 'delete confirmation message');
equal('Delete', request.confirmText, 'delete confirmation action label');
equal('Cancel', request.cancelText, 'delete confirmation cancel label');
equal('danger', request.tone, 'delete confirmation tone');

runAsyncTests()
  .then(() => {
    console.log('Cdev delete confirmation tests passed.');
  })
  .catch((error: unknown) => {
    console.error(error);
    throw error;
  });

async function runAsyncTests(): Promise<void> {
  const confirmationRequests: unknown[] = [];
  const confirmed = await confirmTodoDeletion({ id: 42, title: 'Ship the slice' }, {
    confirm: (confirmationRequest) => {
      confirmationRequests.push(confirmationRequest);
      return Promise.resolve(true);
    },
  });

  equal(true, confirmed, 'delete confirmation should return true when user confirms');
  equal(1, confirmationRequests.length, 'delete confirmation should open one confirmation request');

  const canceled = await confirmTodoDeletion({ id: 43, title: 'Keep the slice' }, {
    confirm: () => Promise.resolve(false),
  });

  equal(false, canceled, 'delete confirmation should return false when user cancels');

  const deletedIds: number[] = [];
  const deleteConfirmed = await deleteTodoAfterConfirmation(
    { id: 42, title: 'Ship the slice' },
    { confirm: () => Promise.resolve(true) },
    (todoId) => {
      deletedIds.push(todoId);
    },
  );

  equal(true, deleteConfirmed, 'delete flow should report deletion after confirmation');
  equal(42, deletedIds[0], 'delete flow should call delete action with todo id');

  const canceledIds: number[] = [];
  const deleteCanceled = await deleteTodoAfterConfirmation(
    { id: 43, title: 'Keep the slice' },
    { confirm: () => Promise.resolve(false) },
    (todoId) => {
      canceledIds.push(todoId);
    },
  );

  equal(false, deleteCanceled, 'delete flow should report no deletion after cancellation');
  equal(0, canceledIds.length, 'delete flow should not call delete action after cancellation');
}

function equal<T>(expected: T, actual: T, description: string): void {
  if (!Object.is(expected, actual)) {
    throw new Error(`${description}: expected ${String(expected)}, got ${String(actual)}`);
  }
}
