import { EasyStudioContextComponent } from './../../shared/components/easystudio-context/easystudio-context.component';
import { SettingsService } from './settings.service';
import { EsCustomizItem } from './../../shared/models/es-customization-item.model';
import { HttpMenuService } from './../../menu/services/http-menu.service';
import { Injectable, EventEmitter } from '@angular/core';
import { Observable } from '../../rxjs.imports';

export class MyObj {
    application: string
    module: string

    constructor(app, mod) {
        this.application = app;
        this.module = mod;
    }
}
@Injectable()
export class EasystudioService {

    public isDesignable: boolean;
    public subscriptions = [];
    public currentModule: string;                      //current module selected by ESContext
    public currentApplication: string;                 //current applic selected by ESContext          
    public modules: any[];                             //list of modules in the file system
    public applications: any[];                      //list of applics in the file system
    public customizations: EsCustomizItem[];    //list of customization in the file system, each knows its owners
    public memoryESContext: { allApplications: MyObj[] };
    public memoryCustsList: { Customizations: EsCustomizItem[] };

    //--------------------------------------------------------------------------------  
    private _defaultApplication:string; //default applic read from preferences
    get defaultApplication():string {
        return this._defaultApplication;
    }
    set defaultApplication(theBar:string) {
        this._defaultApplication = theBar;
    }

    //--------------------------------------------------------------------------------
    private _defaultModule:string;//default module read from preferences
    get defaultModule():string {
        return this._defaultModule;
    }
    set defaultModule(theBar:string) {
        this._defaultModule = theBar;
    }

    //#region both
    //--------------------------------------------------------------------------------
    constructor(public httpMenuService: HttpMenuService,
        public settingService: SettingsService) {
        this.getDefaultContext(false);
    }

    //--------------------------------------------------------------------------------
    dispose() {
        this.subscriptions.forEach(subs => subs.unsubscribe());
    }

    //--------------------------------------------------------------------------------
    public isContextActive(): boolean {
        return this.currentApplication !== undefined && this.currentModule !== undefined;
    }
    //--------------------------------------------------------------------------------
    public getCurrentContext(): any {
        this.subscriptions.push(this.httpMenuService.getCurrentContext().subscribe((result) => {
            let array = this.extractCouple(result);
            if (array !== null && array !== undefined) {
                this.currentApplication = array[0];
                this.currentModule = array[1];
            }
            return result;
        }));
    }
    //--------------------------------------------------------------------------------
    private extractCouple(result: Response): string[] {
        if (result == undefined) return null;
        let res = result["_body"];
        if (res !== "") {
            let array: string[] = res.toString().split(';');
            if (!array || array.length != 2) return null;
            return array;

        }
    }
    //#endregion

    //#region methods for menu items customizations
    //--------------------------------------------------------------------------------
    public initEasyStudioData(object: any) {
        this.subscriptions.push(this.httpMenuService.initEasyStudioData(object).subscribe((result) => {
            this.memoryCustsList = { Customizations: [] };
            this.customizations = [];
            let body = result["_body"]; // if body=="" isDesignable not, if body=="Customizations:{}" isDesignable but empty
            let canHaveCustoms = body != "";
            if (canHaveCustoms) {
                this.memoryCustsList = JSON.parse(body);
                if (this.memoryCustsList != undefined) {
                    this.memoryCustsList.Customizations.forEach(element => {
                        if (this.customizations.find(e => e === element) === undefined)
                            this.customizations.push(element);
                    })
                }
            }
            this.isDesignable = true; //canHaveCustoms; //TODOROBY
        }));
    }

    //--------------------------------------------------------------------------------
    public isEasyStudioDocument(object: any) {
        return true;//TODOROBY
     /*   this.subscriptions.push(this.httpMenuService.isEasyStudioDocument(object).subscribe((result) => {
            return result;
        }));*/
    }

    //--------------------------------------------------------------------------------
    public runEasyStudio(target: any, customizationName: string) {
        this.subscriptions.push(this.httpMenuService.runEasyStudio(target, customizationName).subscribe((result) => { }));
    }

    //--------------------------------------------------------------------------------
    public cloneDocument(object: any, docName: string, docTitle: string): boolean {
        if (docName == undefined || !this.isContextActive())
            return;
        if (docTitle == undefined)
            docTitle = docName;
        this.subscriptions.push(this.httpMenuService.cloneAsEasyStudioDocument(object, docName, docTitle, this)
            .subscribe((result) => {
                return result;
            }));
    }

