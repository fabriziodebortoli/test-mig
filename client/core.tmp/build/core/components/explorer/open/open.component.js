import { Component } from '@angular/core';
import { URLSearchParams, Http } from '@angular/http';
import { Observable } from 'rxjs';
import { PanelBarExpandMode } from '@progress/kendo-angular-layout';
import { UtilsService, EventDataService, ExplorerService } from './../../..//index';
import { ImageService } from '../../../../menu/services/image.service';
import { MenuService } from '../../../../menu/services/menu.service';
import { DocumentComponent } from '../../document.component';
export class OpenComponent extends DocumentComponent {
    /**
     * @param {?} explorerService
     * @param {?} eventData
     * @param {?} imageService
     * @param {?} menuService
     * @param {?} utilsService
     * @param {?} http
     */
    constructor(explorerService, eventData, imageService, menuService, utilsService, http) {
        super(explorerService, eventData);
        this.explorerService = explorerService;
        this.imageService = imageService;
        this.menuService = menuService;
        this.utilsService = utilsService;
        this.http = http;
        this.kendoPanelBarExpandMode = PanelBarExpandMode.Multiple;
    }
    /**
     * @return {?}
     */
    ngOnDestroy() {
        this.applicationsSubscription.unsubscribe();
    }
    /**
     * @return {?}
     */
    ngOnInit() {
        this.applicationsSubscription = this.getApplications().subscribe(result => {
            this.applications = result.Applications.Application;
        });
    }
    /**
     * @return {?}
     */
    getApplications() {
        return this.http.get('http://localhost:5000/explorer-open/get-applications/', { withCredentials: true }).map((res) => {
            return res.json();
        }).catch(this.handleError);
    }
    /**
     * @param {?} error
     * @return {?}
     */
    handleError(error) {
        // In a real world app, we might use a remote logging infrastructure
        // We'd also dig deeper into the error to get a better message
        let /** @type {?} */ errMsg = (error.message) ? error.message :
            error.status ? `${error.status} - ${error.statusText}` : 'Server error';
        console.error(errMsg);
        return Observable.throw(errMsg);
    }
    /**
     * @param {?} application
     * @return {?}
     */
    selecteApplication(application) {
        this.folderSubscription = this.callGetFolder(application).subscribe(result => {
            this.folders = result.Folders.Folder;
        });
    }
    /**
     * @param {?} application
     * @return {?}
     */
    callGetFolder(application) {
        let /** @type {?} */ params = new URLSearchParams();
        params.set('applicationPath', application);
        return this.http.get('http://localhost:5000/explorer-open/get-folders/' + "kk", { search: params }).map((res) => {
            return res.json();
        }).catch(this.handleError);
    }
    /**
     * @param {?} folder
     * @return {?}
     */
    selecteFolder(folder) {
        console.log('sono nella selectFolder');
        this.filesSubscription = this.callGetFolderFiles(folder).subscribe(result => {
            console.log(result);
            this.files = result.Files.File;
            console.log(this.files);
        });
    }
    /**
     * @param {?} folder
     * @return {?}
     */
    callGetFolderFiles(folder) {
        let /** @type {?} */ params = new URLSearchParams();
        params.set('folderPath', folder);
        return this.http.get('http://localhost:5000/explorer-open/get-folderFiles/' + "kk", { search: params }).map((res) => {
            return res.json();
        }).catch(this.handleError);
    }
}
OpenComponent.decorators = [
    { type: Component, args: [{
                selector: 'tb-open',
                template: "<!--<div class=\"divSinistra\"> <div class=\"panelbar-wrapper\"> <kendo-panelbar> <kendo-panelbar-item *ngFor=\"let application of applications\" (click)=selecteApplication(application.path) [title]=\"application.name\"> <template kendoPanelBarContent> <div *ngFor=\"let folder of folders\" class=\"appTitle\" (click)=selecteFolder(folder.path)> <div class=\"panel\">{{folder.name}}</div> </div> </template> </kendo-panelbar-item> </kendo-panelbar> </div> </div>  <kendo-panelbar> <kendo-panelbar-item *ngFor=\"let application of applications\" (click)=selecteApplication(application.path) [title]=\"application.name\"> <template kendoPanelBarContent> <div *ngFor=\"let folder of folders\" class=\"appTitle\" (click)=selecteFolder(folder.path)> <div class=\"panel\">{{folder.name}}</div> </div> </template> //  <div (click)=selecteFolder(folder.path)>{{folder.name}}</div> </kendo-panelbar-item> </kendo-panelbar>--> <div style=\"float:left; display:block; width:20%; height:150px; background-color:#F00;\"> <div class=\"panelbar-wrapper\"> <kendo-panelbar> <kendo-panelbar-item *ngFor=\"let application of applications\" (click)=selecteApplication(application.path) [title]=\"application.name\"> <ng-template kendoPanelBarContent> <div *ngFor=\"let folder of folders\"> <kendo-panelbar-item [title]=\"folder.name\" (click)=selecteFolder(folder.path)></kendo-panelbar-item> </div> </ng-template> </kendo-panelbar-item> </kendo-panelbar> </div> </div> <div style=\"float:left; display:block; width:70%; background-color:#FF0;\"> <kendo-grid [data]=\"files\" [selectable]=\"true\"> <kendo-grid-column width=\"20\"> <ng-template kendoGridCellTemplate let-dataItem> <input type=\"checkbox\" /> </ng-template> </kendo-grid-column> <kendo-grid-column field=\"name\" title=\"Name\" width=\"120\"></kendo-grid-column> <kendo-grid-column field=\"path\" title=\"Path\" width=\"470\"></kendo-grid-column> </kendo-grid> </div> <div style=\"clear:both;\"></div>",
                styles: [".appTitle { height: 30px; } .bookmarks { margin-top: 10px; font-size: small; } /* Style the buttons that are used to open and close the accordion panel */ button.accordion { background-color: #eee; color: #444; cursor: pointer; padding: 18px; width: 30%; text-align: left; border: none; outline: none; } /* Add a background color to the button if it is clicked on (add the .active class with JS), and when you move the mouse over it (hover) */ /* Style the accordion panel. Note: hidden by default */ div.panel { padding: 0 18px; background-color: white; } .divSinistra { width: 40%; float: left; display: block; } .divDestra { float: left; display: block; padding: 70; background-color: #FF0; } .panelbar-wrapper { max-width: 400px; margin: 0 auto; }"],
                providers: [EventDataService]
            },] },
];
/**
 * @nocollapse
 */
