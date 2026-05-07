import { computed, inject, Injectable, signal } from '@angular/core';
import { CONFIRMATION_DIALOG } from '@cdev/common/composition/confirmations/confirmation-dialog.token';
import { ERROR_REPORTER } from '@cdev/common/composition/errors/error-reporter.token';
import {
  AddTodoCommand,
  ClearCompletedCommand,
  DeleteTodoCommand,
  GetTodosQuery,
  ToggleTodoCommand,
} from '@cdev/modules/todos/application';
import { Todo, TodoFilter } from '@cdev/modules/todos/domain';
import {
  ADD_TODO_HANDLER,
  CLEAR_COMPLETED_HANDLER,
  DELETE_TODO_HANDLER,
  GET_TODOS_HANDLER,
  TOGGLE_TODO_HANDLER,
} from '@cdev/modules/todos/composition/todos/todos.tokens';
import { deleteTodoAfterConfirmation } from './delete-todo-confirmation';

@Injectable()
export class TodosPageFacade {
  private readonly getTodosHandler = inject(GET_TODOS_HANDLER);
  private readonly addTodoHandler = inject(ADD_TODO_HANDLER);
  private readonly toggleTodoHandler = inject(TOGGLE_TODO_HANDLER);
  private readonly deleteTodoHandler = inject(DELETE_TODO_HANDLER);
  private readonly clearCompletedHandler = inject(CLEAR_COMPLETED_HANDLER);
  private readonly confirmationDialog = inject(CONFIRMATION_DIALOG);
  private readonly errors = inject(ERROR_REPORTER);

  readonly newTodo = signal('');
  readonly filter = signal<TodoFilter>('all');
  readonly todos = signal<Todo[]>([]);
  readonly activeCount = computed(() => this.todos().filter((todo) => !todo.completed).length);
  readonly completedCount = computed(() => this.todos().length - this.activeCount());
  readonly visibleTodos = computed(() => {
    const todos = this.todos();

    switch (this.filter()) {
      case 'active':
        return todos.filter((todo) => !todo.completed);
      case 'completed':
        return todos.filter((todo) => todo.completed);
      default:
        return todos;
    }
  });

  constructor() {
    void this.loadTodos();
  }

  async loadTodos(): Promise<void> {
    try {
      this.todos.set(await this.getTodosHandler.handle(new GetTodosQuery()));
    } catch (error) {
      this.errors.report(error);
    }
  }

  async addTodo(): Promise<void> {
    this.errors.clear();

    try {
      await this.addTodoHandler.handle(new AddTodoCommand(this.newTodo()));
      this.newTodo.set('');
      await this.loadTodos();
    } catch (error) {
      this.errors.report(error);
    }
  }

  async toggleTodo(todoId: number): Promise<void> {
    this.errors.clear();

    try {
      await this.toggleTodoHandler.handle(new ToggleTodoCommand(todoId));
      await this.loadTodos();
    } catch (error) {
      this.errors.report(error);
    }
  }

  async deleteTodo(todo: Todo): Promise<void> {
    this.errors.clear();

    try {
      const deleted = await deleteTodoAfterConfirmation(
        todo,
        this.confirmationDialog,
        (todoId) => this.deleteTodoHandler.handle(new DeleteTodoCommand(todoId)),
      );

      if (deleted) {
        await this.loadTodos();
      }
    } catch (error) {
      this.errors.report(error);
    }
  }

  async clearCompleted(): Promise<void> {
    this.errors.clear();

    try {
      await this.clearCompletedHandler.handle(new ClearCompletedCommand());
      await this.loadTodos();
    } catch (error) {
      this.errors.report(error);
    }
  }

  setFilter(filter: TodoFilter): void {
    this.filter.set(filter);
  }
}
