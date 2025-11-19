import { BaseModel } from "./base-model";
import { User } from "./user";

export class paymentDones extends BaseModel{
    user: User;
    paymentIsDone : boolean;
    total: number;

    constructor(user: User, paymentIsDone : boolean, total: number,  id?: number, creationDate?: Date, updateDate?: Date){
        super(id, creationDate, updateDate);
        this.user = user;
        this.paymentIsDone = paymentIsDone;
        this.total = total;
        }
}