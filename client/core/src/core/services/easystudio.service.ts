import { EasyStudioContextComponent } from './../../shared/components/easystudio-context/easystudio-context.component';
import { SettingsService } from './settings.service';
import { EsCustomizItem, PairAppMod } from './../../shared/models/es-customization-item.model';
import { HttpMenuService } from './../../menu/services/http-menu.service';
import { Injectable, EventEmitter } from '@angular/core';
import { Observable } from '../../rxjs.imports';
import { InfoService } from './info.service';

@Injectable()
export class EasystudioService {


    public applicType: any;
    public isDesignable: boolean;
    public subscriptions = [];
    public currentModule: string;                      //current module selected by ESContext
    public currentApplication: string;                 //current applic selected by ESContext          

    public customizations: EsCustomizItem[];    //list of customization in the file system, each knows its owners
    public memoryCustsList: { Customizations: EsCustomizItem[] };

    public memoryCusts: { appsList: PairAppMod[] };
    public memoryTbApps:{ appsList: PairAppMod[] };

    private custsApps: any[] = [];
    private tbappApps: any[] = [];
    //--------------------------------------------------------------------------------  
    get applications(): any[] {
        // this.modules = [];
        if (this.applicType == ApplicationType.Customization)
            return this.custsApps;
        if (this.applicType == ApplicationType.TBApplication)
            return this.tbappApps;
    }
    set applications(theBar: any[]) {
        if (this.applicType == ApplicationType.Customization)
            this.custsApps = theBar;
        if (this.applicType == ApplicationType.TBApplication)
            this.tbappApps = theBar;
    }

    private custsMods: any[] = [];
    private tbappMods: any[] = [];
    //--------------------------------------------------------------------------------  
    get modules(): any[] {
        if (this.applicType == ApplicationType.Customization)
            return this.custsMods;
        if (this.applicType == ApplicationType.TBApplication)
            return this.tbappMods;
    }
    set modules(theBar: any[]) {
        if (this.applicType == ApplicationType.Customization)
            this.custsMods = theBar;
        if (this.applicType == ApplicationType.TBApplication)
            this.tbappMods = theBar;
    }

    //--------------------------------------------------------------------------------  
    private _easystudioEdition: boolean; //default applic read from preferences
    get easystudioEdition(): boolean {
        return this._easystudioEdition;
    }
    set easystudioEdition(theBar: boolean) {
        this._easystudioEdition = theBar;
    }

    //--------------------------------------------------------------------------------
    private _defaultModule: string;//default module read from preferences
    get defaultModule(): string {
        return this._defaultModule;
    }
    set defaultModule(theBar: string) {
        this._defaultModule = theBar;
    }

    //--------------------------------------------------------------------------------  
    private _defaultApplication: string; //default applic read from preferences
    get defaultApplication(): string {
        return this._defaultApplication;
    }
    set defaultApplication(theBar: string) {
        this._defaultApplication = theBar;
    }

