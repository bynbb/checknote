import {
  confirmTodoDeletion,
  deleteTodoAfterConfirmation,
} from '../../../../../../src/modules/todos/presentation/todos/delete-todo-confirmation';

const messages: string[] = [];

const confirmed = confirmTodoDeletion({ id: 42, title: 'Ship the slice' }, (message) => {
  messages.push(message);
  return true;
});

equal(true, confirmed, 'delete confirmation should return true when user confirms');
equal('Delete "Ship the slice"?', messages[0], 'delete confirmation message');

const canceled = confirmTodoDeletion({ id: 43, title: 'Keep the slice' }, () => false);

equal(false, canceled, 'delete confirmation should return false when user cancels');

runAsyncTests()
  .then(() => {
    console.log('Cdev delete confirmation tests passed.');
  })
  .catch((error: unknown) => {
    console.error(error);
    throw error;
  });

async function runAsyncTests(): Promise<void> {
  const deletedIds: number[] = [];
  const deleteConfirmed = await deleteTodoAfterConfirmation(
    { id: 42, title: 'Ship the slice' },
    (todoId) => {
      deletedIds.push(todoId);
    },
    () => true,
  );

  equal(true, deleteConfirmed, 'delete flow should report deletion after confirmation');
  equal(42, deletedIds[0], 'delete flow should call delete action with todo id');

  const canceledIds: number[] = [];
  const deleteCanceled = await deleteTodoAfterConfirmation(
    { id: 43, title: 'Keep the slice' },
    (todoId) => {
      canceledIds.push(todoId);
    },
    () => false,
  );

  equal(false, deleteCanceled, 'delete flow should report no deletion after cancellation');
  equal(0, canceledIds.length, 'delete flow should not call delete action after cancellation');
}

function equal<T>(expected: T, actual: T, description: string): void {
  if (!Object.is(expected, actual)) {
    throw new Error(`${description}: expected ${String(expected)}, got ${String(actual)}`);
  }
}
