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
    cmpRef: ComponentRef<DocumentComponent>;

    opened = false;
    componentInfo: ComponentInfo;
    document: DocumentService; 
    eventData: EventDataService;
    cmpContainer: ViewContainerRef;

    @ViewChild('cmpContainer', { read: ViewContainerRef }) set content(content: ViewContainerRef) {
        this.cmpContainer = content;
        if (this.cmpContainer) {
            const me = this;
             setTimeout(function () {
                 //TODO fare nella ngAfterViewChecked, ma bisogna updradare angular
                 me.cmpRef = me.cmpContainer.createComponent(me.componentInfo.factory);
                 me.cmpRef.instance.cmpId = me.componentInfo.id; //assegno l'id al componente
 
                 //documento ed eventi sono condivisi col componente master
                 me.cmpRef.instance.document = me.document;
                 me.cmpRef.instance.eventData = me.eventData;
             }, 1);
        }
    }

    constructor() { }

    open(componentInfo: ComponentInfo, document: DocumentService, eventData: EventDataService) {
        this.componentInfo = componentInfo;
        this.document = document;
        this.eventData = eventData;

        this.opened = true;
    }

    close() {
        this.opened = false;
        this.cmpRef = null;
        this.componentInfo = null;
    }

}
