import { Component, inject, isWritableSignal, OnInit, resource, signal } from '@angular/core';
import { AccountTag } from '../../models/account-tag';
import { TagFormComponent } from "../../form/tag-form/tag-form.component";
import { TagService } from '../../service/accountService/tag.service';

@Component({
  selector: 'app-accounttag',
  imports: [TagFormComponent],
  templateUrl: './account-tag.component.html',
  styleUrl: './account-tag.component.css'
})
export class AccountTagComponent{
  
  readonly service = inject(TagService);

  deleteTag(id:number){
    this.service.delete(id);
  }
}
