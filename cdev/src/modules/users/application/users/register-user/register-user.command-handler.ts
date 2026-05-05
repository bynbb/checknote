import { CommandHandler } from '../../../../../common/application/messaging/command-handler';
import { UserRepository } from '../../abstractions/user-repository';
import { RegisterUserCommand } from './register-user.command';

export class RegisterUserCommandHandler implements CommandHandler<RegisterUserCommand> {
  constructor(private readonly repository: UserRepository) {}

  async handle(command: RegisterUserCommand): Promise<void> {
    await this.repository.register(command.name, command.email);
  }
}
