import { ComponentService, DocumentComponent, EventDataService, LayoutService } from '@taskbuilder/core';
import { Component, OnInit, ComponentFactoryResolver, ChangeDetectorRef } from '@angular/core';
import { URLSearchParams, Http, Response } from '@angular/http';

import { ESPService } from './../esp.service';

@Component({
    selector: 'tb-esp-page',
    templateUrl: './esp-page.component.html',
    styleUrls: ['./esp-page.component.scss'],
    providers: [ESPService]
})
export class ESPPageComponent extends DocumentComponent implements OnInit {

    constructor(
        public espService: ESPService,
         eventData: EventDataService,
         changeDetectorRef:ChangeDetectorRef) {
        super(espService, eventData, null, changeDetectorRef);
    }

    ngOnInit() {
        super.ngOnInit();
        this.eventData.model = { 'Title': { 'value': "ESP" } };
    }

}

@Component({
    template: ''
})
export class ESPPageFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(ESPPageComponent, resolver, { name: 'esp' });
    }
} 