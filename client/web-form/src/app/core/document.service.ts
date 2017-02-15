import { EventService } from 'tb-core';
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
}
