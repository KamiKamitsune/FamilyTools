import { BaseModel } from './base-model';
import {EMPTY_USER_FORM_MODEL, User} from './user';

export interface AccountLine extends BaseModel {
  user: User;
  value: number;
}

export const EMPTY_LINE_FORM_MODEL = {
  user: EMPTY_USER_FORM_MODEL,
  value: 0
};


export const EMPTY_LIST_LINE_FORM_MODEL = [
  EMPTY_LINE_FORM_MODEL,
  EMPTY_LINE_FORM_MODEL
]
