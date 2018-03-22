import { TbComponentService } from './../../../core/services/tbcomponent.service';
import { MatSnackBar } from '@angular/material';
import { TopbarMenuAppComponent } from './../topbar/topbar-menu/topbar-menu-app/topbar-menu-app.component';
import { SettingsService } from './../../../core/services/settings.service';
import { InfoService } from './../../../core/services/info.service';
import { EasystudioService } from './../../../core/services/easystudio.service';
import { LayoutModule, PanelBarExpandMode } from '@progress/kendo-angular-layout';
import { Component, ViewChild, ElementRef, OnInit, AfterViewInit, OnDestroy, ChangeDetectorRef } from '@angular/core';
import { Button } from '@progress/kendo-angular-buttons';
import { Collision } from '@progress/kendo-angular-popup/dist/es/models/collision.interface';
import { Align } from '@progress/kendo-angular-popup/dist/es/models/align.interface';
import { TbComponent } from './../../../shared/components/tb.component';


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
    public type = "Customization";

    public newPairVisible = false;
    public isDefault = false;

    constructor(
        public easystudioService: EasystudioService,
        public infoService: InfoService,
        public settingsService: SettingsService,
        public snackBar: MatSnackBar,
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

        this.easystudioService.initEasyStudioContext(this.type);
    }

    //--------------------------------------------------------------------------------
    ngOnDestroy() {
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
            this.applicSelected = undefined;
            this.moduleSelected = undefined;
            this.wantSetPairAsDefault = false;
            this.isDefault = false;
        });
    }

    //--------------------------------------------------------------------------------
    public cancel() {
        this.opened = false;
        this.newPairVisible = false;
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
        this.easystudioService.refreshEasyBuilderApps(this.type);
    }

    //--------------------------------------------------------------------------------
    public ResetProperties() {
        this.applicSelected = undefined;
        this.moduleSelected = undefined;
        this.defaultNewApp = undefined;
        this.defaultNewMod = undefined;
        this.isDefault = false;
    }

    //--------------------------------------------------------------------------------
    public ok() {
        let elemSearched = this.easystudioService.memoryESContext.allApplications.find(
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
        if (!show)  return;

        if(this.applicSelected === undefined)
            this.newApplic = this.generateNewApplicationName();
        else this.newApplic = this.applicSelected;
        if(this.moduleSelected === undefined) 
            this.newModule = this.generateNewModuleName(this.applicSelected);
        else this.newModule = this.moduleSelected;               
        
    }

    //---------------------------------------------------------------------------------------------
    public addNewPair(newAppName, newModName) {
        if (newAppName === undefined || newModName === undefined)
            return;
        if (this.easystudioService.memoryESContext.allApplications.indexOf(newAppName, newModName) === -1) {
            this.easystudioService.createNewContext(newAppName, newModName, this.type);//type = standard or custom
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
        this.snackBar.open(
            this._TB('It is not possible to change customization context with opened documents.\r\nPlease, close all documents.'),
            this._TB('Ok')
        );
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