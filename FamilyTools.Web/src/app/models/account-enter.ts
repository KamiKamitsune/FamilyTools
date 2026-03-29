import { BaseModel } from './base-model';
import {AccountLine, EMPTY_LINE_FORM_MODEL, EMPTY_LIST_LINE_FORM_MODEL} from './account-line';
import { AccountTag } from './account-tag';
import { OperationType } from '../enum/app.enum';

export interface AccountEnter extends BaseModel {
  lines: AccountLine[];
  tag: AccountTag;
  name: string;
  operationType: OperationType;
  totalValue: number;
  date: Date;
  isDisabled: boolean;
}

export const EMPTY_ENTER_FORM_MODEL = {
  lines: EMPTY_LIST_LINE_FORM_MODEL,
  tag: {
    name: "",
    color: ""
  },
  name: '',
  operationType: OperationType.Unknown,
  totalValue: 0,
  date: new Date(),
  isDisabled: false
};
