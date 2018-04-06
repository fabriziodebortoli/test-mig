import { TbComponent, SettingsService, TbComponentService } from '@taskbuilder/core';
import { TabStripComponent } from '@progress/kendo-angular-layout/dist/es/tabstrip/tabstrip.component';
import { TopbarMenuAppComponent } from './../topbar/topbar-menu/topbar-menu-app/topbar-menu-app.component';
import { LayoutModule, PanelBarExpandMode } from '@progress/kendo-angular-layout';
import { Component, ViewChild, ElementRef, OnInit, AfterViewInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { Button } from '@progress/kendo-angular-buttons';
import { Collision } from '@progress/kendo-angular-popup/dist/es/models/collision.interface';
import { Align } from '@progress/kendo-angular-popup/dist/es/models/align.interface';

import { ApplicationType, EasystudioService } from './../../services/easystudio.service';

@Component({
    selector: 'tb-es-context',
    templateUrl: './easystudio-context.component.html',
    styleUrls: ['./easystudio-context.component.scss']
})
export class EasyStudioContextComponent extends TbComponent implements OnInit, OnDestroy {

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
    public wantSetPairAsDefault = false;
    //  public type = ApplicationType.Customization;

    public newPairVisible = false;
    public isDefault = false;

    @ViewChild('kendoTabStripInstance') kendoTabStripInstance: TabStripComponent;

    constructor(
        public settingsService: SettingsService,
        public easystudioService: EasystudioService,
        tbComponentService: TbComponentService,
        changeDetectorRef: ChangeDetectorRef) {
        super(tbComponentService, changeDetectorRef);
        this.enableLocalization();
    }

    //--------------------------------------------------------------------------------
    ngOnInit(): void {
        super.ngOnInit()
        this.title = this._TB('Customization Context');
        this.defaultNewApp = this._TB('NewApplication');
        this.defaultNewMod = this._TB('NewModule');
        this.openCustomizationContext = this._TB('Open Customization Context');
        this.closeCustomizationContext = this._TB('Close customization context');

        this.easystudioService.initEasyStudioContext();
        this.onTabSelect(null);
    }

    //--------------------------------------------------------------------------------
    ngOnDestroy() {
    }

    //--------------------------------------------------------------------------------
    public onTabSelect(e) {
        let oldType = this.easystudioService.applicType;
        this.easystudioService.applicType = (e == null || e.index == 0)
            ? ApplicationType.Customization : ApplicationType.TBApplication;
        if (oldType != this.easystudioService.applicType) {
            this.easystudioService.modules = [];
            this.ResetProperties();
        }
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
        this.easystudioService.canModifyContext().subscribe((result) => {
            if (!result) {
                this.sayCantCustomize();
                return;
            }
            this.opened = false;
            this.newPairVisible = false;
            this.easystudioService.closeCustomizationContext();
            this.wantSetPairAsDefault = false;
            this.ResetProperties();
        });
    }

    //--------------------------------------------------------------------------------
    public cancel() {
        this.opened = false;
        this.newPairVisible = false;
        this.ResetProperties();
        this.kendoTabStripInstance.selectTab(0);
        this.easystudioService.applicType = ApplicationType.Customization;
    }

    //--------------------------------------------------------------------------------
    public changeCustomizationContext() {
        this.easystudioService.getDefaultContext(false);
        this.easystudioService.canModifyContext().subscribe((result) => {
            if (!result) {
                this.sayCantCustomize();
                return;
            }
            this.opened = !this.opened;
            if (this.contextIsValid()) {
                this.setApplic(this.easystudioService.currentApplication);
                this.setModule(this.easystudioService.currentModule);
            }
        });
    }

    //--------------------------------------------------------------------------------
    public refresh() {
        this.ResetProperties();
        this.kendoTabStripInstance.selectTab(0);
        this.easystudioService.applicType = ApplicationType.Customization;
        this.easystudioService.refreshEasyBuilderApps();
    }

    //--------------------------------------------------------------------------------
    public ResetProperties() {
        this.applicSelected = undefined;
        this.moduleSelected = undefined;
        this.isDefault = false;
    }

    //--------------------------------------------------------------------------------
    public ok() {
        let elemSearched = this.easystudioService.getMemoryForType().find(
            c => c.application === this.applicSelected && c.module === this.moduleSelected);
        if (elemSearched === undefined) //ora per default, se hanno digitato una coppia che non esista, gliela creo
            this.addNewPair(this.applicSelected, this.moduleSelected);
        this.easystudioService.setAppAndModule(this.applicSelected, this.moduleSelected, this.wantSetPairAsDefault);
        this.opened = false;
        this.wantSetPairAsDefault = false;
        this.isDefault = false;
    }

    //--------------------------------------------------------------------------------
    public setDefaultContext() {
        this.wantSetPairAsDefault = this.selectionIsValid();
    }

    //--------------------------------------------------------------------------------
    public setApplic(app: string) {
        if (this.easystudioService.getApplications().indexOf(app) === -1) return;
        this.applicSelected = app;
        this.moduleSelected = undefined;
        this.wantSetPairAsDefault = false;
        this.isDefault = false;
        this.easystudioService.modules = this.easystudioService.getModulesBy(app);
        if (this.easystudioService.modules.length == 1) {
            this.moduleSelected = this.easystudioService.getModules()[0];
            this.checkIfIsDefault();
        }
    }

    //--------------------------------------------------------------------------------
    public setModule(mod: string) {
        if (this.easystudioService.getModules().indexOf(mod) === -1) return;
        this.moduleSelected = mod;
        this.checkIfIsDefault();
    }

    //--------------------------------------------------------------------------------
    private checkIfIsDefault() {
        this.isDefault = this.easystudioService.defaultApplication == this.applicSelected
            && this.easystudioService.defaultModule == this.moduleSelected;
    }

    //--------------------------------------------------------------------------------
    public showNewPair(show: boolean) {
        this.newPairVisible = show;
        if (!show) return;

        if (this.applicSelected === undefined)
            this.newApplic = this.generateNewApplicationName();
        else this.newApplic = this.applicSelected;
        if (this.moduleSelected === undefined)
            this.newModule = this.generateNewModuleName(this.applicSelected);
        else this.newModule = this.moduleSelected;

    }

    //---------------------------------------------------------------------------------------------
    public addNewPair(newAppName, newModName) {
        if (newAppName === undefined || newModName === undefined)
            return;
        if (this.easystudioService.getMemoryForType().indexOf(newAppName, newModName) === -1) {
            this.easystudioService.createNewContext(newAppName, newModName);//type = standard or custom
            this.applicSelected = newAppName;
            this.moduleSelected = newModName;
        }
        this.newPairVisible = false;
        this.isDefault = false;
    }

    //--------------------------------------------------------------------------------
    public generateNewApplicationName(): any {
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
    public generateNewModuleName(appName) {
        var i = 0;
        var newName = undefined;
        do {
            i++;
            newName = this.defaultNewMod + i.toString();

        } while (this.exists(newName, undefined));
        return newName;
    }

    //--------------------------------------------------------------------------------
    public exists(newName: string, newModName: string) {
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
    public sayCantCustomize() {
        alert("TO DO - utilizzare diagnostic - " + this._TB('It is not possible to change customization context with opened documents.\r\nPlease, close all documents.'))
        // this.snackBar.open(
        //     this._TB('It is not possible to change customization context with opened documents.\r\nPlease, close all documents.'),
        //     this._TB('Ok')
        // );
    }
    //--------------------------------------------------------------------------------
    public openDefaultContextMethod(): void {
        this.easystudioService.canModifyContext().subscribe((result) => {
            if (!result) {
                this.sayCantCustomize();
                return;
            }
            this.easystudioService.getDefaultContext(true);
        });
    }

}