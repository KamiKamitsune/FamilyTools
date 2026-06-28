import { ComponentFixture, TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { UserDetailComponent } from './user-detail.component';
import { AppSettings } from '@core/config/app.constants';
import { User } from '@shared/models/user';

describe('UserDetailComponent', () => {
  let component: UserDetailComponent;
  let fixture: ComponentFixture<UserDetailComponent>;
  let httpMock: HttpTestingController;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [UserDetailComponent],
      providers: [provideHttpClient(), provideHttpClientTesting()],
    }).compileComponents();

    fixture = TestBed.createComponent(UserDetailComponent);
    httpMock = TestBed.inject(HttpTestingController);
    httpMock.expectOne(`${AppSettings.USER_URL}`).flush([]);

    // L'input `user` est requis : il faut le fournir avant le premier rendu.
    const user: User = { id: 1, firstName: 'Jean', lastName: 'Dupont', userName: 'jdupont' };
    fixture.componentRef.setInput('user', user);

    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  afterEach(() => httpMock.verify());

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
