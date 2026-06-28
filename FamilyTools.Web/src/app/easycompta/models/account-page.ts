import { BaseModel } from '@shared/models/base-model';
import { User } from '@shared/models/user';
import { AccountEnter } from './account-enter';
import { PaymentDone } from './payment-done';

export interface AccountPage extends BaseModel {
  name: string;
  enters: AccountEnter[];
  paymentDones: PaymentDone[];
  isClosing: boolean;
  date: Date;
}
