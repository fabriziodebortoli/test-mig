import { ComponentService, DocumentComponent, EventDataService, LayoutService, DataService, InfoService } from '@taskbuilder/core';
import { Component, Input, OnInit, ComponentFactoryResolver, ChangeDetectorRef } from '@angular/core';
import { URLSearchParams, Http } from '@angular/http';

import { SFMService } from './../sfm.service';
import { CoreService } from './../core/sfm-core.service';

export enum filterType {
    mo_routing_step = 0,
    mo = 1,
    work_center = 2,
    operation = 3,
    sale_order = 4,
    job = 5,
    customer = 6
}

@Component({
    selector: 'tb-sfm-page',
    templateUrl: './sfm-page.component.html',
    styleUrls: ['./sfm-page.component.scss'],
    providers: [SFMService]
})
export class SFMPageComponent extends DocumentComponent implements OnInit  {
    public isCollapsed = false;

    worker: any;

    manufacturingParameters: any;

    workerName: string;
    workerImage: string;

    private _sidebarSize: string = localStorage.getItem('sidebarSize') || '25%';
  
    constructor(
        public sfmService: SFMService,
        eventData: EventDataService,
        private dataService: DataService,
        changeDetectorRef: ChangeDetectorRef,
        private coreService: CoreService,
        private infoService: InfoService) {
        super(sfmService, eventData, null, changeDetectorRef);
    }

    async ngOnInit() {
        this.worker = await this.coreService.getWorker();
        this.workerName = this.coreService.workerName;
        this.workerImage = this.infoService.getUrlImage(this.coreService.workerImage);
        this.manufacturingParameters = this.coreService.getManufacturingParameters();
    }

    public get sidebarSize(): string {
        return this._sidebarSize;
    }
    public set sidebarSize(newSize: string) {
        this._sidebarSize = newSize;
        localStorage.setItem('sidebarSize', newSize);
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



