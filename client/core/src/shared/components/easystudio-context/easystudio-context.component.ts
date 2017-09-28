import { ApplicationDateComponent } from './../../controls/application-date/application-date.component';
import { ButtonComponent } from './../../controls/button/button.component';
import { Component, Input, ViewChild } from '@angular/core';
import { Button } from '@progress/kendo-angular-buttons';
import { Collision } from '@progress/kendo-angular-popup/dist/es/models/collision.interface';
import { Align } from '@progress/kendo-angular-popup/dist/es/models/align.interface';
import { HttpService } from './../../../core/services/http.service';

@Component({
    selector: 'tb-es-context',
    templateUrl: './easystudio-context.component.html'
})
export class EasyStudioContextComponent {

    isEasyStudioActivated = true;
    opened: boolean = false;
    uniqueJson: string;
    applications: [any];
    modules: any;
    memory: [string];

    constructor(private httpService: HttpService) {
        this.uniqueJson = this.httpService.getEsAppsAndModules();
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
        let y: [any] = JSON.parse(this.uniqueJson);
      //  this.applications.push(y[0][0]);
        
    }





}