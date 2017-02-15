import { EventDataService } from './eventdata.service';
import { Injectable } from '@angular/core';

import { WebSocketService } from './websocket.service';

import { ViewModeType } from '../shared/models/view-mode-type.model';

import { Logger } from 'libclient';

@Injectable()
export class DocumentService {
    mainCmpId: string;
    constructor(protected logger: Logger, protected eventData: EventDataService) {
    }
    init(cmpId: string) {
        this.mainCmpId = cmpId;
    }
    dispose() {
        delete this.mainCmpId;
    }
    getTitle() {
        let title = '...';

        if (this.eventData.model && this.eventData.model.Title && this.eventData.model.Title.value)
            title = this.eventData.model.Title.value;
        return title;
    }

    getViewModeType() {
        let viewModeType = ViewModeType.D;

        if (this.eventData.model && this.eventData.model.viewModeType) {
            viewModeType = this.eventData.model.viewModeType;
        }
        return viewModeType;
    }
}
