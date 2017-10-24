import { SettingsService } from './settings.service';
import { EsCustomizItem } from './../../shared/models/es-customization-item.model';
import { HttpMenuService } from './../../menu/services/http-menu.service';
import { Injectable, EventEmitter } from '@angular/core';

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
    public applications: any[];                        //list of applics in the file system
    public customizations: EsCustomizItem[];    //list of customization in the file system, each knows its owners
    public memory: { allApplications: MyObj[] };
    public memory2: { Customizations: EsCustomizItem[] };


    constructor(public httpMenuService: HttpMenuService,
        public settingService: SettingsService) {
        this.getCurrentContext();
    }

    //--------------------------------------------------------------------------------
    public getCurrentContext(): any {
        this.subscriptions.push(this.httpMenuService.getCurrentContext().subscribe((result) => {
            this.extractNames(result);
            return result;
        }));
    }

    //--------------------------------------------------------------------------------
    private extractNames(result: Response) {
        if (result == undefined) return;
        let res = result["_body"];
        if (res !== "") {
            let array: string[] = res.toString().split(';');
            if (!array || array.length != 2) return;
            this.currentApplication = array[0];
            this.currentModule = array[1];
        }
    }

    //--------------------------------------------------------------------------------
    public initEasyStudioContext() {
        this.subscriptions.push(this.httpMenuService.getEsAppsAndModules().subscribe((result) => {
            this.extractNames2(result);
            return result;
        }));
    }

    //--------------------------------------------------------------------------------
    public initEasyStudioData(object: any) {
        this.subscriptions.push(this.httpMenuService.initEasyStudioData(object).subscribe((result) => {
            this.memory2 = { Customizations: [] };
            this.customizations = [];
            let body = result["_body"]; // if body=="" is not design, if body=="Customizations:{}" is design but empty
            let canHaveCustoms = body != "";
            if (canHaveCustoms) {                
                this.memory2 = JSON.parse(body);
                if (this.memory2 != undefined) {
                    this.memory2.Customizations.forEach(element => {
                        if (this.customizations.indexOf(element) === -1)
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
    public isContextActive(): boolean {
        return this.currentApplication !== undefined && this.currentModule !== undefined;
    }

    //--------------------------------------------------------------------------------
    public runEasyStudio(target: any, customizationName: string) {
        this.subscriptions.push(this.httpMenuService.runEasyStudio(target, customizationName).subscribe((result) => { }));
    }

    //--------------------------------------------------------------------------------
    public closeCustomizationContext() {
        this.subscriptions.push(this.httpMenuService.closeCustomizationContext().subscribe((result) => {
            this.currentApplication = undefined;
            this.currentModule = undefined;
        }));
    }

    //--------------------------------------------------------------------------------
    private extractNames2(result: Response) {
        if (result == undefined) return;
        this.applications = [];
        this.modules = [];

        let resultJson = result.json();        //let resultText = result.text();
        let body = result["_body"];
        if (body === undefined || body === "")
            return;
        this.memory = JSON.parse(result["_body"]);
        let allApplications = resultJson["allApplications"];
        if(!allApplications) return;
        for (var index = 0; index < allApplications.length; index++) {
            var applicElem = allApplications[index].application;
            if (this.applications.indexOf(applicElem) === -1)
                this.applications.push(applicElem);
        }
    }

    //--------------------------------------------------------------------------------
    public refreshEasyBuilderApps() {
        this.subscriptions.push(this.httpMenuService.refreshEasyBuilderApps().subscribe((result) => {
            this.extractNames2(result);
            return result;
        }));
    }

    //--------------------------------------------------------------------------------
    public setAppAndModule(applicSelected, moduleSelected, isThisPairDefault) {
        this.subscriptions.push(this.httpMenuService.setAppAndModule(applicSelected, moduleSelected, isThisPairDefault).subscribe((result) => {
            this.currentApplication = applicSelected;
            this.currentModule = moduleSelected;
        }));
    }

    //--------------------------------------------------------------------------------
    public createNewContext(newAppName, newModName, type) {
        this.subscriptions.push(this.httpMenuService.createNewContext(newAppName, newModName, type).subscribe((result) => {
            if (result) {
                this.modules.push(newModName);
                //this.memory.allApplications.push(newAppName, newModName);
                this.memory.allApplications.push(new MyObj(newAppName, newModName));

                if (this.applications.indexOf(newAppName) === -1) { //nessuna occorrenza
                    this.applications.push(newAppName);
                }
                this.modules = this.getModulesBy(newAppName);
                this.setAppAndModule(newAppName, newModName, false);

            }
        }));
    }

    //--------------------------------------------------------------------------------
    public getModulesBy(app: string) {
        let y = this.memory;
        if(!y) return;
        let modules: any[] = new Array();
        for (var index = 0; index < y.allApplications.length; index++) {
            var element = y.allApplications[index].application;
            if (element !== app)
                continue;
            modules.push(y.allApplications[index].module);
        }
        return modules;
    }

    //--------------------------------------------------------------------------------
    dispose() {
        this.subscriptions.forEach(subs => subs.unsubscribe());
    }
}