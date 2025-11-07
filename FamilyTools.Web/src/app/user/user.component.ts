import { Component, inject} from '@angular/core';
import { User } from '../models/user';
import { FormControl, FormGroup, Validators, ReactiveFormsModule } from '@angular/forms';
import { DatePipe } from '@angular/common';
import { UserService } from '../service/accountService/user.service';

@Component({
  selector: 'app-user',
  standalone: true,
  templateUrl: './user.component.html',
  styleUrl: './user.component.css',
  imports: [ReactiveFormsModule, DatePipe]
})

export class UserComponent{
  
  service = inject(UserService);
  
  userform : FormGroup = new FormGroup({
    firstname : new FormControl('',{nonNullable : true, validators: [Validators.required, Validators.minLength(3)]}),
    lastname: new FormControl('',{nonNullable : true, validators: [Validators.required, Validators.minLength(3)]}),
    username : new FormControl('',{nonNullable : true, validators: [Validators.required, Validators.minLength(3)]}),
  });

  isSubmit:boolean = false;

  // form
  get firstname(){
    return this.userform.get('firstname')
  }

  get lastname(){
    return this.userform.get('lastname')
  }

  get username(){
    return this.userform.get('username')
  }

  

  CreateUser(){
    this.isSubmit = true;

    if(this.userform.valid){
        let user : User = new User (
          this.userform.getRawValue().firstname,
          this.userform.getRawValue().lastname,
          this.userform.getRawValue().username
        );
        this.service.createUserApi(user);
        this.userform.reset();
    }

  }

  DeleteUser(id : number){
    this.service.deleteUserApi(id);
  }

}