import { EasystudioService } from './../../../core/services/easystudio.service';
import { LocalizationService } from './../../../core/services/localization.service';
import { HttpMenuService } from './../../../menu/services/http-menu.service';
import { LayoutModule, PanelBarExpandMode } from '@progress/kendo-angular-layout';
import { Component, ViewChild, ElementRef, OnInit, AfterViewInit, OnDestroy } from '@angular/core';
import { Button } from '@progress/kendo-angular-buttons';
import { Collision } from '@progress/kendo-angular-popup/dist/es/models/collision.interface';
import { Align } from '@progress/kendo-angular-popup/dist/es/models/align.interface';

export interface MyObj {
    application: string
    module: string
}

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

    public isEasyStudioActivated = true;
    public showAddModuleButton = false;
    public showAddPairButton = false;

    public applications: any[] = new Array();
    public modules: any[];
    public memory: { allApplications: MyObj[] };
    public applicSelected: string;
    public moduleSelected: string;
    public lastApplicSelected: string;
    public lastModuleSelected: string;
    public isThisPairDefault = false;
    public type = "Customization";

    public newPairVisible = false;

    constructor(
        public httpMenuService: HttpMenuService,
        public localizationService: LocalizationService,
        public easystudioService: EasystudioService
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
            }
        });

        let sub = this.httpMenuService.getEsAppsAndModules().subscribe((result) => {
            this.extractNames(result);
            sub.unsubscribe();
        });

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
        return !this.disabledIf();
    }

    //--------------------------------------------------------------------------------
    public canShowEasyStudioButton() {
        return true;
    }

    //--------------------------------------------------------------------------------
    public close() {
        this.opened = false;
        this.easystudioService.closeCustomizationContext();
        // let sub = this.httpMenuService.closeCustomizationContext().subscribe((result) => {
        //     if(result){
        this.applicSelected = undefined;
        this.moduleSelected = undefined;
        //     }
        //     sub.unsubscribe(); });
    }

    //--------------------------------------------------------------------------------
    public cancel() {
        this.opened = false;
    }

    //--------------------------------------------------------------------------------
    public changeCustomizationContext() {
        this.opened = !this.opened;
    }

    //--------------------------------------------------------------------------------
    public refresh() {
        let sub = this.httpMenuService.refreshEasyBuilderApps().subscribe(
            result => {
                this.extractNames(result);
                sub.unsubscribe();
            }
        );
    }

    //--------------------------------------------------------------------------------
    public ok() {
        this.easystudioService.setAppAndModule(this.applicSelected, this.moduleSelected, this.isThisPairDefault);
        // let sub = this.httpMenuService.setAppAndModule(this.applicSelected, this.moduleSelected, this.isThisPairDefault).subscribe((result) => {
        //     sub.unsubscribe();
        // });
        //pairDefault= false;
        this.opened = false;
    }

    //--------------------------------------------------------------------------------
    public disabledIf() {
        return this.applicSelected === undefined || this.moduleSelected === undefined || this.newPairVisible;
    }

    //--------------------------------------------------------------------------------
    private extractNames(result: Response) {
        if (result == undefined) return;
        this.applications = [];
        this.modules = [];
        this.memory = { allApplications: [] };

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
    private getModulesBy(app: string) {
        let y = this.memory;
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
    private setApplic(app: string, inputToSet) {
        this.applicSelected = app;
        this.moduleSelected = undefined;
        this.modules = this.getModulesBy(app);
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
        if (this.modules.length == 1) {
            this.moduleSelected = this.modules[0];
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
        if (this.memory.allApplications.indexOf(newAppName, newModName) === -1) {
            //type = standard or custom
            let sub = this.httpMenuService.createNewContext(this.applicSelected, this.moduleSelected, this.type).subscribe((result) => {
                sub.unsubscribe();
            });
            this.memory.allApplications.push(newAppName, newModName);
            this.applicSelected = newAppName;
            this.moduleSelected = newModName;
            if (this.applications.indexOf(newAppName) === -1) { //nessuna occorrenza
                this.applications.push(newAppName);
            }
        }
        this.newPairVisible = false;
        this.modules = this.getModulesBy(newAppName);
        this.refresh();
        let sub = this.httpMenuService.setAppAndModule(this.applicSelected, this.moduleSelected, this.isThisPairDefault).subscribe((result) => {
            sub.unsubscribe();
        });
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
            list = this.applications;
            return list.indexOf(newName) !== -1;
        }

        list = this.getModulesBy(newName);
        return list.indexOf(newModName) !== -1;
    }

    ifHasToBe(mod) {
        return ((this.modules.length == 1) ||
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