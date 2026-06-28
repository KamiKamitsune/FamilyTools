import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { OperationTypeService } from './operation-type.service';
import { AppSettings } from '@core/config/app.constants';
import { OperationType } from '@easycompta/models/operation-type';

describe('OperationTypeService', () => {
  let service: OperationTypeService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(OperationTypeService);
    httpMock = TestBed.inject(HttpTestingController);

    // Le constructeur déclenche un premier chargement de la liste.
    httpMock.expectOne(AppSettings.OPERATIONSTYPES_URL).flush([]);
  });

  afterEach(() => httpMock.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('getOperationTypeListApi alimente le signal operationTypes', () => {
    const types: OperationType[] = [{ id: 1, name: 'Carte bancaire' }];

    service.getOperationTypeListApi();
    httpMock.expectOne(AppSettings.OPERATIONSTYPES_URL).flush(types);

    expect(service.operationTypes()).toEqual(types);
  });
});
