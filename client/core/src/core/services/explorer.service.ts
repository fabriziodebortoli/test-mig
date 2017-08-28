import { InfoService } from './info.service';
import { Injectable } from '@angular/core';

import { EventDataService } from './eventdata.service';
import { DocumentService } from './document.service';
import { Logger } from './logger.service';

@Injectable()
export class ExplorerService extends DocumentService {

    constructor(logger: Logger, eventData: EventDataService, infoService: InfoService) {
        super(logger, eventData, infoService);
    }

    setSelectedApplication(application) {

    }
}
