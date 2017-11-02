import { SettingsService } from './../core/services/settings.service';
import { InfoService } from './../core/services/info.service';
import { ComponentService } from './../core/services/component.service';
import { EventDataService } from './../core/services/eventdata.service';

import { Logger } from './../core/services/logger.service';
import { DocumentService } from './../core/services/document.service';
import { Injectable } from '@angular/core';
import { Observable } from '../rxjs.imports';

@Injectable()
export class SettingsPageService extends DocumentService {

    constructor(
        public logger: Logger,
        public eventData: EventDataService,
        public componentService: ComponentService,
        public infoService: InfoService,
        public settingsService: SettingsService
    ) {
        super(logger, eventData, infoService);
    }

    close() {
        super.close();
        this.componentService.removeComponentById(this.mainCmpId);
        this.settingsService.settingsPageOpenedEvent.emit(false);
    }
}
