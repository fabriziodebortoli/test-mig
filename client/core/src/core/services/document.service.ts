import { InfoService } from './info.service';
import { TbComponentService } from './tbcomponent.service';
import { Injectable } from '@angular/core';

import { ViewModeType } from '../../shared/models';

import { Logger } from './logger.service';

import { EventDataService } from './eventdata.service';

@Injectable()
export class DocumentService extends TbComponentService {

    mainCmpId: string;
    constructor(public logger: Logger, public eventData: EventDataService, public infoService: InfoService) {
        super(logger, infoService);
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

    getHeader() {
        let header = this.getTitle();

        if (this.eventData.model && this.eventData.model.HeaderStripTitle && this.eventData.model.HeaderStripTitle.value)
            header = this.eventData.model.HeaderStripTitle.value;

        return header;
    }

    getViewModeType() {

        return ViewModeType.R;

        // let viewModeType = ViewModeType.D;

        // if (this.eventData.model && this.eventData.model.viewModeType) {
        //     viewModeType = this.eventData.model.viewModeType;
        // }
        // return viewModeType;
    }

    close() {

    }


}
