import { Command } from '../../../../../common/application/messaging/command';

export class RegisterUserCommand implements Command {
  readonly type = 'users.register';

  constructor(
    readonly name: string,
    readonly email: string,
  ) {}
}
