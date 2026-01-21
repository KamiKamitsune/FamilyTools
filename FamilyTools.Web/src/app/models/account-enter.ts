import { BaseModel } from './base-model';
import { AccountLine } from './account-line';
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
