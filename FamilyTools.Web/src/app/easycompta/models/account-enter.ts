import { BaseModel } from '@shared/models/base-model';
import { AccountLine } from './account-line';
import { AccountTag } from './account-tag';
import { OperationType } from './operation-type';

export interface AccountEnter extends BaseModel {
  lines: AccountLine[];
  tag: AccountTag;
  name: string;
  operationType: OperationType;
  totalValue: number;
  date: Date;
  isDisabled: boolean;
}

/** Payload d'entrée envoyé au back : entités liées référencées par id (cf. AccountEnterDto côté API). */
export interface AccountLineDto {
  id?: number;
  userId: number;
  value: number;
}

export interface AccountEnterDto {
  id?: number;
  name: string;
  date: string;
  isDisabled: boolean;
  tagId: number;
  operationTypeId: number;
  totalValue: number;
  lines: AccountLineDto[];
}