    //#region both
    //--------------------------------------------------------------------------------
    constructor(
        public httpMenuService: HttpMenuService,
        public infoService: InfoService,
        public settingsService: SettingsService) {
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
    public easyStudioActivation(): any {
        return this.settingsService.IsEasyStudioActivated && this.infoService.isDesktop;
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
            this.isDesignable = canHaveCustoms;
        }));
    }

    //--------------------------------------------------------------------------------
    public isEasyStudioDocument(object: any) {
        this.subscriptions.push(this.httpMenuService.isEasyStudioDocument(object).subscribe((result) => {
            return result;
        }));
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
    public initEasyStudioContext() {
        this.subscriptions.push(this.httpMenuService.getEsAppsAndModules(ApplicationType.Customization).subscribe((result) => {
            this.extractInfos(result, ApplicationType.Customization);
            return result;
        }));

        this.subscriptions.push(this.httpMenuService.getEsAppsAndModules(ApplicationType.TBApplication).subscribe((result) => {
            this.extractInfos(result, ApplicationType.TBApplication);
            return result;
        }));
    }

    //--------------------------------------------------------------------------------
    public canModifyContext(): Observable<any> {
        return this.httpMenuService.canModifyContext().map((res: Response) => res.json());
    }

    //--------------------------------------------------------------------------------
    public extractInfos(result: Response, type: ApplicationType) {
        if (result == undefined) return;

        let body = JSON.parse((result["_body"]));
        if (!body) return false;
        this.extractESEdition(body);
        if (type == ApplicationType.Customization)
            this.extractNamesAllApps(body);
        else if (type == ApplicationType.TBApplication)
            this.extractNamesTbApps(body);
    }

    //--------------------------------------------------------------------------------
    public extractESEdition(body: any) {
        this.easystudioEdition = body["DeveloperEd"];
    }
   
    //--------------------------------------------------------------------------------
    public getMemoryForType(): PairAppMod[] {
        if (this.applicType == ApplicationType.Customization)
            return this.memoryCusts["appsList"];
        if (this.applicType == ApplicationType.TBApplication)
            return this.memoryTbApps["appsList"];
    }

    //--------------------------------------------------------------------------------
    private extractNamesAllApps(body: any) {
        this.custsApps = [];
        this.modules = [];
        this.memoryCusts = body;
        let allApplications = this.getMemoryForType();

        if (!allApplications) return;
        for (var index = 0; index < allApplications.length; index++) {
            var applicElem = allApplications[index].application;
            if (this.custsApps.find(e => e === applicElem) === undefined)
                this.custsApps.push(applicElem);
        }
    }

    //--------------------------------------------------------------------------------
    private extractNamesTbApps(body: any) {
        this.tbappApps = [];
        this.modules = [];
        this.memoryTbApps = body; 
        let allApplications = this.memoryTbApps["appsList"]; //in apertura il type non cambia, non posso prenderlo dal metodo

        if (!allApplications) return;
        for (var index = 0; index < allApplications.length; index++) {
            var applicElem = allApplications[index].application;
            if (this.tbappApps.find(e => e === applicElem) === undefined)
                this.tbappApps.push(applicElem);
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
            this.setAppAndModule(this.currentApplication, this.currentModule, false);
        }));
    }

    //--------------------------------------------------------------------------------
    public refreshEasyBuilderApps(type = undefined) {
        this.httpMenuService.updateCachedDateAndSave().subscribe(
            (result) => {
                if (result) {
                    this.httpMenuService.checkAfterRefresh(type).subscribe();
                    this.memoryCusts = undefined;
                    this.memoryTbApps = undefined;
                    this.initEasyStudioContext();
                    this.getDefaultContext(false);
                }
                return result;
            }
        );
        this.subscriptions.push(this.httpMenuService.cleanApplicationInfosPathFinder().subscribe());
    }

    //--------------------------------------------------------------------------------
    public setAppAndModule(applicSelected, moduleSelected, isThisPairDefault) {
        this.subscriptions.push(this.httpMenuService.setAppAndModule(applicSelected, moduleSelected, isThisPairDefault).subscribe((result) => {
            this.currentApplication = applicSelected;
            this.currentModule = moduleSelected;
            if (isThisPairDefault) {
                this.defaultApplication = applicSelected;
                this.defaultModule = moduleSelected;
            }
            this.httpMenuService.updateBaseCustomizationContext(this.currentApplication, this.currentModule).subscribe();
        }));
    }

    //--------------------------------------------------------------------------------
    public createNewContext(newAppName, newModName) {
        this.subscriptions.push(this.httpMenuService.createNewContext(newAppName, newModName, this.applicType)
        .subscribe((result) => {
            if (result) {
                let newObj = new PairAppMod(newAppName, newModName);
                if (this.getMemoryForType().find(e => e === newObj) === undefined)
                    this.getMemoryForType().push(newObj);
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
        let apps = this.getMemoryForType();
        if (apps == undefined) return null;
        let modulesLocal: any[] = new Array();

        for (var index = 0; index < apps.length; index++) {
            var element = apps[index].application;
            if (element !== app)
                continue;
            if (modulesLocal.indexOf(apps[index].module) === -1)  //nessuna occorrenza
                modulesLocal.push(apps[index].module);
        }
        return modulesLocal;
    }

    //#endregion
}

export enum ApplicationType {
    Customization = 'Customization',
    TBApplication = 'TaskBuilderApplication'
} 