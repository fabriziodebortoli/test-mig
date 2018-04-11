import { ComponentService, DocumentComponent, EventDataService, LayoutService, DataService } from '@taskbuilder/core';
import { Component, Input, OnInit, OnAfterViewInit, ComponentFactoryResolver, ChangeDetectorRef } from '@angular/core';

@Component({
    selector: 'mostep-component',
    templateUrl: './mostep-component.html',
    styleUrls: ['./mostep-component.scss']
})

export class MOStepComponent implements OnInit {

    @Input() workerId: number;
    @Input() rec: any;

    status: string = "Unknown";
    statusColor: string = "badge-light";
    progressBarColor: string = "bg-success";
    
    ngOnInit() {
    }

    ngOnAfterViewInit() {
        if (this.rec.MA_MOSteps_MOStatus === '20578304')
        {
            this.status = "Released";
            this.statusColor = "badge-primary";
        }
        else if (this.rec.MA_MOSteps_MOStatus === '20578305')
        {
            this.status = "In processing";
            this.statusColor = "badge-warning";
        }
    }
 }

