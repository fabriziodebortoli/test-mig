import { DocumentServiceParams } from './../core/services/document.service.params';
import { ComponentService } from './../core/services/component.service';
import { EventDataService } from './../core/services/eventdata.service';

import { Logger } from './../core/services/logger.service';
import { DocumentService } from './../core/services/document.service';
import { Injectable } from '@angular/core';
import { Observable } from '../rxjs.imports';

@Injectable()
export class TestService extends DocumentService {

    constructor(
        params: DocumentServiceParams,
        public componentService: ComponentService) {
        super(params);
    }

    close() {
        super.close();
        this.componentService.removeComponentById(this.mainCmpId);
    }
}
