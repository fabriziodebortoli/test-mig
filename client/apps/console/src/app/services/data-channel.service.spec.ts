import { TestBed, inject } from '@angular/core/testing';

import { DataChannelService } from './data-channel.service';

describe('DataChannelService', () => {
  beforeEach(() => {
    TestBed.configureTestingModule({
      providers: [DataChannelService]
    });
  });

  it('should ...', inject([DataChannelService], (service: DataChannelService) => {
    expect(service).toBeTruthy();
  }));
});
