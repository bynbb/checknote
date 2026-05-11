import { HttpClient } from '@angular/common/http';
import { makeEnvironmentProviders } from '@angular/core';
import {
  AddTodoCommandHandler,
  ClearCompletedCommandHandler,
  DeleteTodoCommandHandler,
  GetTodosQueryHandler,
  TodoIdGenerator,
  TodoRepository,
  ToggleTodoCommandHandler,
} from '@cdev/modules/todos/application';
import {
  DateNowTodoIdGenerator,
} from '@cdev/modules/todos/infrastructure';
import { TodosPageFacade } from '@cdev/modules/todos/presentation';
import { AngularHttpTodoRepository } from './angular-http-todo-repository';
import {
  ADD_TODO_HANDLER,
  CLEAR_COMPLETED_HANDLER,
  DELETE_TODO_HANDLER,
  GET_TODOS_HANDLER,
  TODO_ID_GENERATOR,
  TODO_REPOSITORY,
  TOGGLE_TODO_HANDLER,
} from './todos.tokens';

export function provideTodosModule() {
  return makeEnvironmentProviders([
    {
      provide: AngularHttpTodoRepository,
      useFactory: (http: HttpClient) => new AngularHttpTodoRepository(http),
      deps: [HttpClient],
    },
    { provide: DateNowTodoIdGenerator, useFactory: () => new DateNowTodoIdGenerator() },
    TodosPageFacade,
    { provide: TODO_REPOSITORY, useExisting: AngularHttpTodoRepository },
    { provide: TODO_ID_GENERATOR, useExisting: DateNowTodoIdGenerator },
    {
      provide: GET_TODOS_HANDLER,
      useFactory: (repository: TodoRepository) => new GetTodosQueryHandler(repository),
      deps: [TODO_REPOSITORY],
    },
    {
      provide: ADD_TODO_HANDLER,
      useFactory: (repository: TodoRepository, ids: TodoIdGenerator) => new AddTodoCommandHandler(repository, ids),
      deps: [TODO_REPOSITORY, TODO_ID_GENERATOR],
    },
    {
      provide: TOGGLE_TODO_HANDLER,
      useFactory: (repository: TodoRepository) => new ToggleTodoCommandHandler(repository),
      deps: [TODO_REPOSITORY],
    },
    {
      provide: DELETE_TODO_HANDLER,
      useFactory: (repository: TodoRepository) => new DeleteTodoCommandHandler(repository),
      deps: [TODO_REPOSITORY],
    },
    {
      provide: CLEAR_COMPLETED_HANDLER,
      useFactory: (repository: TodoRepository) => new ClearCompletedCommandHandler(repository),
      deps: [TODO_REPOSITORY],
    },
  ]);
}
