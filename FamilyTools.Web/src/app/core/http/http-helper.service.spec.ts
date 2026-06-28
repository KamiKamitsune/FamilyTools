import { TestBed } from '@angular/core/testing';
import { provideHttpClient } from '@angular/common/http';
import { HttpTestingController, provideHttpClientTesting } from '@angular/common/http/testing';

import { HttpHelperService } from './http-helper.service';

describe('HttpHelperService', () => {
  let service: HttpHelperService;
  let httpMock: HttpTestingController;

  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [provideHttpClient(), provideHttpClientTesting()],
    });
    service = TestBed.inject(HttpHelperService);
    httpMock = TestBed.inject(HttpTestingController);
  });

  afterEach(() => httpMock.verify());

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('get accole le paramètre à l’URL', () => {
    service.get<{ id: number }>('api/User/index', 5).subscribe();

    const req = httpMock.expectOne('api/User/index/5');
    expect(req.request.method).toBe('GET');
    req.flush({ id: 5 });
  });

  it('get sans paramètre n’ajoute pas de slash', () => {
    service.get('api/User/').subscribe();

    const req = httpMock.expectOne('api/User/');
    expect(req.request.method).toBe('GET');
    req.flush([{}]);
  });

  it('delete cible l’URL avec id', () => {
    service.delete('api/User/delete', 9).subscribe();

    const req = httpMock.expectOne('api/User/delete/9');
    expect(req.request.method).toBe('DELETE');
    req.flush(true);
  });
});
