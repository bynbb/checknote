import { Command } from '@cdev/common/application';

export class RegisterUserCommand implements Command {
  readonly type = 'users.register';

  constructor(
    readonly name: string,
    readonly email: string,
  ) {}
}
