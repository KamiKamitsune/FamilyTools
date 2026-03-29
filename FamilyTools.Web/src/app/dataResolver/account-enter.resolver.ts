import {ActivatedRouteSnapshot, ResolveFn, RouterStateSnapshot} from '@angular/router';
import { inject } from '@angular/core';
import {EnterService} from '../service/accountService/enter.service.service';
import {AccountEnter} from '../models/account-enter';

export const accountEnterResolver : ResolveFn<AccountEnter> = (
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot
  ) => {
  const enterService = inject(EnterService);
  const enterId = route.paramMap.get('id')!;
  return enterService.getEnter(Number.parseInt(enterId));
};
