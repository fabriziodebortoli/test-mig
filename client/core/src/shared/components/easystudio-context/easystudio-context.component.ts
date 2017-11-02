import { TopbarMenuAppComponent } from './../topbar/topbar-menu/topbar-menu-app/topbar-menu-app.component';
import { SettingsService } from './../../../core/services/settings.service';
import { InfoService } from './../../../core/services/info.service';
import { EasystudioService } from './../../../core/services/easystudio.service';
import { LocalizationService } from './../../../core/services/localization.service';
import { LayoutModule, PanelBarExpandMode } from '@progress/kendo-angular-layout';
import { Component, ViewChild, ElementRef, OnInit, AfterViewInit, OnDestroy } from '@angular/core';
import { Button } from '@progress/kendo-angular-buttons';
import { Collision } from '@progress/kendo-angular-popup/dist/es/models/collision.interface';
import { Align } from '@progress/kendo-angular-popup/dist/es/models/align.interface';


@Component({
    selector: 'tb-es-context',
    templateUrl: './easystudio-context.component.html',
    styleUrls: ['./easystudio-context.component.scss']
})


export class EasyStudioContextComponent implements OnInit, OnDestroy {

    public localizationsLoadedSubscription: any;
    public localizationLoaded: boolean;

    public opened: boolean = false;

    public title: string;
    public defaultNewApp: string;
    public defaultNewMod: string;
    public closeCustomizationContext: string;
    public openCustomizationContext: string;

    public showAddModuleButton = false;
    public showAddPairButton = false;

    public applicSelected: string;
    public moduleSelected: string;
    public newApplic: string;
    public newModule: string;
    public isThisPairDefault = false;
    public type = "Customization";

    public newPairVisible = false;
    public isDefault = false;

    constructor(
        public localizationService: LocalizationService,
        public easystudioService: EasystudioService,
        public infoService: InfoService,
        public settingsService: SettingsService) { }

    //--------------------------------------------------------------------------------
    ngOnInit(): void {
        this.localizationsLoadedSubscription = this.localizationService.localizationsLoaded.subscribe((loaded) => {
            this.localizationLoaded = loaded;
            if (this.localizationLoaded && this.localizationService.localizedElements) {
                this.title = this.localizationService.localizedElements.CustomizationContext;
                this.defaultNewApp = this.localizationService.localizedElements.DefaultNewApp;
                this.defaultNewMod = this.localizationService.localizedElements.DefaultNewMod;
                this.openCustomizationContext = this.localizationService.localizedElements.OpenCustomizationContext;
                this.closeCustomizationContext = this.localizationService.localizedElements.CloseCustomizationContext;
            }
        });
        this.easystudioService.initEasyStudioContext();
        this.easystudioService.getDefaultContext(false);
    }

    //--------------------------------------------------------------------------------
    ngOnDestroy() {
        this.localizationsLoadedSubscription.unsubscribe();
    }

    //--------------------------------------------------------------------------------
    public contextIsValid() {
        return this.easystudioService.isContextActive();
    }

    //--------------------------------------------------------------------------------
    public selectionIsValid() {
        return this.applicSelected !== undefined && this.moduleSelected !== undefined;
    }

    //--------------------------------------------------------------------------------
    public close() {
        this.opened = false;
        this.newPairVisible = false;
        this.easystudioService.closeCustomizationContext();
        this.applicSelected = undefined;
        this.moduleSelected = undefined;
        this.isThisPairDefault = false;
        this.isDefault = false;
    }

    //--------------------------------------------------------------------------------
    public cancel() {
        this.opened = false;
        this.newPairVisible = false;
    }

    //--------------------------------------------------------------------------------
    public changeCustomizationContext() {
        this.opened = !this.opened;
        if (this.contextIsValid()) {
            this.setApplic(this.easystudioService.currentApplication);
            this.setModule(this.easystudioService.currentModule);
        }
    }

    //--------------------------------------------------------------------------------
    public refresh() {
        this.easystudioService.refreshEasyBuilderApps();
        this.applicSelected = undefined;
        this.moduleSelected = undefined;
    }

    //--------------------------------------------------------------------------------
    public ok() {
        let elemSearched = this.easystudioService.memoryESContext.allApplications.find(
            c => c.application === this.applicSelected && c.module === this.moduleSelected);
        if(elemSearched === undefined) //ora per default, se hanno digitato una coppia che non esista, gliela creo
            this.addNewPair(this.applicSelected, this.moduleSelected);
        this.easystudioService.setAppAndModule(this.applicSelected, this.moduleSelected, this.isThisPairDefault);
        this.opened = false;
        this.isThisPairDefault = false;
        this.isDefault = false;
    }

    //--------------------------------------------------------------------------------
    public setDefaultContext() {
        this.isThisPairDefault = this.selectionIsValid();
    }

    //--------------------------------------------------------------------------------
    private setApplic(app: string) {
        if (this.easystudioService.getApplications().indexOf(app) === -1) return;
        this.applicSelected = app;
        this.moduleSelected = undefined;
        this.isThisPairDefault = false;
        this.isDefault = false;
        this.easystudioService.modules = this.easystudioService.getModulesBy(app);
        if (this.easystudioService.modules.length == 1) {
            this.moduleSelected = this.easystudioService.getModules()[0];
            this.checkIfIsDefault();
        }
    }

    //--------------------------------------------------------------------------------
    private setModule(mod: string) {
        if (this.easystudioService.getModules().indexOf(mod) === -1) return;
        this.moduleSelected = mod;
        this.checkIfIsDefault();
    }

    //--------------------------------------------------------------------------------
    private checkIfIsDefault() {
        this.isDefault = this.easystudioService.defaultApplication === this.applicSelected 
         && this.easystudioService.defaultModule === this.moduleSelected;
    }

    //--------------------------------------------------------------------------------
    showNewPair(show: boolean) {
        this.newPairVisible = show;
        if (show) {
            this.newApplic = this.generateNewApplicationName();
            this.newModule = this.generateNewModuleName(this.applicSelected);
            this.applicSelected = undefined;
            this.moduleSelected = undefined;
        }
    }

    //---------------------------------------------------------------------------------------------
    addNewPair(newAppName, newModName) {
        if (newAppName === undefined || newModName === undefined)
            return;
        if (this.easystudioService.memoryESContext.allApplications.indexOf(newAppName, newModName) === -1) {
            this.easystudioService.createNewContext(newAppName, newModName, this.type);//type = standard or custom
            this.applicSelected = newAppName;
            this.moduleSelected = newModName;
        }
        this.newPairVisible = false;
    }

    //--------------------------------------------------------------------------------
    generateNewApplicationName(): any {
        var i = 0;
        var newName = undefined;
        do {
            i++;
            newName = this.defaultNewApp + i.toString();

        } while (this.exists(newName, undefined));
        this.newPairVisible = true;
        return newName;
    }

    //--------------------------------------------------------------------------------
    generateNewModuleName(appName) {
        var i = 0;
        var newName = undefined;
        do {
            i++;
            newName = this.defaultNewMod + i.toString();

        } while (this.exists(newName, undefined));
        return newName;
    }

    //--------------------------------------------------------------------------------
    exists(newName: string, newModName: string) {
        if (newName === undefined)
            return;
        var list = [];
        if (newModName === undefined) {
            list = this.easystudioService.getApplications();
            return list.indexOf(newName) !== -1;
        }

        list = this.easystudioService.getModulesBy(newName);
        return list.indexOf(newModName) !== -1;
    }

    //--------------------------------------------------------------------------------
    openDefaultContextMethod(): void{
        this.easystudioService.getDefaultContext(true);
    }

}