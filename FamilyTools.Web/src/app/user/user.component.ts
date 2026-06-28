import { ChangeDetectionStrategy, Component, inject } from '@angular/core';
import { User } from '@shared/models/user';
import { FormControl, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { UserService } from '@user/data/user.service';
import { UserFormComponent } from "./user-form/user-form.component";

@Component({
  selector: 'app-user',
  standalone: true,
  templateUrl: './user.component.html',
  changeDetection: ChangeDetectionStrategy.OnPush,
  styleUrl: './user.component.css',
  imports: [ReactiveFormsModule, DatePipe, UserFormComponent]
})

export class UserComponent{
  
  service = inject(UserService);

  DeleteUser(id : number){
    this.service.deleteUserApi(id);
  }

}