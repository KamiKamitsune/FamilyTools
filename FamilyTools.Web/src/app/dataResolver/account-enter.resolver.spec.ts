import { TestBed } from '@angular/core/testing';
import { ResolveFn } from '@angular/router';

import { accountEnterResolver } from './account-enter.resolver';

describe('accountEnterResolver', () => {
  const executeResolver: ResolveFn<boolean> = (...resolverParameters) => 
      TestBed.runInInjectionContext(() => accountEnterResolver(...resolverParameters));

  beforeEach(() => {
    TestBed.configureTestingModule({});
  });

  it('should be created', () => {
    expect(executeResolver).toBeTruthy();
  });
});
