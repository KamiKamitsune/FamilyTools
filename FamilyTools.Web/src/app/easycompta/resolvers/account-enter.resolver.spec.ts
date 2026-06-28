import { TestBed } from '@angular/core/testing';
import { ActivatedRouteSnapshot, convertToParamMap, RouterStateSnapshot } from '@angular/router';
import { Observable, of } from 'rxjs';

import { accountEnterResolver } from './account-enter.resolver';
import { AccountEnterService } from '@easycompta/data/account-enter.service';
import { AccountEnter } from '@easycompta/models/account-enter';

describe('accountEnterResolver', () => {
  let serviceStub: { get: jasmine.Spy };

  beforeEach(() => {
    serviceStub = { get: jasmine.createSpy('get') };
    TestBed.configureTestingModule({
      providers: [{ provide: AccountEnterService, useValue: serviceStub }],
    });
  });

  it("résout l'écriture en parsant l'id de la route", () => {
    const enter = { id: 42 } as AccountEnter;
    serviceStub.get.and.returnValue(of(enter));

    const route = { paramMap: convertToParamMap({ id: '42' }) } as unknown as ActivatedRouteSnapshot;

    const result = TestBed.runInInjectionContext(() =>
      accountEnterResolver(route, {} as RouterStateSnapshot),
    ) as Observable<AccountEnter>;

    expect(serviceStub.get).toHaveBeenCalledWith(42);
    result.subscribe(value => expect(value).toBe(enter));
  });
});
