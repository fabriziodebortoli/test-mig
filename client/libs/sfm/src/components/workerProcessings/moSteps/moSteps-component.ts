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
    subsWorker: any;
    workerName: string;

    constructor(private coreService: CoreService,
        private processingsService: ProcessingsService) { }

    ngOnInit() {
        this.subsWorker = this.coreService.getWorker().subscribe(row => {
            this.worker = row;
            this.workerName = this.coreService.workerName;
        });
        this.subsProcessings = this.processingsService.getProcessings(filterType.mo_routing_step).subscribe(rows => {
            this.processingsList = rows;
        });
    }

    ngOnDestroy(): void {
        this.subsWorker.unsubscribe();
        this.subsProcessings.unsubscribe();
    }
}


