import { inject, Injectable } from '@angular/core';
import { HttpHelperService } from '../httpHelper/http-helper.service';
import { UserService } from './user.service';
import { User } from '../../models/user';

@Injectable({
  providedIn: 'root',
})
export class EnterService {
  private httpservice = inject(HttpHelperService);
  private userService = inject(UserService);

}