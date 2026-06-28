import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';

import { AccountTagComponent } from './account-tag.component';
import { AppSettings } from '@core/config/app.constants';

describe('AccountTagComponent', () => {
  let component: AccountTagComponent;
  let fixture: ComponentFixture<AccountTagComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AccountTagComponent],
      providers: [provideHttpClient(), provideHttpClientTesting(), provideRouter([])],
    }).compileComponents();

    fixture = TestBed.createComponent(AccountTagComponent);
    httpMock = TestBed.inject(HttpTestingController);

    // Le AccountTagService charge la liste des tags dès sa construction.
    httpMock.expectOne(AppSettings.TAG_URL).flush([]);

    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => httpMock.verify());

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
