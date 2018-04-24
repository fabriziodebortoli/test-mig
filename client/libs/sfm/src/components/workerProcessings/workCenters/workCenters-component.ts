import { ComponentService, DocumentComponent, EventDataService, LayoutService, DataService } from '@taskbuilder/core';
import { Component, Input, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { CoreService } from './../../../core/sfm-core.service';
import { ProcessingsService, filterType } from './../../../core/sfm-processing.service';

@Component({
    selector: 'workCenters',
    templateUrl: './workCenters-component.html',
    styleUrls: ['./workCenters-component.scss']
})

export class workCentersComponent implements OnInit, OnDestroy {

    processingsList: any[] = [];
    subsProcessings: any;

    worker: any;
    subsWorker: any;

    constructor(private coreService: CoreService,
        private processingsService: ProcessingsService) { }

    ngOnInit() {
        this.subsWorker = this.coreService.getWorker().subscribe(row => {
            this.worker = row;
        });
        this.subsProcessings = this.processingsService.getProcessings(47, filterType.work_center).subscribe(rows => {
//            this.subsProcessings = this.processingsService.getProcessings(this.worker.RM_Workers_WorkerID, filterType.mo_routing_step).subscribe(rows => {
                this.processingsList = rows;
        });
    }

    ngOnDestroy() {
        this.subsWorker.unsubscribe();
        this.subsProcessings.unsubscribe();
    }
}



