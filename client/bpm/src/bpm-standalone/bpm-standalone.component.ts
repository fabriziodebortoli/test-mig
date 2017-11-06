import { ComponentService, DocumentComponent, EventDataService, LayoutService } from '@taskbuilder/core';
import { Component, AfterContentInit, OnInit, ComponentFactoryResolver, HostListener } from '@angular/core';
import { URLSearchParams, Http, Response } from '@angular/http';

import { BPMService } from './../bpm.service';

@Component({
    selector: 'tb-bpm-standalone',
    templateUrl: './bpm-standalone.component.html',
    styleUrls: ['./bpm-standalone.component.scss'],
    providers: [BPMService]
})
export class BPMStandaloneComponent extends DocumentComponent implements OnInit, AfterContentInit {

    constructor(public bpmService: BPMService, eventData: EventDataService, public layoutService: LayoutService) {
        super(bpmService, eventData, null);
    }

    ngOnInit() {
        super.ngOnInit();
        this.eventData.model = { 'Title': { 'value': "BPM" } };
        this.calcViewHeight();
    }

    ngAfterContentInit() {
        setTimeout(() => this.calcViewHeight(), 0);
    }

    @HostListener('window:resize', ['$event'])
    onResize(event) {
        this.calcViewHeight();
    }
    calcViewHeight() {
        console.log("screen.height", screen.height);
        this.layoutService.setViewHeight(screen.height);
    }
}