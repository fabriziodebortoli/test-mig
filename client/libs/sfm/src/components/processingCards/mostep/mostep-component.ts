import { ComponentService, DocumentComponent, EventDataService, LayoutService, DataService } from '@taskbuilder/core';
import { Component, Input, OnInit, ComponentFactoryResolver, ChangeDetectorRef } from '@angular/core';

@Component({
    selector: 'mostep',
    templateUrl: './mostep-component.html',
    styleUrls: ['./mostep-component.scss']
})

export class mostepComponent implements OnInit {

    @Input() workerId: number = 47;
    @Input() rec: any;

    allWorkers: string = '';
    moNumber: string;
    item: string;
    totalQty: number;
    percentage: string;
    status: string = "Unknown";
    statusColor: string = "badge-light";
    progressBarColor: string = "bg-primary";
    producedQtyColor: string = "bg-secondary";
      
    ngOnInit() {
        if (+this.rec.SF_LabourAssignment_WorkerID === 0)
            this.allWorkers = 'All Workers';
        else
            this.allWorkers = 'Worker ' + this.rec.SF_LabourAssignment_WorkerID;

        this.moNumber = 'MO ' + this.rec.MA_MO_MONo + ' Routing Step ' +  this.rec.SF_LabourAssignment_RtgStep;
        if (Number(this.rec.SF_LabourAssignment_AltRtgStep) > 0)
        {
            if (this.rec.SF_LabourAssignment_Alternate === '')
                this.moNumber += ' [Alt. Routing Step ' + this.rec.SF_LabourAssignment_AltRtgStep + ']';
            else
                this.moNumber += ' [Alt. ' + this.rec.SF_LabourAssignment_Alternate + ' Routing Step ' + this.rec.SF_LabourAssignment_AltRtgStep + ']';
        }

        if (this.rec.MA_MO_Variant = '')
            this.item = this.rec.MA_MO_BOM;
        else
            this.item = this.rec.MA_MO_BOM + ' ' + this.rec.MA_MO_Variant;

        if (this.rec.MA_MOSteps_MOStatus === '20578304')
        {
            this.status = 'Released';
            this.statusColor = 'badge-primary';
        }
        else if (this.rec.MA_MOSteps_MOStatus === '20578305')
        {
            this.status = 'In Processing';
            this.statusColor = 'badge-warning';
        }

        this.totalQty = +this.rec.MA_MOSteps_ProducedQty + 
                        +this.rec.MA_MOSteps_SecondRateQuantity +
                        +this.rec.MA_MOSteps_ScrapQuantity;

        if (this.totalQty > 0)
        {
            if (this.totalQty >= +this.rec.MA_MO_ProductionQty)
                this.producedQtyColor = 'bg-success';
            else
                this.producedQtyColor = 'bg-warning';

            if (+this.rec.MA_MO_ProductionQty > 0)
                this.percentage = this.totalQty * 100 / +this.rec.MA_MO_ProductionQty + "%";
        }
    }
 }

