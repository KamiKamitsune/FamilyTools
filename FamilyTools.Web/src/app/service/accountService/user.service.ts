import { inject, Injectable, signal } from '@angular/core';
import { User } from '../../models/user';
import { HttpClient } from '@angular/common/http';
import { HttpHelperService } from '../httpHelper/http-helper.service';
import { AppSetings } from '../../constants/app.constants';
import { finalize, Observable, Subscriber } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class UserService {

  private helperService = inject(HttpHelperService);

  constructor(){
    this.getUserListApi()
  }

  Users = signal<User[]>([]);

  public getUserListApi(){
    return this.helperService.get<User[]>(`${AppSetings.USER_URL}`).subscribe({
      next : result => result != null ? this.Users.set(result) : console.log("response void"),
      error : error => console.log(error)
    })
  }

  public async createUserApi(user: User) {
    this.helperService.post(`${AppSetings.USER_URL}create`, user).pipe(
      finalize(() => this.getUserListApi())
    );
  }

  public async deleteUserApi(id: number) {
    this.helperService.delete(`${AppSetings.USER_URL}delete`, id).pipe(
      finalize(() => this.getUserListApi())
    );
  }

  public async getUser(id: number){
    return this.helperService.get<User>(`${AppSetings.USER_URL}index`, id).pipe(
      finalize(() => this.getUserListApi())
    );
  }

  public async updateUserApi(user: User) {
    return this.helperService.put<User>(`${AppSetings.USER_URL}edit`, user).pipe(
      finalize(() => this.getUserListApi())
    );

  }

}
