import { Injectable } from '@angular/core';

import { DocumentService, TbComponentServiceParams, EventDataService, SettingsService, ComponentService } from '@taskbuilder/core';

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
