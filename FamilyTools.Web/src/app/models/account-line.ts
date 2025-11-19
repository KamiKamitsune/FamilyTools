import { BaseModel } from './base-model';
import { User } from './user';

export class AccountLine extends BaseModel {
  user: User;
  value: number;

  constructor(user: User, value: number, id?: number, creationDate?: Date, updateDate?: Date){
    super(id, creationDate, updateDate);
    this.user = user;
    this.value = value;
  }
}
