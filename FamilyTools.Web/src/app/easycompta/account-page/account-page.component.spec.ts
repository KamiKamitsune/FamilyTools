import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';
import { provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { AccountPageComponent } from './account-page.component';
import { ImportEventsService } from '@easycompta/data/import-events.service';
import { AppSettings } from '@core/config/app.constants';

describe('AccountPageComponent', () => {
  let component: AccountPageComponent;
  let fixture: ComponentFixture<AccountPageComponent>;
  let httpMock: HttpTestingController;

  // On neutralise la connexion SignalR temps réel (pas de réseau en test).
  const importEventsStub: Partial<ImportEventsService> = {
    connect: () => Promise.resolve(),
    importCompleted$: of(),
  };

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [AccountPageComponent],
      providers: [
        provideHttpClient(),
        provideHttpClientTesting(),
        provideRouter([]),
        { provide: ImportEventsService, useValue: importEventsStub },
      ],
    }).compileComponents();

    fixture = TestBed.createComponent(AccountPageComponent);
    httpMock = TestBed.inject(HttpTestingController);

    // Tags et membres chargés à la construction des services injectés.
    httpMock.expectOne(AppSettings.TAG_URL).flush([]);
    httpMock.expectOne(`${AppSettings.USER_URL}`).flush([]);

    component = fixture.componentInstance;
    fixture.detectChanges(); // déclenche ngOnInit -> chargement des mois

    httpMock.expectOne(`${AppSettings.PAGE_URL}getallmonth`).flush([]);
  });

  afterEach(() => httpMock.verify());

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
