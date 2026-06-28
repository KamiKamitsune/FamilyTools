import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { AccountEnterService } from './account-enter.service';
import { AppSettings } from '@core/config/app.constants';
import { AccountEnterDto } from '@easycompta/models/account-enter';

describe('AccountEnterService', () => {
  let service: AccountEnterService;
  let httpMock: HttpTestingController;

  const dto: AccountEnterDto = {
    name: 'Courses',
    date: '2026-06-12',
    isDisabled: false,
    tagId: 1,
    operationTypeId: 1,
    totalValue: 30,
    lines: [],
  };

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(AccountEnterService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('get interroge index/{id}', () => {
    service.get(7).subscribe();

    const req = httpMock.expectOne(`${AppSettings.ENTER_URL}index/7`);
    expect(req.request.method).toBe('GET');
    req.flush({ id: 7 });
  });

  it('create envoie un POST sur create avec le DTO', () => {
    service.create(dto).subscribe();

    const req = httpMock.expectOne(`${AppSettings.ENTER_URL}create`);
    expect(req.request.method).toBe('POST');
    expect(req.request.body).toEqual(dto);
    req.flush({ id: 1 });
  });

  it('update envoie un PUT sur edit avec le DTO', () => {
    service.update(dto).subscribe();

    const req = httpMock.expectOne(`${AppSettings.ENTER_URL}edit`);
    expect(req.request.method).toBe('PUT');
    expect(req.request.body).toEqual(dto);
    req.flush({ id: 1 });
  });

  it('delete envoie un DELETE sur delete/{id}', () => {
    service.delete(3).subscribe();

    const req = httpMock.expectOne(`${AppSettings.ENTER_URL}delete/3`);
    expect(req.request.method).toBe('DELETE');
    req.flush(true);
  });
});
