import { ComponentService, EventDataService, LayoutService, ComponentInfoService } from '@taskbuilder/core';
import { Component, AfterContentInit, OnInit, ComponentFactoryResolver, HostListener } from '@angular/core';
import { URLSearchParams, Http, Response } from '@angular/http';

import { ESPService } from './../esp.service';

@Component({
    selector: 'tb-esp-standalone',
    templateUrl: './esp-standalone.component.html',
    styleUrls: ['./esp-standalone.component.scss'],
    providers: [ESPService, ComponentInfoService]
})
export class ESPStandaloneComponent implements OnInit, AfterContentInit {

    constructor(public espService: ESPService, eventData: EventDataService, public layoutService: LayoutService) {
    }

    ngOnInit() {
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