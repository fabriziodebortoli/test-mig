import { ComponentService, DocumentComponent, EventDataService, LayoutService } from '@taskbuilder/core';
import { Component, OnInit, ComponentFactoryResolver, ChangeDetectorRef } from '@angular/core';
import { URLSearchParams, Http } from '@angular/http';

import { SFMService } from './../sfm.service';

@Component({
    selector: 'tb-sfm-page',
    templateUrl: './sfm-page.component.html',
    styleUrls: ['./sfm-page.component.scss'],
    providers: [SFMService]
})
export class SFMPageComponent extends DocumentComponent implements OnInit {

    constructor(
        public sfmService: SFMService,
         eventData: EventDataService,
         changeDetectorRef:ChangeDetectorRef) {
        super(sfmService, eventData, null, changeDetectorRef);
    }

    ngOnInit() {
        super.ngOnInit();
        this.eventData.model = { 'Title': { 'value': "SFM" } };
    }

}

@Component({
    template: ''
})
export class SFMPageFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(SFMPageComponent, resolver, { name: 'sfm' });
    }
} 