import { BaseModel } from './base-model';
import { User } from './user';
import { AccountEnter } from './account-enter';
import { PaymentDone } from './payment-done';

export interface AccountPage extends BaseModel {
  name: string;
  enters: AccountEnter[];
  paymentDones: PaymentDone[];
  isClosing: boolean;
  date: Date;
}
