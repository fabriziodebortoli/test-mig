import { DocumentServiceParams } from './document.service.params';
import { } from './tbcomponent.service.params';
import { Injectable } from '@angular/core';
import { EventDataService } from './eventdata.service';
import { DocumentService } from './document.service';

@Injectable()
export class ExplorerService extends DocumentService {

    constructor(params: DocumentServiceParams) {
        super(params);
    }

    setSelectedApplication(application) {

    }
}
