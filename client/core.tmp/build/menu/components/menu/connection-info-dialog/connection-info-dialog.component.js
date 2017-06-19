import { Component } from '@angular/core';
import { MdDialogRef } from '@angular/material';
import { HttpMenuService } from './../../../services/http-menu.service';
import { LocalizationService } from './../../../services/localization.service';
export class ConnectionInfoDialogComponent {
    /**
     * @param {?} dialogRef
     * @param {?} httpMenuService
     * @param {?} localizationService
     */
    constructor(dialogRef, httpMenuService, localizationService) {
        this.dialogRef = dialogRef;
        this.httpMenuService = httpMenuService;
        this.localizationService = localizationService;
    }
    /**
     * @return {?}
     */
    ngOnInit() {
        this.connectionInfoSub = this.httpMenuService.getConnectionInfo().subscribe(result => {
            this.connectionInfos = result;
            this.showdbsize = this.connectionInfos.showdbsizecontrols == 'Yes';
        });
    }
    /**
     * @return {?}
     */
    ngOnDestroy() {
        this.connectionInfoSub.unsubscribe();
    }
}
ConnectionInfoDialogComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-connection-info-dialog',
                template: " <table> <tr> <td class=\"loginStyleOff pointer backButtonPadding\" ></td> <td> </td> </tr> <!--User--> <tr> <td style=\"text-align:right\"> {{localizationService.localizedElements.CompanyLabel}}: </td> <td><b>{{connectionInfos?.company}}</b></td> </tr> <tr> <td style=\"text-align:right\"> {{localizationService.localizedElements.Username}}: </td> <td><b>{{connectionInfos?.user}}</b></td> </tr> <tr> <td style=\"text-align:right\"> {{localizationService.localizedElements.Administrator}}: </td> <td><b>{{connectionInfos?.admin}}</b></td> </tr> <tr> <td style=\"text-align:right\"> {{localizationService.localizedElements.EasyStudioDeveloper}}: </td> <td><b>{{connectionInfos?.ebdev}}</b></td> </tr> <!--General--> <tr style=\"border-top:solid; border-color:black;border-top-style:solid; border-width:1px\"> <td style=\"text-align:right\"> {{localizationService.localizedElements.Instance}}: </td> <td><b>{{connectionInfos?.installation}}</b></td> </tr> <tr> <td style=\"text-align:right\"> {{localizationService.localizedElements.FileApplicationServer}}: </td> <td><b>{{connectionInfos?.remotefileserver}}</b></td> </tr> <tr> <td style=\"text-align:right\"> {{localizationService.localizedElements.WebApplicationServer}}: </td> <td><b>{{connectionInfos?.remotewebserver}}</b></td> </tr> <tr> <td style=\"text-align:right\"> {{localizationService.localizedElements.AuditingEnabled}}: </td> <td><b>{{connectionInfos?.auditing}}</b></td> </tr> <tr> <td style=\"text-align:right\"> {{localizationService.localizedElements.SecurityEnabled}}: </td> <td><b>connectionInfos?.security}}</b></td> </tr> <!--DATABASE--> <tr style=\"border-top:solid; border-color:black;border-top-style:solid; border-width:1px\"> <td style=\"text-align:right\"> {{localizationService.localizedElements.DBServer}}: </td> <td><b>{{connectionInfos?.dbserver}}</b></td> </tr> <tr> <td style=\"text-align:right\"> {{localizationService.localizedElements.DBName}}: </td> <td><b>{{connectionInfos?.dbname}}</b></td> </tr> <tr> <td style=\"text-align:right\"> {{localizationService.localizedElements.DBUser}}: </td> <td><b>{{connectionInfos?.dbuser}}</b></td> </tr> <!--DATABASE SIZE (if needed)--> <tr [hidden]=\"!showdbsize\"> <td style=\" text-align:right\"> {{localizationService.localizedElements.UsedSpace}}: </td> <td><b>{{connectionInfos?.usedspace}}</b></td> </tr> <tr [hidden]=\"!showdbsize\"> <td style=\"text-align:right\"> {{localizationService.localizedElements.FreeSpace}}: </td> <td><b>{{connectionInfos?.freespace}}</b></td> </tr> </table> <button type=\"button\" (click)=\"dialogRef.close('yes')\">{{localizationService.localizedElements.CloseLabel}}</button> ",
                styles: [""]
            },] },
];
/**
 * @nocollapse
 */
ConnectionInfoDialogComponent.ctorParameters = () => [
    { type: MdDialogRef, },
    { type: HttpMenuService, },
    { type: LocalizationService, },
];
function ConnectionInfoDialogComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    ConnectionInfoDialogComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    ConnectionInfoDialogComponent.ctorParameters;
    /** @type {?} */
    ConnectionInfoDialogComponent.prototype.connectionInfos;
    /** @type {?} */
    ConnectionInfoDialogComponent.prototype.showdbsize;
    /** @type {?} */
    ConnectionInfoDialogComponent.prototype.connectionInfoSub;
    /** @type {?} */
    ConnectionInfoDialogComponent.prototype.dialogRef;
    /** @type {?} */
    ConnectionInfoDialogComponent.prototype.httpMenuService;
    /** @type {?} */
    ConnectionInfoDialogComponent.prototype.localizationService;
}
