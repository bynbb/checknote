import { InjectionToken } from '@angular/core';
import { TodoIdGenerator } from '../application/abstractions/todo-id-generator';
import { TodoRepository } from '../application/abstractions/todo-repository';
import { AddTodoCommandHandler } from '../application/todos/add-todo/add-todo.command-handler';
import { ClearCompletedCommandHandler } from '../application/todos/clear-completed/clear-completed.command-handler';
import { DeleteTodoCommandHandler } from '../application/todos/delete-todo/delete-todo.command-handler';
import { GetTodosQueryHandler } from '../application/todos/get-todos/get-todos.query-handler';
import { ToggleTodoCommandHandler } from '../application/todos/toggle-todo/toggle-todo.command-handler';

export const TODO_REPOSITORY = new InjectionToken<TodoRepository>('TODO_REPOSITORY');
export const TODO_ID_GENERATOR = new InjectionToken<TodoIdGenerator>('TODO_ID_GENERATOR');
export const GET_TODOS_HANDLER = new InjectionToken<GetTodosQueryHandler>('GET_TODOS_HANDLER');
export const ADD_TODO_HANDLER = new InjectionToken<AddTodoCommandHandler>('ADD_TODO_HANDLER');
export const TOGGLE_TODO_HANDLER = new InjectionToken<ToggleTodoCommandHandler>('TOGGLE_TODO_HANDLER');
export const DELETE_TODO_HANDLER = new InjectionToken<DeleteTodoCommandHandler>('DELETE_TODO_HANDLER');
export const CLEAR_COMPLETED_HANDLER = new InjectionToken<ClearCompletedCommandHandler>('CLEAR_COMPLETED_HANDLER');
