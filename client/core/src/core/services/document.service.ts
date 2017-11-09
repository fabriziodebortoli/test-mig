import { DocumentServiceParams } from './document.service.params';
import { Injectable } from '@angular/core';
import { ViewModeType } from '../../shared/models/view-mode-type.model';
import { TbComponentService } from './tbcomponent.service';
import { EventDataService } from './eventdata.service';

@Injectable()
export class DocumentService extends TbComponentService {

    mainCmpId: string;

    public eventData: EventDataService;
    constructor(params: DocumentServiceParams) {
        super(params);
        this.eventData = params.eventData;
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
