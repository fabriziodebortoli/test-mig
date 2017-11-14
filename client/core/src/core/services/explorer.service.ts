import { TbComponentServiceParams } from './tbcomponent.service.params';
import { Injectable } from '@angular/core';
import { EventDataService } from './eventdata.service';
import { DocumentService } from './document.service';

@Injectable()
export class ExplorerService extends DocumentService {

    constructor(params: TbComponentServiceParams, eventData: EventDataService) {
        super(params, eventData);
    }

    setSelectedApplication(application) {

    }
}
