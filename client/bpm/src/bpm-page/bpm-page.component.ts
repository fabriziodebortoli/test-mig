import { ComponentService, DocumentComponent, EventDataService, LayoutService } from '@taskbuilder/core';
import { Component, OnInit, ComponentFactoryResolver, ChangeDetectorRef } from '@angular/core';
import { URLSearchParams, Http, Response } from '@angular/http';

import { BPMService } from './../bpm.service';

@Component({
    selector: 'tb-bpm-page',
    templateUrl: './bpm-page.component.html',
    styleUrls: ['./bpm-page.component.scss'],
    providers: [BPMService]
})
export class BPMPageComponent extends DocumentComponent implements OnInit {

    constructor(
        public bpmService: BPMService,
         eventData: EventDataService,
         changeDetectorRef:ChangeDetectorRef) {
        super(bpmService, eventData, null, changeDetectorRef);
    }

    ngOnInit() {
        super.ngOnInit();
        this.eventData.model = { 'Title': { 'value': "BPM" } };
    }

}

@Component({
    template: ''
})
export class BPMPageFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(BPMPageComponent, resolver, { name: 'bpm' });
    }
} 