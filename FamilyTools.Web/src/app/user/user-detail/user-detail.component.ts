import { ChangeDetectionStrategy, Component, inject, input } from '@angular/core';
import { User } from '@shared/models/user';
import {UserService} from '@user/data/user.service';

@Component({
  selector: 'app-userdetail',
  imports: [],
  templateUrl: './user-detail.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './user-detail.component.css',
})
export class UserDetailComponent{

  user = input.required<User>();
  private service = inject(UserService);

  updateUser(){
    this.service.updateUserApi(this.user());
  }
}
