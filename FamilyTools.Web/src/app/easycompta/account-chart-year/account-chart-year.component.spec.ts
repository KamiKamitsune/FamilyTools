import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';

import { AccountChartYearComponent } from './account-chart-year.component';
import { AppSettings } from '@core/config/app.constants';

describe('AccountChartYearComponent', () => {
  let component: AccountChartYearComponent;
  let fixture: ComponentFixture<AccountChartYearComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AccountChartYearComponent],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(AccountChartYearComponent);
    httpMock = TestBed.inject(HttpTestingController);

    // Tags et membres chargés à la construction des services injectés.
    httpMock.expectOne(AppSettings.TAG_URL).flush([]);
    httpMock.expectOne(`${AppSettings.USER_URL}`).flush([]);

    component = fixture.componentInstance;
    fixture.detectChanges(); // ngOnInit -> chargement des mois (pour en déduire les années)

    httpMock.expectOne(`${AppSettings.PAGE_URL}getallmonth`).flush([]);
  });

  afterEach(() => httpMock.verify());

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
