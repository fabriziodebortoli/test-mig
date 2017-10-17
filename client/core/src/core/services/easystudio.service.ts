import { SettingsService } from './settings.service';
import { EsCustomizItem } from './../../shared/models/es-customization-item.model';
import { HttpMenuService } from './../../menu/services/http-menu.service';
import { Injectable, EventEmitter } from '@angular/core';

@Injectable()
export class EasystudioService {

    public isDesignable: boolean;
    public subscriptions = [];
    public currentModule: string;                      //current module selected by ESContext
    public currentApplication: string;                 //current applic selected by ESContext
    public modules: any[];                             //list of modules in the file system
    public applications: any[];                        //list of applics in the file system
    public customizations: EsCustomizItem[];    //list of customization in the file system, each knows its owners
    public memory: { Customizations: EsCustomizItem[] };


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
            if (array.length != 2) return;
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
            this.memory = { Customizations: [] };
            let res = result["_body"];
            if (res !== "") {
                this.memory = JSON.parse(result["_body"]);
                if (this.memory != undefined) {
                    this.customizations = this.memory.Customizations;/*[];
                    let r = this.memory.Customizations;
                    r.forEach(element => { this.customizations.push(element); });*/
                }
            }
            this.isDesignable = this.customizations != undefined;
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
    public  createNewContext(applicSelected, moduleSelected, type) {
        this.subscriptions.push(this.httpMenuService.createNewContext(applicSelected, moduleSelected, type).subscribe((result) => { }));
    }

    //--------------------------------------------------------------------------------
    dispose() {
        this.subscriptions.forEach(subs => subs.unsubscribe());
    }
}