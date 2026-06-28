import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { AccountTagService } from './account-tag.service';
import { AppSettings } from '@core/config/app.constants';
import { AccountTag } from '@easycompta/models/account-tag';

describe('AccountTagService', () => {
  let service: AccountTagService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(AccountTagService);
    httpMock = TestBed.inject(HttpTestingController);

    // Chargement initial déclenché par le constructeur.
    httpMock.expectOne(AppSettings.TAG_URL).flush([]);
  });

  afterEach(() => httpMock.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('getTagList alimente le signal allTag', () => {
    const tags: AccountTag[] = [{ id: 1, name: 'Courses', color: '#FF0000' }];

    service.getTagList();
    httpMock.expectOne(AppSettings.TAG_URL).flush(tags);

    expect(service.allTag()).toEqual(tags);
  });

  it('create poste le tag puis recharge la liste', () => {
    const tag: AccountTag = { name: 'Loisirs', color: '#00FF00' };

    service.create(tag);

    const post = httpMock.expectOne(`${AppSettings.TAG_URL}create`);
    expect(post.request.method).toBe('POST');
    post.flush({ id: 2, ...tag });

    httpMock.expectOne(AppSettings.TAG_URL).flush([{ id: 2, ...tag }]);
    expect(service.allTag().length).toBe(1);
  });

  it('delete supprime puis recharge la liste', () => {
    service.delete(5);

    const del = httpMock.expectOne(`${AppSettings.TAG_URL}delete/5`);
    expect(del.request.method).toBe('DELETE');
    del.flush(true);

    httpMock.expectOne(AppSettings.TAG_URL).flush([]);
    expect(service.allTag()).toEqual([]);
  });
});
