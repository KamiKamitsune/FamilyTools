import { Component, inject} from '@angular/core';
import { User } from '../models/user';
import { FormControl, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { UserService } from '../service/accountService/user.service';
import { UserFormComponent } from "../form/user-form/user-form.component";

@Component({
  selector: 'app-user',
  standalone: true,
  templateUrl: './user.component.html',
  styleUrl: './user.component.css',
  imports: [ReactiveFormsModule, DatePipe, UserFormComponent]
})

export class UserComponent{
  
  service = inject(UserService);

  DeleteUser(id : number){
    this.service.deleteUserApi(id);
  }

}