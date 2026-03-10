import { Component, computed, effect, inject, input, Signal, signal } from '@angular/core';
import { OperationType } from '../../enum/app.enum';
import { AccountEnter } from '../../models/account-enter';
import { UserService } from '../../service/accountService/user.service';
import { TagService } from '../../service/accountService/tag.service';
import { applyEach, Field, form, required } from '@angular/forms/signals';
import { KeyValuePipe } from '@angular/common';
import { AccountLineFormComponent } from '../account-line-form/account-line-form.component';
import { EnterService } from '../../service/accountService/enter.service.service';
import { AccountLine } from '../../models/account-line';
import { User } from '../../models/user';

@Component({
  selector: 'app-account-enter-form',
  imports: [Field, KeyValuePipe, AccountLineFormComponent],
  templateUrl: './account-enter-form.component.html',
  styleUrl: './account-enter-form.component.css',
})
export class AccountEnterFormComponent{
  
  userService = inject(UserService);
  tagService = inject(TagService);
  enterService = inject(EnterService);


  id = input<number| undefined>();
  UserUse  = signal<User[]>([]); 
  remainingUser : Signal<User[]> = computed(() => {
    return this.userService.Users().filter(user => !this.UserUse().some(x => x.id == user.id));
  });
  OperationType = OperationType;
  lines: AccountLine[] = [];

  enterModel = signal<AccountEnter>({
    lines: [],
    tag: {
      name: "",
      color: ""
    },
    name: '',
    operationType: OperationType.Unknown,
    totalValue: 0,
    date: new Date(),
    isDisabled: false
  })
  
  enterForm = form(this.enterModel, (schemaPath) => {
    required(schemaPath.name, {message: "Name is required"});
    required(schemaPath.tag, {message: "tag is required"});
    required(schemaPath.operationType, {message: "operation type is required"});
    applyEach(schemaPath.lines, (line) => {
      required(line.user, {message:"user is required"});
      required(line.value, {message:"value is required"});
    })
  })
  
  constructor(){
    effect(() => {    
    for (let i = 0; i < this.remainingUser().length; i++) {
      this.addLines();      
    }
    });
  }


  onSubmit($event: SubmitEvent) {
    console.log("submit");
  }

  addLines(){
    if (this.remainingUser().length > 0) {
      const user = this.remainingUser()[0];
      let line : AccountLine = {
      user: user,
      value: 5
      };
      this.lines.push(line);
      console.log("userservice");
      console.log(this.userService.Users());
      console.log("lines");
      console.log(this.lines);
      this.UserUse.update(current => [...current, user]);
    }
  }
}
