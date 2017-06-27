import { Injectable } from '@angular/core';

import { EventDataService } from './eventdata.service';
import { DocumentService } from './document.service';
import { Logger } from './logger.service';

@Injectable()
export class ExplorerService extends DocumentService {

    constructor(logger: Logger, eventData: EventDataService) {
        super(logger, eventData);
    }

    setSelectedApplication(application) {

    }
}
