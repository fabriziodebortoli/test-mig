import { EventDataService } from './eventdata.service';
import { DocumentService } from './document.service';
import { Logger } from 'libclient';
import { Injectable } from '@angular/core';


@Injectable()
export class DataService extends DocumentService{

  constructor(logger: Logger, eventData: EventDataService) {
        super(logger, eventData);
  }
}
