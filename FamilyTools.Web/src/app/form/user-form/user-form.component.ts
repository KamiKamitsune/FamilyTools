import { Component, signal, ChangeDetectionStrategy, inject } from '@angular/core';
import { User } from '../../models/user';
import { form, Field, required } from '@angular/forms/signals';
import { UserService } from '../../service/accountService/user.service';

@Component({
  selector: 'app-user-form',
  imports: [Field],
  templateUrl: './user-form.component.html',
  styleUrl: './user-form.component.css',
})
export class UserFormComponent {

  service = inject(UserService);
  
  userModel = signal<User>({
    firstName : '',
    lastName : '',
    userName : ''  });

    userForm = form(this.userModel, (schemaPath) => {
      required(schemaPath.firstName, {message: 'FirstName is required'})
      required(schemaPath.lastName, {message: 'LastName is required'})
      required(schemaPath.userName, {message: 'UserName is required'})
    });

    onSubmit(event: Event){
      event.preventDefault();

      const credentials = this.userModel();
      console.log('User in with:', credentials)
      var user = new User(
        credentials.firstName,
        credentials.lastName,
        credentials.userName
      )
      this.userForm().reset();
      this.service.createUserApi(user);

    }
}