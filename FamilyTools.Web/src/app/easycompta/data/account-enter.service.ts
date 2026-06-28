import { inject, Injectable } from '@angular/core';
import { HttpHelperService } from '@core/http/http-helper.service';
import { AppSettings } from '@core/config/app.constants';
import {Observable} from 'rxjs';
import {AccountEnter, AccountEnterDto} from '@easycompta/models/account-enter';

@Injectable({
  providedIn: 'root',
})
export class AccountEnterService {
  private helperService = inject(HttpHelperService);

  public get(id: number): Observable<AccountEnter> {
    return this.helperService.get<AccountEnter>(`${AppSettings.ENTER_URL}index`, id);
  }

  public update(enter: AccountEnterDto) {
    return this.helperService.put<AccountEnter>(`${AppSettings.ENTER_URL}edit`, enter);
  }

  public create(enter: AccountEnterDto) {
    return this.helperService.post<AccountEnter>(`${AppSettings.ENTER_URL}create`, enter);
  }

  public delete(id: number) {
    return this.helperService.delete(`${AppSettings.ENTER_URL}delete`, id);
  }
}
