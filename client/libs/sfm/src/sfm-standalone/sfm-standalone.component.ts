import { ComponentService, EventDataService, LayoutService, ComponentInfoService } from '@taskbuilder/core';
import { Component, AfterContentInit, OnInit, ComponentFactoryResolver, HostListener } from '@angular/core';
import { URLSearchParams, Http } from '@angular/http';

import { SFMService } from './../sfm.service';

@Component({
    selector: 'tb-sfm-standalone',
    templateUrl: './sfm-standalone.component.html',
    styleUrls: ['./sfm-standalone.component.scss'],
    providers: [SFMService, ComponentInfoService]
})
export class SFMStandaloneComponent implements OnInit, AfterContentInit {

    constructor(public sfmService: SFMService, eventData: EventDataService, public layoutService: LayoutService) {
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