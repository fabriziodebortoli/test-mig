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
    public lastApplicSelected: string;
    public lastModuleSelected: string;
    public isThisPairDefault = false;
    public type = "Customization";

    public newPairVisible = false;

    constructor(
        public localizationService: LocalizationService,
        public easystudioService: EasystudioService,        
        public infoService: InfoService,
        public settingsService: SettingsService
        
    ) {
    }

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

        if(this.easystudioService.isContextActive()){
            this.applicSelected = this.easystudioService.currentApplication;
            this.moduleSelected = this.easystudioService.currentModule;
        }
    }


    //--------------------------------------------------------------------------------
    ngOnDestroy() {
        this.localizationsLoadedSubscription.unsubscribe();
    }

    //--------------------------------------------------------------------------------
    public contextIsValid() {
        return this.easystudioService.currentApplication !== undefined && this.easystudioService.currentModule !== undefined;
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
    private setApplic(app: string, inputToSet) {
        this.applicSelected = app;
        this.moduleSelected = undefined;
        this.easystudioService.modules = this.easystudioService.getModulesBy(app);
        this.hightlightApp(app);
    }

    //--------------------------------------------------------------------------------
    private setModule(mod: string, inputToSet) {
        this.moduleSelected = mod;
        this.hightlightMod(mod);
    }

    //--------------------------------------------------------------------------------
    hightlightApp(item: any) {
        if (!item) return;
        if (this.lastApplicSelected && this.lastApplicSelected) {
            let prev = document.getElementById(this.lastApplicSelected);
            if (prev) prev.className = "";
        }
        let button = this.hightlightButton(item);
        if (button) {
            this.lastApplicSelected = button.id;
            button.className = "selected";
        }
        if (this.easystudioService.modules.length == 1) {
            this.moduleSelected = this.easystudioService.modules[0];
            let mod = document.getElementById(this.moduleSelected);
            if (mod) mod.className = "";
            this.hightlightMod(this.moduleSelected);
        }
        // //se invece ho già indicazione di un modulo, controllo che esista e lo evidenzio
        // else if ($scope.module && $scope.ExistsModule(elem, $scope.module)) {
        // 	$scope.hightlightMod($scope.module);
        // }
    }

    //---------------------------------------------------------------------------------------------
    hightlightMod(item: any) {
        // if ($scope.application == easyStudioService.defaultApplication && $scope.module == easyStudioService.defaultModule) {
        // 	$scope.formData.isFavorite = true;
        // }
        if (this.lastModuleSelected) {
            let prev = document.getElementById(this.lastModuleSelected);
            if (prev) prev.className = "";
        }
        let button = this.hightlightButton(item);
        if (!button) return;
        this.lastModuleSelected = button.id;
        button.className = "selected";
    };

    //--------------------------------------------------------------------------------
    hightlightButton(item: any) {
        let button = document.getElementById(item);
        if (!button) return;
        button.className = "selected";
        return button;
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

    ifHasToBe(mod) {
        return ((this.easystudioService.modules.length == 1) ||
            (this.moduleSelected && this.moduleSelected === mod));
    }



    /*//---------------------------------------------------------------------------------------------
	$scope.setInvisibleMod = function () {
		$scope.addModuleVisible = false;
	}

	//---------------------------------------------------------------------------------------------
	$scope.setInvisiblePair = function () {
		$scope.addPairVisible = false;
	}

	//---------------------------------------------------------------------------------------------
	$scope.setVisibleMod = function () {
		$scope.addModuleVisible = true;
		if ($scope.application !== undefined)
			$scope.GenerateNewModuleName($scope.application);
		$scope.setInvisiblePair();
	}

	//---------------------------------------------------------------------------------------------
	$scope.setVisiblePair = function () {
		$scope.addPairVisible = true;
		var newApp = $scope.GenerateNewApplicationName();
		$scope.GenerateNewModuleName(newApp);
		$scope.setInvisibleMod();
	}*/



}