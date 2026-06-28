import { BaseModel } from './base-model';

export interface User extends BaseModel{
  firstName: string;
  lastName: string;
  userName: string;
}

export const EMPTY_USER_FORM_MODEL = {
  firstName: "",
  lastName: "",
  userName: "",
};
