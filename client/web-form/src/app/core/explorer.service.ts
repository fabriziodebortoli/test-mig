import { EventDataService } from './eventdata.service';
import { DocumentService } from './document.service';
import { Logger } from '@taskbuilder/core';
import { Injectable } from '@angular/core';


@Injectable()
export class ExplorerService extends DocumentService {

    constructor(logger: Logger, eventData: EventDataService) {
        super(logger, eventData);
    }

    setSelectedApplication(application) {

    }
}
