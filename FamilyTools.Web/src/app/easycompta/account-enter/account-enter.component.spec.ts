import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { ActivatedRoute, provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { AccountEnterComponent } from './account-enter.component';
import { AppSettings } from '@core/config/app.constants';
import { AccountEnter } from '@easycompta/models/account-enter';

describe('AccountEnterComponent', () => {
  let component: AccountEnterComponent;
  let fixture: ComponentFixture<AccountEnterComponent>;
  let httpMock: HttpTestingController;

  // L'écriture est fournie par le resolver via `route.data` (lecture synchrone).
  const enter = {
    id: 1,
    name: 'Courses',
    date: '2026-06-01',
    isDisabled: false,
    tag: { id: 1, name: 'Courses', color: '#FF0000' },
    operationType: { id: 1, name: 'CB' },
    totalValue: 10,
    lines: [],
  } as unknown as AccountEnter;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AccountEnterComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: ActivatedRoute, useValue: { data: of({ enter }) } },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AccountEnterComponent);
    httpMock = TestBed.inject(HttpTestingController);

    // Le formulaire enfant charge membres, tags et types d'opération.
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

  it('expose l’écriture résolue', () => {
    expect(component.enter().id).toBe(1);
  });
});
