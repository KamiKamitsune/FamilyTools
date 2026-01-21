import { Component, inject, signal } from '@angular/core';
import { AccountTag } from '../../models/account-tag';
import { form, required } from '@angular/forms/signals';
import { TagService } from '../../service/accountService/tag.service';

@Component({
  selector: 'app-tag-form',
  imports: [],
  templateUrl: './tag-form.component.html',
  styleUrl: './tag-form.component.css',
})

export class TagFormComponent {

  service = inject(TagService)

  tagModel = signal<AccountTag>({
    name: '',
    color: ''
  })

  tagForm = form(this.tagModel, schemaPath => {
      required(schemaPath.name, {message: 'Name is required'})
      required(schemaPath.color, {message: 'Color is required'})
  })

  onSubmit(event: Event){
      event.preventDefault();

      const credentials = this.tagModel();
      console.log('User in with:', credentials)
      this.service.create(credentials);
      this.tagForm().reset();
    }
}
