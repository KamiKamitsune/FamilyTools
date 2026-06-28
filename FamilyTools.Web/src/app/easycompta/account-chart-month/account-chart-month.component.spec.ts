import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';

import { AccountChartMonthComponent } from './account-chart-month.component';
import { AppSettings } from '@core/config/app.constants';

describe('AccountChartMonthComponent', () => {
  let component: AccountChartMonthComponent;
  let fixture: ComponentFixture<AccountChartMonthComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AccountChartMonthComponent],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(AccountChartMonthComponent);
    httpMock = TestBed.inject(HttpTestingController);

    // Tags et membres chargés à la construction des services injectés.
    httpMock.expectOne(AppSettings.TAG_URL).flush([]);
    httpMock.expectOne(`${AppSettings.USER_URL}`).flush([]);

    component = fixture.componentInstance;
    fixture.detectChanges(); // ngOnInit -> chargement des mois

    httpMock.expectOne(`${AppSettings.PAGE_URL}getallmonth`).flush([]);
  });

  afterEach(() => httpMock.verify());

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
