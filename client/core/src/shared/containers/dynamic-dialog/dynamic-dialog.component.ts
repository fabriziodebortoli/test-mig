import { EventDataService } from './../../../core/services/eventdata.service';
import { DocumentComponent } from './../../components/document.component';
import { DocumentService } from './../../../core/services/document.service';
import { ComponentInfo } from './../../models/component-info.model';
import { Component, OnInit, Type, Input, ViewChild, ViewContainerRef, ComponentRef, OnDestroy  } from '@angular/core';

@Component({
    selector: 'tb-dynamic-dialog',
    templateUrl: './dynamic-dialog.component.html',
    styleUrls: ['./dynamic-dialog.component.scss'] 
})
export class DynamicDialogComponent { 

    opened = false;
    componentInfo: ComponentInfo;
    

    constructor() { }

    open(componentInfo: ComponentInfo) {
        this.componentInfo = componentInfo;
        this.opened = true;
    }

    close() {
        this.opened = false;
        this.componentInfo = null;
    }

}
