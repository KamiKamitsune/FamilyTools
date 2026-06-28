import { BaseModel } from '@shared/models/base-model';
import { User } from '@shared/models/user';

export interface AccountLine extends BaseModel {
  user: User;
  value: number;
}
