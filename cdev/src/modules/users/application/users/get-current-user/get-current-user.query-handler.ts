import { QueryHandler } from '@cdev/common/application';
import { User } from '@cdev/modules/users/domain';
import { UserRepository } from '../../abstractions/user-repository';
import { GetCurrentUserQuery } from './get-current-user.query';

export class GetCurrentUserQueryHandler implements QueryHandler<GetCurrentUserQuery, User> {
  constructor(private readonly repository: UserRepository) {}

  handle(_query: GetCurrentUserQuery): Promise<User> {
    return this.repository.getCurrentUser();
  }
}
