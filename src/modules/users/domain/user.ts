import { UserId } from './user-id';

export interface User {
  readonly id: UserId;
  readonly name: string;
  readonly email: string;
}
