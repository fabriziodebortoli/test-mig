import { Injectable } from '@angular/core';

@Injectable()
export class ExplorerService 
{

  public selectedApplication: any;
  public selectedFolder:  any;
  public applicationMenu: any;
  
  constructor() { }


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
