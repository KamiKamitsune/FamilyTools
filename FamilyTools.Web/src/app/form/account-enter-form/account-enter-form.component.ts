import { Component, inject, signal } from '@angular/core';
import { OperationType } from '../../enum/app.enum';
import { AccountEnter } from '../../models/account-enter';
import { UserService } from '../../service/accountService/user.service';
import { TagService } from '../../service/accountService/tag.service';
import { Field, form, required } from '@angular/forms/signals';
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
export class AccountEnterFormComponent {
  
  userService = inject(UserService);
  tagService = inject(TagService);
  enterService = inject(EnterService);
  
  remainingUser = [...this.userService.Users()];

  OperationType = OperationType;

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
  })
  
  constructor(){
    console.log(this.userService.Users());
    console.log(this.remainingUser.length)
    for (let i = 0; i < this.remainingUser.length; i++) {
      this.addLines();      
    }
  }

  onSubmit($event: SubmitEvent) {
  throw new Error('Method not implemented.');
  }

  addLines(){
    if (this.remainingUser.length > 0) {
      let line : AccountLine = {
      user: this.remainingUser[0],
      value: 5
    };
    this.enterModel().lines.push(line);
    }
  }
}
