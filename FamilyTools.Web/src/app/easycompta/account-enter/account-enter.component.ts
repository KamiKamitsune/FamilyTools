import { Component, inject, OnInit } from '@angular/core';
import { AccountService } from '../../service/accountService/account.service';
import { HttpClient } from '@angular/common/http';
import { FormArray, FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { AccountEnter } from '../../models/account-enter';
import { AccountTag } from '../../models/account-tag';
import { User } from '../../models/user';
import { AccountLine } from '../../models/account-line';
import { RedirectCommand } from '@angular/router';

@Component({
  selector: 'app-accountenter',
  imports: [ReactiveFormsModule],
  templateUrl: './account-enter.component.html',
  styleUrl: './account-enter.component.css'
})

export class AccountEnterComponent implements OnInit{
  
  readonly service = inject(AccountService);
  private readonly http = inject(HttpClient);
  private formBuilder = inject(FormBuilder);

  enter : AccountEnter = new AccountEnter("",0, 0,new AccountTag("", "") , new Date(), []);

  id_user_modifier : number[] = [];

  error_total_and_value : boolean = false;

  // selectionné les itilisateurs concerné (tous par default)
  // pouvoir attribuer des valeur a un utilisateur en selectionnant + de détail
  // mettre une valeur total qui divise pour chaque utilisateur
  // quand on change une valeur pour un utilisateur spécifique, ca recalcule pour les autres par rapport au total
  // si cela a déja été modifier pour un utilisateur, on ne met pas a jour pour lui mais seulement pour ceux qui n'ont pas été modifier
  // si tout le monde a été modifier et que le total de chacun ne correspond pas au total défini, on met une alerte

  linesFormTemplate : FormGroup = this.formBuilder.group({
    user : ['', Validators.required],
    value : [0, Validators.required]
  })

  //tester le formbuilder pour mettre une liste de line
  enter_form = this.formBuilder.group({
    tag : [-1, Validators.required],
    name : ['', Validators.required],
    totalValue : ['', Validators.required],
    date : ['', Validators.required],
    lines : this.formBuilder.array([
      this.linesFormTemplate
    ])
  })

  ngOnInit(): void {
      this.service.all_user.forEach(user => this.enter.lines.push(new AccountLine(user, 0)));
  }

  get tag(){
    return this.enter_form.get("tag")
  }

  get name(){
    return this.enter_form.get("name")
  }

  get totalValue(){
    return this.enter_form.get("totalValue")
  }

  get date(){
    return this.enter_form.get("date")
  }

  get lines() : FormArray{
    return this.enter_form.get("lines") as FormArray
  }

  addLineInForm(){
    this.lines.push(this.linesFormTemplate);
  }

  createEnter(){
    this.enterFormToEnter();
    this.createEnterApi();
  }

  createEnterApi(){
    this.http.post<User[]>('/api/accounttag/create', this.enter).subscribe({
      next : result =>  null, //TODO : redirection
      error: error => console.error(error)
    })
  }

  enterFormToEnter(){ 

    this.enter.name = this.name?.value as string;
    this.enter.totalValue = Number(this.totalValue?.value);
    this.enter.tag = this.service.all_tag.find(value => value.id == Number(this.tag)) ?? new AccountTag("", "");
    this.enter.date = new Date(this.date?.value as string);

    this.enter.lines = this.lines.controls.map(line => new AccountLine(
      this.service.all_user.find(user => user.id = Number(line.get("user")?.value)) as User, 
      line.get("value")?.value)
    );
  }

  changeValueForOneLines(userId: number, value: number){
    
    this.enter.lines.forEach(line => line.userLink.id == userId ? line.value = value : line.value);

    if (!this.id_user_modifier.includes(userId)) {
      this.id_user_modifier.push(userId);
    }

    let total = this.enter.totalValue;

    this.enter.lines.forEach(line => {
      if (line.userLink.id) {        
        if (this.id_user_modifier.includes(line.userLink.id)) {
          total -= line.value;
        }
      }
    });


    let value_by_user = total / this.enter.lines.map(line => this.id_user_modifier.includes(line.userLink.id)).length;
    this.enter.lines.forEach(line => line.value = this.id_user_modifier.includes(line.id) ? line.value : value_by_user );

    if (this.enter.totalValue != this.enter.getTotal()) {
      this.error_total_and_value == true;
    }
  }

  deleteLine(idLien : number){
    this.enter.lines.filter(line => line.id == idLien).pop();
  }

  addLine(id_user : number, value : number){

    let user_find = this.service.all_user.find(user => user.id == id_user);

    if (user_find) {
      this.lines.push(this.linesFormTemplate)
    }
  }


}
