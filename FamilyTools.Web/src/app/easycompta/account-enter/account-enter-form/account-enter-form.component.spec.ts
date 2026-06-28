import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';

import { AccountEnterFormComponent } from './account-enter-form.component';
import { AppSettings } from '@core/config/app.constants';

describe('AccountEnterFormComponent', () => {
  let component: AccountEnterFormComponent;
  let fixture: ComponentFixture<AccountEnterFormComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AccountEnterFormComponent],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(AccountEnterFormComponent);
    httpMock = TestBed.inject(HttpTestingController);

    // Membres, tags et types d'opération sont chargés à la construction des services injectés.
    httpMock.expectOne(`${AppSettings.USER_URL}`).flush([]);
    httpMock.expectOne(AppSettings.TAG_URL).flush([]);
    httpMock.expectOne(AppSettings.OPERATIONSTYPES_URL).flush([]);

    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => httpMock.verify());

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
