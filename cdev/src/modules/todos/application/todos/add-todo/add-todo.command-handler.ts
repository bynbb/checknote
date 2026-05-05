import { CommandHandler } from '../../../../../common/application/messaging/command-handler';
import { Todo } from '../../../domain/todo';
import { TodoIdGenerator } from '../../abstractions/todo-id-generator';
import { TodoRepository } from '../../abstractions/todo-repository';
import { AddTodoCommand } from './add-todo.command';

export class AddTodoCommandHandler implements CommandHandler<AddTodoCommand> {
  constructor(
    private readonly repository: TodoRepository,
    private readonly ids: TodoIdGenerator,
  ) {}

  async handle(command: AddTodoCommand): Promise<void> {
    const title = command.title.trim();

    if (!title) {
      return;
    }

    const todos = await this.repository.getAll();
    const todo: Todo = {
      id: this.ids.nextId(),
      title,
      completed: false,
    };

    await this.repository.saveAll([todo, ...todos]);
  }
}
