import { TbComponentServiceParams } from './../core/services/tbcomponent.service.params';
import { SettingsService } from './../core/services/settings.service';
import { ComponentService } from './../core/services/component.service';
import { EventDataService } from './../core/services/eventdata.service';
import { DocumentService } from './../core/services/document.service';
import { Injectable } from '@angular/core';
import { Observable } from '../rxjs.imports';

@Injectable()
export class SettingsPageService extends DocumentService {

    constructor(
        params: TbComponentServiceParams,
        eventData: EventDataService,
        public settingsService: SettingsService,
        public componentService: ComponentService
       
    ) {
        super(params, eventData);
    }

    close() {
        super.close();
        this.componentService.removeComponentById(this.mainCmpId);
        this.settingsService.settingsPageOpenedEvent.emit(false);
    }
}
