import { Component, inject, OnInit } from '@angular/core';
import { UserService } from '../../service/accountService/user.service';

@Component({
  selector: 'app-userdetail',
  imports: [],
  templateUrl: './user-detail.component.html',
  styleUrl: './user-detail.component.css',
})
export class UserDetailComponent{

  private service = inject(UserService);
  
  updateUser(){
    this.service.updateUserApi();
  }
}
