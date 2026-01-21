import { BaseModel } from './base-model';
import { User } from './user';

export interface AccountLine extends BaseModel {
  user: User;
  value: number;
}
