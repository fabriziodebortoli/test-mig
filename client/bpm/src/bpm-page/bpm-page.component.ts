import { ComponentService, DocumentComponent, EventDataService } from '@taskbuilder/core';
import { Component, OnInit, ComponentFactoryResolver } from '@angular/core';
import { URLSearchParams, Http, Response } from '@angular/http';

import { BPMService } from './../bpm.service';

@Component({
    selector: 'tb-bpm-page',
    templateUrl: './bpm-page.component.html',
    styleUrls: ['./bpm-page.component.scss'],
    providers: [BPMService]
})
export class BPMPageComponent extends DocumentComponent implements OnInit {

    constructor(public bpmService: BPMService, eventData: EventDataService) {
        super(bpmService, eventData, null);
    }

    ngOnInit() { }

}

@Component({
    template: ''
})
export class BPMPageFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(BPMPageComponent, resolver, { name: 'bpm' });
    }
} 