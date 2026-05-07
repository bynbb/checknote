interface ConfirmableTodo {
  readonly id: number;
  readonly title: string;
}

type ConfirmDialog = (message: string) => boolean;
type DeleteTodo = (todoId: number) => Promise<void> | void;

export function confirmTodoDeletion(
  todo: ConfirmableTodo,
  confirm: ConfirmDialog = (message) => window.confirm(message),
): boolean {
  return confirm(`Delete "${todo.title}"?`);
}

export async function deleteTodoAfterConfirmation(
  todo: ConfirmableTodo,
  deleteTodo: DeleteTodo,
  confirm?: ConfirmDialog,
): Promise<boolean> {
  if (!confirmTodoDeletion(todo, confirm)) {
    return false;
  }

  await deleteTodo(todo.id);
  return true;
}
