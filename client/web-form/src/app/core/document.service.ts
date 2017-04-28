import { Injectable } from '@angular/core';

import { ViewModeType } from '../shared/models/view-mode-type.model';

import { Logger } from './logger.service';

import { EventDataService } from './eventdata.service';

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

        return ViewModeType.R;

        // let viewModeType = ViewModeType.D;

        // if (this.eventData.model && this.eventData.model.viewModeType) {
        //     viewModeType = this.eventData.model.viewModeType;
        // }
        // return viewModeType;
    }

    close()
    {
        
    }
}
