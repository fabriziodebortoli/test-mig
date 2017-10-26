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

    public expandMode: number = PanelBarExpandMode.Multiple;
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
    public isThisPairDefault = false;
    public type = "Customization";

    public newPairVisible = false;

    constructor(
        public localizationService: LocalizationService,
        public easystudioService: EasystudioService,
        public infoService: InfoService,
        public settingsService: SettingsService ) {}

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
    }

    //--------------------------------------------------------------------------------
    public ok() {
        this.easystudioService.setAppAndModule(this.applicSelected, this.moduleSelected, this.isThisPairDefault);
        this.opened = false;
    }

    //--------------------------------------------------------------------------------
    private setApplic(app: string) {
        if (this.easystudioService.applications.indexOf(app) === -1) return;
        this.applicSelected = app;
        this.moduleSelected = undefined;
        this.easystudioService.modules = this.easystudioService.getModulesBy(app);
        if (this.easystudioService.modules.length == 1) {
            this.moduleSelected = this.easystudioService.modules[0];
        }
    }

    //--------------------------------------------------------------------------------
    private setModule(mod: string) {
        if (this.easystudioService.modules.indexOf(mod) === -1) return;
        this.moduleSelected = mod;
    }

    //--------------------------------------------------------------------------------
    showNewPair(show: boolean) {
        this.newPairVisible = show;
        if (show) {
            this.applicSelected = this.generateNewApplicationName();
            this.moduleSelected = this.generateNewModuleName(this.applicSelected);
        }
        else {
            this.applicSelected = undefined;
            this.moduleSelected = undefined;
        }
    }

    //---------------------------------------------------------------------------------------------
    addNewPair(newAppNameEl, newModNameEl) {
        if (newAppNameEl === undefined || newModNameEl === undefined)
            return;
        if (newAppNameEl.value === undefined || newModNameEl.value === undefined)
            return;
        let newAppName = newAppNameEl.value;
        let newModName = newModNameEl.value;
        if (this.easystudioService.memory.allApplications.indexOf(newAppName, newModName) === -1) {
            //type = standard or custom
            this.easystudioService.createNewContext(this.applicSelected, this.moduleSelected, this.type);

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
            list = this.easystudioService.applications;
            return list.indexOf(newName) !== -1;
        }

        list = this.easystudioService.getModulesBy(newName);
        return list.indexOf(newModName) !== -1;
    }
}