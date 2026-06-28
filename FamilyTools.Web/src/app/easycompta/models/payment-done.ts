import { BaseModel } from '@shared/models/base-model';
import { User } from '@shared/models/user';

export interface PaymentDone extends BaseModel{
    user: User;
    paymentIsDone : boolean;
    total: number;
}