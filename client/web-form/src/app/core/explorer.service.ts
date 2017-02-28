import { ViewModeType } from 'tb-shared';
import { DocumentService } from './document.service';
import { EventDataService } from './eventdata.service';
import { Logger } from 'libclient';
import { Injectable } from '@angular/core';

@Injectable()
export class ExplorerService extends DocumentService {

    public selectedApplication: any;
    public selectedFolder: any;
    public applicationMenu: any;

    constructor(logger: Logger, eventData: EventDataService) {
        super(logger, eventData);
    }

    getTitle() {
        return "Explorer Demo";
    }

    getViewModeType() {
        return ViewModeType.M;
    }

    //---------------------------------------------------------------------------------------------
    setSelectedApplication(application) {
        if (this.selectedApplication != undefined && this.selectedApplication.title == application.title)
            return;

        if (this.selectedApplication != undefined)
            this.selectedApplication.isSelected = false;

        this.selectedApplication = application;
        this.selectedApplication.isSelected = true;

        //      this.settingsService.lastApplicationName = application.name;
        //      this.settingsService.setPreference('LastApplicationName', encodeURIComponent(this.settingsService.lastApplicationName), undefined);

        //     var tempGroupArray = this.utilsService.toArray(this.selectedApplication.Group);
        //     if (tempGroupArray[0] != undefined)
        //         this.selectedFolder(tempGroupArray[0]);
    }

}
