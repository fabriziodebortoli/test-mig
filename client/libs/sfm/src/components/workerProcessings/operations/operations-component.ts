import { ComponentService, DocumentComponent, EventDataService, LayoutService, DataService } from '@taskbuilder/core';
import { Component, Input, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectorRef } from '@angular/core';
import { ActivatedRoute, Params, Router } from '@angular/router';
import { CoreService } from './../../../core/sfm-core.service';
import { ProcessingsService, filterType } from './../../../core/sfm-processing.service';

@Component({
    selector: 'operations',
    templateUrl: './operations-component.html',
    styleUrls: ['./operations-component.scss']
})

export class operationsComponent implements OnInit, OnDestroy {

    processingsList: any[] = [];
    subsProcessings: any;

    constructor(private coreService: CoreService,
        private processingsService: ProcessingsService) { }

    ngOnInit() {
        this.subsProcessings = this.processingsService.getProcessings(filterType.operation).subscribe(rows => {
            this.processingsList = rows;
        });
    }

    ngOnDestroy(): void {
        this.subsProcessings.unsubscribe();
    }
}


