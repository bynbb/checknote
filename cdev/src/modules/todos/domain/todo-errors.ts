export class EmptyTodoTitleError extends Error {
  constructor() {
    super('Todo title cannot be empty.');
  }
}
