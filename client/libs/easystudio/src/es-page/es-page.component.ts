import { ComponentService, DocumentComponent, EventDataService, LayoutService } from '@taskbuilder/core';
import { Component, OnInit, ComponentFactoryResolver, ChangeDetectorRef } from '@angular/core';
import { URLSearchParams, Http, Response } from '@angular/http';

import { ESService } from './../es.service';

@Component({
    selector: 'tb-es-page',
    templateUrl: './es-page.component.html',
    styleUrls: ['./es-page.component.scss'],
    providers: [ESService]
})
export class ESPageComponent extends DocumentComponent implements OnInit {

    constructor(
        public esService: ESService,
         eventData: EventDataService,
         changeDetectorRef:ChangeDetectorRef) {
        super(esService, eventData, null, changeDetectorRef);
    }

    ngOnInit() {
        super.ngOnInit();
        this.eventData.model = { 'Title': { 'value': "EasyStudio" } };
    }

}

@Component({
    template: ''
})
export class ESPageFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(ESPageComponent, resolver, { name: 'es' });
    }
} 