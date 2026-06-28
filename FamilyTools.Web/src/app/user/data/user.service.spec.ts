import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { UserService } from './user.service';
import { User } from '@shared/models/user';
import { AppSettings } from '@core/config/app.constants';

describe('UserService', () => {
  let service: UserService;
  let httpMock: HttpTestingController;

  const listUrl = `${AppSettings.USER_URL}`;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(UserService);
    httpMock = TestBed.inject(HttpTestingController);

    // Le constructeur déclenche un premier chargement de la liste.
    httpMock.expectOne(listUrl).flush([]);
  });

  afterEach(() => {
    httpMock.verify();
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('createUserApi envoie un POST puis rafraîchit la liste', () => {
    const newUser: User = { firstName: 'Jane', lastName: 'Doe', userName: 'jdoe' };
    const refreshed: User[] = [{ id: 1, firstName: 'Jane', lastName: 'Doe', userName: 'jdoe' }];

    service.createUserApi(newUser);

    const post = httpMock.expectOne(`${AppSettings.USER_URL}create`);
    expect(post.request.method).toBe('POST');
    expect(post.request.body).toEqual(newUser);
    post.flush(newUser);

    const refresh = httpMock.expectOne(listUrl);
    expect(refresh.request.method).toBe('GET');
    refresh.flush(refreshed);

    expect(service.users()).toEqual(refreshed);
  });

  it('deleteUserApi envoie un DELETE puis rafraîchit la liste', () => {
    service.deleteUserApi(5);

    const del = httpMock.expectOne(`${AppSettings.USER_URL}delete/5`);
    expect(del.request.method).toBe('DELETE');
    del.flush(true);

    const refresh = httpMock.expectOne(listUrl);
    expect(refresh.request.method).toBe('GET');
    refresh.flush([]);

    expect(service.users()).toEqual([]);
  });

  it('updateUserApi envoie un PUT puis rafraîchit la liste', () => {
    const user: User = { id: 7, firstName: 'John', lastName: 'Roe', userName: 'jroe' };
    const refreshed: User[] = [user];

    service.updateUserApi(user);

    const put = httpMock.expectOne(`${AppSettings.USER_URL}edit`);
    expect(put.request.method).toBe('PUT');
    expect(put.request.body).toEqual(user);
    put.flush(user);

    const refresh = httpMock.expectOne(listUrl);
    expect(refresh.request.method).toBe('GET');
    refresh.flush(refreshed);

    expect(service.users()).toEqual(refreshed);
  });
});
