import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { StatisticsService } from './statistics.service';
import { AppSettings } from '@core/config/app.constants';

describe('StatisticsService', () => {
  let service: StatisticsService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(StatisticsService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('expensesByTagForMonth interroge ExpensesByTagForAMonth/{mois}/{année}', () => {
    service.expensesByTagForMonth(6, 2026).subscribe();
    const req = httpMock.expectOne(`${AppSettings.ENTER_URL}ExpensesByTagForAMonth/6/2026`);
    expect(req.request.method).toBe('GET');
    req.flush({ 1: 10 });
  });

  it('expensesByUserForYear interroge ExpensesByUserForAYear/{année}', () => {
    service.expensesByUserForYear(2026).subscribe();
    const req = httpMock.expectOne(`${AppSettings.LINES_URL}ExpensesByUserForAYear/2026`);
    expect(req.request.method).toBe('GET');
    req.flush({ 1: 20 });
  });

  it('expensesByTagForYear interroge ExpensesByTagForAYear/{année}', () => {
    service.expensesByTagForYear(2026).subscribe();
    const req = httpMock.expectOne(`${AppSettings.ENTER_URL}ExpensesByTagForAYear/2026`);
    expect(req.request.method).toBe('GET');
    req.flush({ 1: 40 });
  });

  it('expensesByMonthForYear interroge ExpensesByMonthForAYear/{année}', () => {
    service.expensesByMonthForYear(2026).subscribe();
    const req = httpMock.expectOne(`${AppSettings.ENTER_URL}ExpensesByMonthForAYear/2026`);
    expect(req.request.method).toBe('GET');
    req.flush({ 6: 30 });
  });

  it('expensesByUserAndTagForMonth interroge ExpensesByUserAndTagForAMonth/{mois}/{année}', () => {
    service.expensesByUserAndTagForMonth(6, 2026).subscribe();
    const req = httpMock.expectOne(`${AppSettings.LINES_URL}ExpensesByUserAndTagForAMonth/6/2026`);
    expect(req.request.method).toBe('GET');
    req.flush([{ userId: 1, tagId: 1, amount: 10 }]);
  });
});
