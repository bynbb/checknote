import { Query } from '@cdev/common/application';
import { User } from '@cdev/modules/users/domain';

export class GetCurrentUserQuery implements Query<User> {
  readonly type = 'users.get-current';
}
