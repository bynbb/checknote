import { ConfirmationDialog, ConfirmationRequest } from '../../../../common/application';

interface ConfirmableTodo {
  readonly id: number;
  readonly title: string;
}

type DeleteTodo = (todoId: number) => Promise<void> | void;

export function createDeleteTodoConfirmationRequest(todo: ConfirmableTodo): ConfirmationRequest {
  return {
    title: 'Delete todo?',
    message: `Delete "${todo.title}"? This cannot be undone.`,
    confirmText: 'Delete',
    cancelText: 'Cancel',
    tone: 'danger',
  };
}

export async function confirmTodoDeletion(
  todo: ConfirmableTodo,
  confirmationDialog: ConfirmationDialog,
): Promise<boolean> {
  return confirmationDialog.confirm(createDeleteTodoConfirmationRequest(todo));
}

export async function deleteTodoAfterConfirmation(
  todo: ConfirmableTodo,
  confirmationDialog: ConfirmationDialog,
  deleteTodo: DeleteTodo,
): Promise<boolean> {
  if (!(await confirmTodoDeletion(todo, confirmationDialog))) {
    return false;
  }

  await deleteTodo(todo.id);
  return true;
}
