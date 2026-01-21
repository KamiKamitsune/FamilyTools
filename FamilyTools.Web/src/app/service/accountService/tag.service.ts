import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { AccountTag } from '../../models/account-tag';
import { HttpHelperService } from '../httpHelper/http-helper.service';
import { AppSetings } from '../../constants/app.constants';

@Injectable({
  providedIn: 'root',
})

export class TagService {
  httpservice = inject(HttpHelperService)

  constructor() {
    this.getTagList();
  }
  
  allTag = signal<AccountTag[]>([]);
  readonly tags = signal<AccountTag[]>([]);
  
  public getTagList(){
    if (this.httpservice) {
      this.httpservice.get<AccountTag[]>(`${AppSetings.TAG_URL}`).subscribe({
        next: result => {
          if (result) {
            this.allTag.set(result)}
          },
        error: error => console.log(error)
      });
    }

  }

  public create(tag: AccountTag) {
    if (this.httpservice) {
      this.httpservice.post<AccountTag>(`${AppSetings.TAG_URL}create`, tag).subscribe({
        next: result => {
          if (result) {
            console.log(`le tag ${result.name} avec la couleur ${result.color} a été ajouté`);
          }
          this.getTagList();
        },
        error: error => console.log(error)
      });
    }
  }

  public delete(id: number){
    this.httpservice.delete(`${AppSetings.TAG_URL}delete`,id).subscribe({
      next : reponse => this.getTagList(),
        error: error => console.log(error)
    });
    
  }
}
