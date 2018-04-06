import { ComponentService, EventDataService, LayoutService, ComponentInfoService } from '@taskbuilder/core';
import { Component, AfterContentInit, OnInit, ComponentFactoryResolver, HostListener } from '@angular/core';
import { URLSearchParams, Http, Response } from '@angular/http';

import { ESService } from './../es.service';

@Component({
    selector: 'tb-es-standalone',
    templateUrl: './es-standalone.component.html',
    styleUrls: ['./es-standalone.component.scss'],
    providers: [ESService, ComponentInfoService]
})
export class ESStandaloneComponent implements OnInit, AfterContentInit {

    constructor(public esService: ESService, eventData: EventDataService, public layoutService: LayoutService) {
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