OpenComponent.ctorParameters = () => [
    { type: ExplorerService, },
    { type: EventDataService, },
    { type: ImageService, },
    { type: MenuService, },
    { type: UtilsService, },
    { type: Http, },
];
function OpenComponent_tsickle_Closure_declarations() {
    /** @type {?} */
    OpenComponent.decorators;
    /**
     * @nocollapse
     * @type {?}
     */
    OpenComponent.ctorParameters;
    /** @type {?} */
    OpenComponent.prototype.applications;
    /** @type {?} */
    OpenComponent.prototype.folders;
    /** @type {?} */
    OpenComponent.prototype.files;
    /** @type {?} */
    OpenComponent.prototype.kendoPanelBarExpandMode;
    /** @type {?} */
    OpenComponent.prototype.applicationsSubscription;
    /** @type {?} */
    OpenComponent.prototype.folderSubscription;
    /** @type {?} */
    OpenComponent.prototype.filesSubscription;
    /** @type {?} */
    OpenComponent.prototype.explorerService;
    /** @type {?} */
    OpenComponent.prototype.imageService;
    /** @type {?} */
    OpenComponent.prototype.menuService;
    /** @type {?} */
    OpenComponent.prototype.utilsService;
    /** @type {?} */
    OpenComponent.prototype.http;
}
