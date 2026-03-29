import {Component, inject, input} from '@angular/core';
import {User} from '../../models/user';
import {UserService} from '../../service/accountService/user.service';

@Component({
  selector: 'app-userdetail',
  imports: [],
  templateUrl: './user-detail.component.html',
  styleUrl: './user-detail.component.css',
})
export class UserDetailComponent{

  user = input.required<User>();
  private service = inject(UserService);

  updateUser(){
    this.service.updateUserApi(this.user());
  }
}
