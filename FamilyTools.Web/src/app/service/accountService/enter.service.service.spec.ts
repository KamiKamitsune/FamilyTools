import { TestBed } from '@angular/core/testing';

import { EnterService } from './enter.service.service';

describe('EnterServiceService', () => {
  let service: EnterService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(EnterService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
