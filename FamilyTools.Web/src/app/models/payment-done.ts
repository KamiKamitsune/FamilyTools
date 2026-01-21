import { BaseModel } from "./base-model";
import { User } from "./user";

export interface PaymentDone extends BaseModel{
    user: User;
    paymentIsDone : boolean;
    total: number;
}