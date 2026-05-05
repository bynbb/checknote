import { InjectionToken } from '@angular/core';
import {
  AddTodoCommandHandler,
  ClearCompletedCommandHandler,
  DeleteTodoCommandHandler,
  GetTodosQueryHandler,
  TodoIdGenerator,
  TodoRepository,
  ToggleTodoCommandHandler,
} from '@cdev/modules/todos/application';

export const TODO_REPOSITORY = new InjectionToken<TodoRepository>('TODO_REPOSITORY');
export const TODO_ID_GENERATOR = new InjectionToken<TodoIdGenerator>('TODO_ID_GENERATOR');
export const GET_TODOS_HANDLER = new InjectionToken<GetTodosQueryHandler>('GET_TODOS_HANDLER');
export const ADD_TODO_HANDLER = new InjectionToken<AddTodoCommandHandler>('ADD_TODO_HANDLER');
export const TOGGLE_TODO_HANDLER = new InjectionToken<ToggleTodoCommandHandler>('TOGGLE_TODO_HANDLER');
export const DELETE_TODO_HANDLER = new InjectionToken<DeleteTodoCommandHandler>('DELETE_TODO_HANDLER');
export const CLEAR_COMPLETED_HANDLER = new InjectionToken<ClearCompletedCommandHandler>('CLEAR_COMPLETED_HANDLER');
