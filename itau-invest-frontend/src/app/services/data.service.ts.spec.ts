import { TestBed } from '@angular/core/testing';

import { DataServiceTs } from './data.service.ts';

describe('DataServiceTs', () => {
  let service: DataServiceTs;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DataServiceTs);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });
});
