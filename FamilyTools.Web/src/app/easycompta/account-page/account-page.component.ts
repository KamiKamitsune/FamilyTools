import { Component, ElementRef, inject, OnInit, ViewChild } from '@angular/core';
import { AccountPage } from '../../models/account-page';
import { HttpClient } from '@angular/common/http';
import { Router, RouterLink } from '@angular/router';
import { DatePipe } from '@angular/common';
import { AccountTag } from '../../models/account-tag';
import { OPERATIONTYPESTRING } from './../../constants/app.constants'
import { PaymentDone } from '../../models/payment-done';

@Component({
  selector: 'app-accountpage',
  imports: [RouterLink, DatePipe],
  templateUrl: './account-page.component.html',
  styleUrl: './account-page.component.css'
})
export class AccountPageComponent implements OnInit {
  pages: AccountPage[] = [];
  current_page: AccountPage | undefined;
  date_list: Date[] = [];
  last_date: Date = new Date();
  current_tags: AccountTag[] = [];
  selectedFile: File | undefined;
  @ViewChild('addCsvButton', { static: false }) addCsvButton!: ElementRef<HTMLInputElement>;
  operationtype = OPERATIONTYPESTRING;


  private readonly _http = inject(HttpClient);
  private readonly router = inject(Router);

  ngOnInit(): void {
    this.get_all_month();
  }

  private get_page_id_by_month(date: Date): number | undefined {
    const page = this.pages.find(x => x.date.getMonth() == date.getMonth() && x.date.getFullYear() == date.getFullYear());
    return page?.id;
  }

  public async changePages(event: Event) {
    const select = event.target as HTMLSelectElement;
    const value = new Date(select.value);
    const pageid = this.get_page_id_by_month(value)
    if (value == undefined || !this.pages.some(x => x.id == pageid)) {
      this.call_new_page(value.getMonth(), value.getFullYear());
    }
    else {
      this.current_page = this.pages.find(x => x.id == pageid);
    }
  }

  private get_all_month() {
    if (this._http) {
      this._http.get<Date[]>('api/easycompta/AccountPage/getallmonth').subscribe({
        next: result => {
          result.forEach(e => {
            this.date_list.push(new Date(e));
          });
          this.date_list.sort((a, b) => b.getTime() - a.getTime());
          this.last_date = this.date_list.reduce((a, b) => a > b ? a : b);
          this.call_new_page(this.last_date.getMonth(), this.last_date.getFullYear());
        },
        error: console.error
      });
    }
  }

  private call_new_page(month: number, year: number) {
    if (this._http) {
      month = month + 1;
      let page = this.pages.find(x => x.date.getMonth() == month && x.date.getFullYear() == year);
      if (page != undefined) {
        this.current_page = page;
      }
      else {
        this._http.get<AccountPage>(`api/easycompta/AccountPage/Get/${month}/${year}`).subscribe({
          next: result => {
            result.date = new Date(result.date);
            this.current_page = result;
          },
          error: console.error
        });
      }
    }
  }

  addAccountEnter() {
    this.router.navigate(["/accountenter"]);
  }

  onFileSelected(event: any) {
    const file: File = event.target.files[0];
    if (file) {
      this.selectedFile = file;
    }
  }

  uploadFile() {
    if (this.selectedFile) {
      const formData = new FormData();

      formData.append('csvFile', this.selectedFile, this.selectedFile.name);

      this._http.post("api/easycompta/ImportCSV", formData).subscribe({
        next: (response) => {
          this.addCsvButton.nativeElement.value = "";
          window.location.reload();
        },
        error: (error) => {
          console.error('Erreur lors de l\'envoi', error);
        }
      });
    }
  }

  clickPaymentDone(event: any){
    if (this._http) {
      this._http.patch<PaymentDone>(`api/easycompta/PaymentDone/Patch/${event.target.id}`, event.target.checked).subscribe({
        next: (response) => {
          this.current_page?.paymentDones.map(x => x.id == response.id? x = response : null);
        },
        error: (error => {
          console.log(error);
          event.target.checked = !event.target.checked;
        })
      })
    }
  }

}
