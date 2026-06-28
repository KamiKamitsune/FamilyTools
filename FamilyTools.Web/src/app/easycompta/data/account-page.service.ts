import { inject, Injectable } from '@angular/core';
import { Observable } from 'rxjs';
import { HttpHelperService } from '@core/http/http-helper.service';
import { AppSettings } from '@core/config/app.constants';
import { AccountPage } from '@easycompta/models/account-page';
import { AccountEnter } from '@easycompta/models/account-enter';
import { PaymentDone } from '@easycompta/models/payment-done';

@Injectable({
  providedIn: 'root',
})
export class AccountPageService {
  private readonly http = inject(HttpHelperService);

  public getAllMonths(): Observable<Date[]> {
    return this.http.get<Date[]>(`${AppSettings.PAGE_URL}getallmonth`);
  }

  public getPage(month: number, year: number): Observable<AccountPage> {
    return this.http.get<AccountPage>(`${AppSettings.PAGE_URL}Get/${month}/${year}`);
  }

  public importCsv(file: File): Observable<{ message: string }> {
    const formData = new FormData();
    formData.append('csvFile', file, file.name);
    return this.http.post<{ message: string }>(AppSettings.IMPORT_CSV_URL, formData);
  }

  public setPaymentDone(id: number, status: boolean): Observable<PaymentDone> {
    return this.http.patch<PaymentDone>(`${AppSettings.PAYMENTDONE_URL}Patch/${id}`, status);
  }

  public setEnterDisabled(id: number, disabled: boolean): Observable<AccountEnter> {
    return this.http.patch<AccountEnter>(`${AppSettings.ENTER_URL}Disabled/${id}`, disabled);
  }

  public getPaymentDonesByPage(pageId: number): Observable<PaymentDone[]> {
    return this.http.get<PaymentDone[]>(`${AppSettings.PAYMENTDONE_URL}GetByPageId/${pageId}`);
  }
}
