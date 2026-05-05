import { makeEnvironmentProviders } from '@angular/core';
import { MockEndpointLogger } from '../../../common/infrastructure/mock-endpoint-logger';
import { TodoIdGenerator } from '../application/abstractions/todo-id-generator';
import { TodoRepository } from '../application/abstractions/todo-repository';
import { AddTodoCommandHandler } from '../application/todos/add-todo/add-todo.command-handler';
import { ClearCompletedCommandHandler } from '../application/todos/clear-completed/clear-completed.command-handler';
import { DeleteTodoCommandHandler } from '../application/todos/delete-todo/delete-todo.command-handler';
import { GetTodosQueryHandler } from '../application/todos/get-todos/get-todos.query-handler';
import { ToggleTodoCommandHandler } from '../application/todos/toggle-todo/toggle-todo.command-handler';
import { DateNowTodoIdGenerator } from '../infrastructure/date-now-todo-id-generator';
import { LocalStorageTodoRepository } from '../infrastructure/local-storage-todo-repository';
import { MockTodosEndpoint } from '../infrastructure/mock-todos-endpoint';
import { TodosPageFacade } from '../presentation/todos-page.facade';
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
      provide: MockTodosEndpoint,
      useFactory: (logger: MockEndpointLogger) => new MockTodosEndpoint(logger),
      deps: [MockEndpointLogger],
    },
    {
      provide: LocalStorageTodoRepository,
      useFactory: (endpoint: MockTodosEndpoint) => new LocalStorageTodoRepository(endpoint),
      deps: [MockTodosEndpoint],
    },
    { provide: DateNowTodoIdGenerator, useFactory: () => new DateNowTodoIdGenerator() },
    TodosPageFacade,
    { provide: TODO_REPOSITORY, useExisting: LocalStorageTodoRepository },
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
