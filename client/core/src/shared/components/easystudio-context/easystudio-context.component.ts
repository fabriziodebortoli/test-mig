import { LayoutModule, PanelBarExpandMode } from '@progress/kendo-angular-layout';
import { ApplicationDateComponent } from './../../controls/application-date/application-date.component';
import { ButtonComponent } from './../../controls/button/button.component';
import { Component, Input, ViewChild } from '@angular/core';
import { Button } from '@progress/kendo-angular-buttons';
import { Collision } from '@progress/kendo-angular-popup/dist/es/models/collision.interface';
import { Align } from '@progress/kendo-angular-popup/dist/es/models/align.interface';
import { HttpService } from './../../../core/services/http.service';


interface MyObj {
    application: string
    module: string
  }
@Component({
    selector: 'tb-es-context',
    templateUrl: './easystudio-context2.component.html',
    styleUrls: ['./easystudio-context.component.scss']
})


export class EasyStudioContextComponent {

    public expandMode: number = PanelBarExpandMode.Multiple;
    isEasyStudioActivated = true;
    opened: boolean = false;
    addModuleVisible = false;
    uniqueJson: string;
    applications: any[] = new Array();
    modules: any;
    memory :{ allApplications: MyObj[] }; 
    applicSelected: string;
    moduleSelected: string;

    constructor(public httpService: HttpService) {
        this.uniqueJson = this.httpService.getEsAppsAndModules();
        this.memory = JSON.parse(this.uniqueJson);
        this.extractNames();
    }

    changeCustomizationContext() {
        this.opened = !this.opened;
    }

    public close() {
        this.opened = false;
    }

    public open() {
        this.opened = true;
    }

    public ok() {
        this.opened = false;
    }

    private extractNames() {
        let y = this.memory;
        for (var index = 0; index < y.allApplications.length; index++) {
            var element = y.allApplications[index].application;
            if(this.applications.indexOf(element)=== -1)
                this.applications.push(element);      
        }        
    }

    private getModulesBy(app: string){
        let y = this.memory;
        let modules: any[] = new Array();
        for (var index = 0; index < y.allApplications.length; index++) {
            var element = y.allApplications[index].application;
            if(element !== app)
                continue;
            modules.push(y.allApplications[index].module);    
        } 
        return modules;
    }

    private setApplic(app: string){
        this.applicSelected = app;
        this.moduleSelected = undefined;
        this.modules = this.getModulesBy(app);
        this.hightlightApp(app);
    }

    private setModule(mod: string) {
        this.moduleSelected = mod;
        this.hightlightApp(mod);
    }

    hightlightApp(item: any){
        
    }




}