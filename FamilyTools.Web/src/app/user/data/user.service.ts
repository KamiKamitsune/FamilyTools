import {inject, Injectable, signal} from '@angular/core';
import { User } from '@shared/models/user';
import { HttpHelperService } from '@core/http/http-helper.service';
import { AppSettings } from '@core/config/app.constants';
import {finalize, Observable} from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserService{

  private helperService = inject(HttpHelperService);

  constructor(){
    this.getUserListApi();
  }

  readonly users = signal<User[]>([]);

  public getUserListApi(){
    return this.helperService.get<User[]>(`${AppSettings.USER_URL}`).subscribe({
      next : result => result != null ? this.users.set(result) : console.log("response void"),
      error : error => console.log(error)
    })
  }

  public createUserApi(user: User) {
    this.helperService.post(`${AppSettings.USER_URL}create`, user).subscribe({
      next: () => this.getUserListApi(),
      error: error => console.log(error)
    });
  }

  public deleteUserApi(id: number) {
    this.helperService.delete(`${AppSettings.USER_URL}delete`, id).subscribe({
      next: () => this.getUserListApi(),
      error: error => console.log(error)
    });
  }

  public getUser(id: number) {
    return this.helperService.get<User>(`${AppSettings.USER_URL}index`, id).pipe(
      finalize(() => this.getUserListApi())
    )
  }

  public updateUserApi(user: User) {
    this.helperService.put<User>(`${AppSettings.USER_URL}edit`, user).subscribe({
      next: () => this.getUserListApi(),
      error: error => console.log(error)
    });
  }

}
