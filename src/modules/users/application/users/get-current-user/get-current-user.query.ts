import { Query } from '../../../../../common/application/messaging/query';
import { User } from '../../../domain/user';

export class GetCurrentUserQuery implements Query<User> {
  readonly type = 'users.get-current';
}
