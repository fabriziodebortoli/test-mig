import { Logger } from 'libclient';
import { DocumentService, EventDataService } from 'tb-core';
import { Injectable } from '@angular/core';

@Injectable()
export class ReportingStudioService extends DocumentService {
    
    constructor(logger: Logger, eventData: EventDataService) {
        super(logger, eventData);
    }
}
