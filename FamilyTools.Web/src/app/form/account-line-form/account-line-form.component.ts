import { Component, computed, effect, inject, input, Input, numberAttribute, Output, signal } from '@angular/core';
import { AccountLine } from '../../models/account-line';
import { UserService } from '../../service/accountService/user.service';
import { form, required, Field } from '@angular/forms/signals';
import { EnterService } from '../../service/accountService/enter.service.service';
import { User } from '../../models/user';

@Component({
  selector: 'app-account-line-form',
  imports: [Field],
  templateUrl: './account-line-form.component.html',
  styleUrl: './account-line-form.component.css',
})
export class AccountLineFormComponent{

  user = input<User>();
  value = input<number>(0);
    
  userService = inject(UserService);
  enterService = inject(EnterService);
  
  lineModel = signal<AccountLine>({
    user: this.user()!,
    value: this.value()
  });
  
  lineForm = form(this.lineModel, (schemaPath) => {
    required(schemaPath.value, {message: "Value is required"})
    required(schemaPath.user, {message: "User is required"})
  });

  constructor(){
    console.log("userComponent for ");
    console.log(this.user());
    console.log(this.value());
    console.log(this.userService.Users())

    effect(() => {
      
    })
  }
}
