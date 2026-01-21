import { BaseModel } from './base-model';

export interface User extends BaseModel{
  firstName: string;
  lastName: string;
  userName: string;
}
