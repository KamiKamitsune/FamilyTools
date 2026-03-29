import { inject, Injectable } from '@angular/core';
import { HttpHelperService } from '../httpHelper/http-helper.service';
import { UserService } from './user.service';
import { User } from '../../models/user';
import {AppSetings} from '../../constants/app.constants';
import {finalize, Observable} from 'rxjs';
import {AccountEnter} from '../../models/account-enter';

@Injectable({
  providedIn: 'root',
})
export class EnterService {
  private helperService = inject(HttpHelperService);
  private userService = inject(UserService);

  public getEnter(id: number): Observable<AccountEnter> {
    return this.helperService.get<AccountEnter>(`${AppSetings.ENTER_URL}index`, id);
  }

}
