import { EventDataService } from 'tb-core';
import { Logger } from 'libclient';
import { DocumentService } from './../../../core/document.service';
import { Injectable } from '@angular/core';


@Injectable()
export class DataServiceService extends DocumentService{

  constructor(logger: Logger, eventData: EventDataService) {
        super(logger, eventData);
  }
}
