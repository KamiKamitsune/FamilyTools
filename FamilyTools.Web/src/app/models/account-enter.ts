import { BaseModel } from './base-model';
import { AccountLine } from './account-line';
import { AccountTag } from './account-tag';

enum OperationType{
      Unknown = 0,
      Prelevement = 1,
      PaiementCarte = 2,
      VirementRecu = 3,
      VirementEmis = 4,
      RemboursementPret = 5,
      Reglement = 6,
      Avoir = 7,
      Cotisation = 8,
      ChequeEmis = 9
    }

export class AccountEnter extends BaseModel {
  lines: AccountLine[];
  tag: AccountTag;
  name: string;
  operationType: OperationType;
  totalValue: number;
  date: Date;

  constructor(name: string, operationType: OperationType,  totalValue: number, tag: AccountTag, date: Date, lines: AccountLine[], id?: number, creationDate?: Date, updateDate?: Date){
    super(id, creationDate, updateDate);
    this.lines = lines;
    this.tag = tag;
    this.name = name;
    this.operationType = operationType;
    this.totalValue = totalValue;
    this.date = date;
  }

    getTotal() : number{
      return this.lines.reduce((acc, line) => acc + line.value, 0)
    }
}
