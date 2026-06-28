import { HttpClient } from '@angular/common/http';
import { inject, Injectable, signal } from '@angular/core';
import { AccountTag } from '@easycompta/models/account-tag';
import { HttpHelperService } from '@core/http/http-helper.service';
import { AppSettings } from '@core/config/app.constants';

@Injectable({
  providedIn: 'root',
})

export class AccountTagService {
  httpService = inject(HttpHelperService)

  constructor() {
    this.getTagList();
  }
  
  allTag = signal<AccountTag[]>([]);
  readonly tags = signal<AccountTag[]>([]);
  
  public getTagList(){
    if (this.httpService) {
      this.httpService.get<AccountTag[]>(`${AppSettings.TAG_URL}`).subscribe({
        next: result => {
          if (result) {
            this.allTag.set(result)}
          },
        error: error => console.log(error)
      });
    }

  }

  public create(tag: AccountTag) {
    if (this.httpService) {
      this.httpService.post<AccountTag>(`${AppSettings.TAG_URL}create`, tag).subscribe({
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

  public update(tag: AccountTag) {
    this.httpService.put<AccountTag>(`${AppSettings.TAG_URL}edit`, tag).subscribe({
      next: () => this.getTagList(),
      error: error => console.log(error)
    });
  }

  public delete(id: number){
    this.httpService.delete(`${AppSettings.TAG_URL}delete`,id).subscribe({
      next : reponse => this.getTagList(),
        error: error => console.log(error)
    });
    
  }
}
