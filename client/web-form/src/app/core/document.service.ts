import { EventDataService } from './eventdata.service';
import { Injectable } from '@angular/core';

import { WebSocketService } from './websocket.service';

import { Logger } from 'libclient';

@Injectable()
export class DocumentService {
    mainCmpId: string;
    constructor(protected logger: Logger, protected eventData: EventDataService) {
    }
    init(cmpId: string)
    {
        this.mainCmpId = cmpId;
    }
    dispose() {
        delete this.mainCmpId;
    }
    getTitle() {
        let title = 'Untitled';
        if (this.eventData.model && this.eventData.model.Title && this.eventData.model.Title.value)
            title = this.eventData.model.Title.value;
        return title;
    }
}