    //#endregion

    //#region methods for EasyStudio Context
    //--------------------------------------------------------------------------------
    public getModules(): any {
        return this.modules;
    }

    //--------------------------------------------------------------------------------
    public getApplications(): any {
        return this.applications;
    }

    //--------------------------------------------------------------------------------
    public initEasyStudioContext(type) {
        this.subscriptions.push(this.httpMenuService.getEsAppsAndModules(type).subscribe((result) => {
            this.extractNamesAllApps(result);
            return result;
        }));
    }

    //--------------------------------------------------------------------------------
    public canModifyContext(): Observable<any> {
        return this.httpMenuService.canModifyContext().map((res: Response) => res.json());
    }

    //--------------------------------------------------------------------------------
    private extractNamesAllApps(result: Response) {
        if (result == undefined) return;
        this.applications = [];
        this.modules = [];

       // let resultJson = result.json();
       
        let body = JSON.parse((result["_body"]));
        if(!body) return false;
        this.memoryESContext = body; //JSON.parse(body.Message);
        let allApplications = this.memoryESContext["allApplications"];

        if (!allApplications) return;
        for (var index = 0; index < allApplications.length; index++) {
            var applicElem = allApplications[index].application;
            if (this.applications.find(e => e === applicElem) === undefined)
                this.applications.push(applicElem);
        }
    }

    //--------------------------------------------------------------------------------
    public getDefaultContext(setAsCurrent: boolean) {
        this.subscriptions.push(this.httpMenuService.getDefaultContext().subscribe((result) => {
            if (result) {
                let array = this.extractCouple(result);
                if (array !== null && array !== undefined) {
                    this.defaultApplication = array[0].toString();
                    this.defaultModule = array[1];
                    if (setAsCurrent) {
                        this.setAppAndModule(this.defaultApplication, this.defaultModule, true);
                    }
                }
                return result;
            }
        }));
    }

    //--------------------------------------------------------------------------------
    public closeCustomizationContext() {
        this.subscriptions.push(this.httpMenuService.closeCustomizationContext().subscribe((result) => {
            this.currentApplication = undefined;
            this.currentModule = undefined;
        }));
    }

    //--------------------------------------------------------------------------------
    public refreshEasyBuilderApps(type) {
        this.httpMenuService.updateCachedDateAndSave().subscribe();
        this.httpMenuService.refreshEasyBuilderApps(type);
        this.initEasyStudioContext(type);
    }

    //--------------------------------------------------------------------------------
    public setAppAndModule(applicSelected, moduleSelected, isThisPairDefault) {
        this.subscriptions.push(this.httpMenuService.setAppAndModule(applicSelected, moduleSelected, isThisPairDefault).subscribe((result) => {
            this.currentApplication = applicSelected;
            this.currentModule = moduleSelected;
            if(isThisPairDefault){ //getDefaultContext(true)
                this.defaultApplication = applicSelected;
                this.defaultModule =  moduleSelected;
            }
            this.httpMenuService.updateBaseCustomizationContext(this.currentApplication, this.currentModule).subscribe();
        }));
    }

    //--------------------------------------------------------------------------------
    public createNewContext(newAppName, newModName, type) {
        this.subscriptions.push(this.httpMenuService.createNewContext(newAppName, newModName, type).subscribe((result) => {
            if (result) {
                let newObj = new MyObj(newAppName, newModName)
                if (this.memoryESContext.allApplications.find(e => e === newObj) === undefined)
                    this.memoryESContext.allApplications.push(newObj);
                if (this.applications.find(e => e === newAppName) === undefined) { //nessuna occorrenza
                    this.applications.push(newAppName);
                }
                this.modules = this.getModulesBy(newAppName);

                this.httpMenuService.updateCachedDateAndSave().subscribe();
                this.setAppAndModule(newAppName, newModName, false);
                return true;
            }
            return result;
        }));
    }

    //--------------------------------------------------------------------------------
    public getModulesBy(app: string) {
        let y = this.memoryESContext;
        if (!y) return;
        let modulesLocal: any[] = new Array();
        for (var index = 0; index < y.allApplications.length; index++) {
            var element = y.allApplications[index].application;
            if (element !== app)
                continue;
            if (modulesLocal.indexOf(y.allApplications[index].module) === -1)  //nessuna occorrenza
                modulesLocal.push(y.allApplications[index].module);
        }
        return modulesLocal;
    }

    //#endregion
}