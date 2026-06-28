import { BaseModel } from '@shared/models/base-model';

export interface AccountTag extends BaseModel {
  name: string;
  color: string;
}