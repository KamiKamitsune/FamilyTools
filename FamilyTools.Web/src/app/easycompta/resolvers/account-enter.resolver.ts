import {ActivatedRouteSnapshot, ResolveFn, RouterStateSnapshot} from '@angular/router';
import { inject } from '@angular/core';
import {AccountEnterService} from '@easycompta/data/account-enter.service';
import {AccountEnter} from '@easycompta/models/account-enter';

export const accountEnterResolver : ResolveFn<AccountEnter> = (
  route: ActivatedRouteSnapshot,
  state: RouterStateSnapshot
  ) => {
  const enterService = inject(AccountEnterService);
  const enterId = route.paramMap.get('id')!;
  return enterService.get(Number.parseInt(enterId));
};
