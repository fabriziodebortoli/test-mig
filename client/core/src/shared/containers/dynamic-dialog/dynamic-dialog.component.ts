import { Component, OnInit, Type, Input, ViewChild, ViewContainerRef } from '@angular/core';

import { EventDataService } from './../../../core/services/eventdata.service';

@Component({
    selector: 'tb-dynamic-dialog',
    templateUrl: './dynamic-dialog.component.html',
    styleUrls: ['./dynamic-dialog.component.scss']
})
export class DynamicDialogComponent implements OnInit {

    opened = false;
    eventData: EventDataService; 
    @ViewChild('cmpContainer', { read: ViewContainerRef }) cmpContainer: ViewContainerRef;
    
    constructor() { }

    ngOnInit() { }

    open(eventData?: EventDataService) {
        this.eventData = eventData;
        this.opened = true;
    }

    close() {
  
    }

}
