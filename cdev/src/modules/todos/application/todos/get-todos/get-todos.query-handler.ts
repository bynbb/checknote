import { QueryHandler } from '../../../../../common/application/messaging/query-handler';
import { Todo } from '../../../domain/todo';
import { TodoRepository } from '../../abstractions/todo-repository';
import { GetTodosQuery } from './get-todos.query';

export class GetTodosQueryHandler implements QueryHandler<GetTodosQuery, Todo[]> {
  constructor(private readonly repository: TodoRepository) {}

  handle(_query: GetTodosQuery): Promise<Todo[]> {
    return this.repository.getAll();
  }
}
