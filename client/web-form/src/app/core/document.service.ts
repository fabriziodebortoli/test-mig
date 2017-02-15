import { EventService } from './event.service';
import { Injectable } from '@angular/core';

import { WebSocketService } from './websocket.service';

import { Logger } from 'libclient';

@Injectable()
export class DocumentService {
    model: any;
    mainCmpId: string;
    constructor(protected logger: Logger, protected eventService: EventService) {
    }
    init(cmpId: string)
    {
        this.mainCmpId = cmpId;
    }
    dispose() {
        delete this.model;
        delete this.mainCmpId;
    }
    getTitle() {
        let title = 'Untitled';
        if (this.model && this.model.Title && this.model.Title.value)
            title = this.model.Title.value;
        return title;
    }
}
