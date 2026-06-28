import { inject, Injectable, signal } from '@angular/core';
import { HttpHelperService } from '@core/http/http-helper.service';
import { AppSettings } from '@core/config/app.constants';
import { OperationType } from '@easycompta/models/operation-type';

@Injectable({
  providedIn: 'root',
})
export class OperationTypeService {
  private readonly http = inject(HttpHelperService);

  readonly operationTypes = signal<OperationType[]>([]);

  constructor() {
    this.getOperationTypeListApi();
  }

  public getOperationTypeListApi() {
    return this.http.get<OperationType[]>(AppSettings.OPERATIONSTYPES_URL).subscribe({
      next: result => this.operationTypes.set(result),
      error: error => console.log(error),
    });
  }
}
