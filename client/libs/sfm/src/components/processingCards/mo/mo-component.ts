import { ComponentService, DocumentComponent, EventDataService, LayoutService, DataService } from '@taskbuilder/core';
import { Component, Input, OnInit, ComponentFactoryResolver, ChangeDetectorRef } from '@angular/core';

@Component({
    selector: 'mo',
    templateUrl: './mo-component.html',
    styleUrls: ['./mo-component.scss']
})

export class moComponent implements OnInit {

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
    
    startDate: string;
    endDate: string;
    setupTime: string;
    processingTime: string;

    ngOnInit() {
        if (+this.rec.SF_LabourAssignment_WorkerID === 0)
            this.allWorkers = 'All Workers';
        else
            this.allWorkers = 'Worker ' + this.rec.SF_LabourAssignment_WorkerID;
                    
        this.moNumber = 'MO ' + this.rec.MA_MO_MONo;

        if (this.rec.MA_MO_Variant = '')
            this.item = this.rec.MA_MO_BOM;
        else
            this.item = this.rec.MA_MO_BOM + ' ' + this.rec.MA_MO_Variant;

        if (this.rec.MA_MO_MOStatus === '20578304')
        {
            this.status = 'Released';
            this.statusColor = 'badge-primary';
        }
        else if (this.rec.MA_MO_MOStatus === '20578305')
        {
            this.status = 'In Processing';
            this.statusColor = 'badge-warning';
        }

        this.totalQty = +this.rec.MA_MO_ProducedQty + 
                        +this.rec.MA_MO_SecondRateQuantity +
                        +this.rec.MA_MO_ScrapQuantity;
        
        if (this.totalQty > 0)
        {
            if (this.totalQty >= +this.rec.MA_MO_ProductionQty)
                this.producedQtyColor = 'bg-success';
            else
                this.producedQtyColor = 'bg-warning';

            if (+this.rec.MA_MO_ProductionQty > 0)
                this.percentage = this.totalQty * 100 / +this.rec.MA_MO_ProductionQty + "%";
        }

        if (this.rec.MA_MO_StartingDate != '1799-12-31')
            this.startDate += "Start: " + this.rec.MA_MO_StartingDate;

        if (this.rec.MA_MO_EndingDate != '1799-12-31')
            this.endDate += "End: " + this.rec.MA_MO_EndingDate;

        if (+this.rec.MA_MO_ActualSetupTime > 0)
            this.setupTime += "Setup: " + this.rec.MA_MO_ActualSetupTime;

        if (+this.rec.MA_MO_ActualProcessingTime > 0)
            this.setupTime += "Processing: " + this.rec.MA_MO_ActualProcessingTime;
    }
 }

