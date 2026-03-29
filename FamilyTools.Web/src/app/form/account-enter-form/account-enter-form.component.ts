import {Component, computed, effect, inject, input, linkedSignal, Signal, signal} from '@angular/core';
import {OperationType} from '../../enum/app.enum';
import {AccountEnter, EMPTY_ENTER_FORM_MODEL} from '../../models/account-enter';
import {UserService} from '../../service/accountService/user.service';
import {TagService} from '../../service/accountService/tag.service';
import {applyEach, form, FormField, required} from '@angular/forms/signals';
import {KeyValuePipe} from '@angular/common';
import {AccountLine} from '../../models/account-line';
import {User} from '../../models/user';

@Component({
  selector: 'app-account-enter-form',
  imports: [KeyValuePipe, FormField],
  templateUrl: './account-enter-form.component.html',
  styleUrl: './account-enter-form.component.css',
})
export class AccountEnterFormComponent{

  userService = inject(UserService);
  tagService = inject(TagService);


  enter = input<AccountEnter>();
  UserUse = signal<User[]>([]);
  remainingUser : Signal<User[]> = computed(() => {
    return this.userService.Users().filter(user => !this.UserUse().some(x => x.id == user.id));
  });
  OperationType = OperationType;

  enterModel = linkedSignal({
    source: this.enter,
    computation: (enter) => enter
    ? enter : EMPTY_ENTER_FORM_MODEL
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
      const remaining = this.remainingUser();
      if (remaining.length > 0 && this.enterModel().lines.length === 0) {
        for (let i = 0; i < remaining.length; i++) {
          this.addLines();
        }
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
      value: 0
      };
      this.enterModel.update(model => ({
        ...model,
        lines: [...model.lines, line]
      }));
      this.UserUse.update(current => [...current, user]);
    }
  }
}
