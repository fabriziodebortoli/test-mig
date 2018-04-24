import { ComponentService, DocumentComponent, EventDataService, LayoutService, DataService } from '@taskbuilder/core';
import { Component, Input, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectorRef } from '@angular/core';
import { URLSearchParams, Http } from '@angular/http';
import { ActivatedRoute, Router } from '@angular/router';

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
export class SFMPageComponent extends DocumentComponent implements OnInit, OnDestroy {
    public isCollapsed = false;

    worker: any;
    subsWorker: any;

    manufacturingParameters: any;
    subsManufacturingParameters: any;

    workerName: string;
    workerImage: string;
    today: Date = new Date();

    constructor(
        public sfmService: SFMService,
        eventData: EventDataService,
        private dataService: DataService,
        changeDetectorRef: ChangeDetectorRef,
        private route: ActivatedRoute,
        private router: Router,
        private coreService: CoreService) {
        super(sfmService, eventData, null, changeDetectorRef);
    }

    ngOnInit() {
        this.subsWorker = this.coreService.getWorker().subscribe(row => {
            this.worker = row;
            this.workerName = this.coreService.workerName;
            this.workerImage = "http://www.m-d.it/wp-content/uploads/2015/04/allontanamento-colombi-piccioni1.jpg";
        });

        this.subsManufacturingParameters = this.coreService.getManufacturingParameters().subscribe(row => {
            this.manufacturingParameters = row;
        });

//       this.onMORoutingStep();
    }
    
    ngOnDestroy() {
        this.subsWorker.unsubscribe();
    }

    onWorkCenter() {
        this.router.navigate(['/workCenters']);
    }
    
    onOperation() {
        this.router.navigate(['/operations']);
    }
    
    onMORoutingStep() {
        this.router.navigate(['/moSteps']);
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



