import { ComponentService, DocumentComponent, EventDataService, LayoutService, DataService } from '@taskbuilder/core';
import { Component, Input, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { CoreService } from './../../../core/sfm-core.service';
import { ProcessingsService, filterType } from './../../../core/sfm-processing.service';

@Component({
    selector: 'moSteps',
    templateUrl: './moSteps-component.html',
    styleUrls: ['./moSteps-component.scss']
})

export class moStepsComponent implements OnInit, OnDestroy {

    processingsList: any[] = [];
    subsProcessings: any;

    worker: any;
    workerName: string;

    constructor(private coreService: CoreService,
        private processingsService: ProcessingsService) { }

    async ngOnInit() {
        this.worker = await this.coreService.getWorker();
        this.subsProcessings = this.processingsService.getProcessings(this.worker.RM_Workers_WorkerID, filterType.mo_routing_step).subscribe(rows => {
            this.processingsList = rows;
        });
    }

    ngOnDestroy() {
        this.subsProcessings.unsubscribe();
    }
}


