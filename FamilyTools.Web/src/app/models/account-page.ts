import { BaseModel } from './base-model';
import { User } from './user';
import { AccountEnter } from './account-enter';
import { PaymentDone } from './payment-done';

export class AccountPage extends BaseModel {
  name: string;
  enters: AccountEnter[];
  paymentDones: PaymentDone[];
  isClosing: boolean;
  date: Date;

  constructor(name: string, enters: AccountEnter[], paymentDones: PaymentDone[], isClosing: boolean, date: Date, id?: number, creationDate?: Date, updateDate?: Date){
    super(id, creationDate, updateDate);
    this.name = name;
    this.enters = enters;
    this.paymentDones = paymentDones;
    this.isClosing = isClosing;
    this.date = new Date(date);
  }
  
    getTotal(){
      this.enters.reduce((acc, enter) => acc + enter.totalValue, 0)
    }
}
