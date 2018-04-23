import { ComponentService, DocumentComponent, EventDataService, LayoutService, DataService } from '@taskbuilder/core';
import { Component, Input, OnInit, ComponentFactoryResolver, ChangeDetectorRef } from '@angular/core';

@Component({
    selector: 'moStep',
    templateUrl: './mostep-component.html',
    styleUrls: ['./mostep-component.scss']
})

export class moStepComponent implements OnInit {

    @Input() workerName: string;
    @Input() rec: any;

    allWorkers: string;
    dueDate: string;
    moNumber: string;
    item: string;
    itemImage: string;
    totalQty: number;
    status: string = 'Unknown';
    statusColor: string = '#000000';
    producedQtyColor: string = '#929fba';
    workerColor: string = '#33b5e5';

    startDate: string = "Start: ";
    endDate: string = "End: ";
    setupTime: string = "Setup: ";
    processingTime: string = "Processing: ";

    ngOnInit() {

        this.itemImage = 'http://www.deightonmanufacturing.co.uk/wp-content/uploads/2014/01/R-Series.png';
        
        // BOM
        if (this.rec.MA_MO_Variant === '')
            this.item = this.rec.MA_MO_BOM;
        else
            this.item = this.rec.MA_MO_BOM + ' [' + this.rec.MA_MO_Variant + ']';

        this.item += ' ' + this.rec.MA_Items_BOMDescription;
            
        // All workers
         if (+this.rec.SFM_ProcessingAssignment_WorkerID === 0)
             this.allWorkers = 'All Workers';

        // Due date
        if (!this.IsDateEmpty(this.rec.SFM_ProcessingAssignment_DueDate))
            this.dueDate = 'Due Date: ' + this.rec.SFM_ProcessingAssignment_DueDate;

        // MO Step
        this.moNumber = 'MO ' + this.rec.MA_MO_MONo + ' Routing Step ' +  this.rec.SFM_ProcessingAssignment_RtgStep;
        if (Number(this.rec.SFM_ProcessingAssignment_AltRtgStep) > 0)
        {
            if (this.rec.SFM_ProcessingAssignment_Alternate === '')
                this.moNumber += ' [Alternative Routing Step ' + this.rec.SFM_ProcessingAssignment_AltRtgStep + ']';
            else
                this.moNumber += ' [Alternative ' + this.rec.SFM_ProcessingAssignment_Alternate + ' Routing Step ' + this.rec.SFM_ProcessingAssignment_AltRtgStep + ']';
        }

        if (+this.rec.MA_MOSteps_MOStatus === 20578304)
        {
            this.status = 'Released';
            this.statusColor = '#33b5e5';
        }
        else if (+this.rec.MA_MOSteps_MOStatus === 20578305)
        {
            this.status = 'In Processing';
            this.statusColor = '#ffbb33';
        }
        else
            console.log(this.rec.MA_MOSteps_MOStatus);

        if (!this.IsTimeEmpty(this.rec.MA_MOSteps_ActualSetupTime))
            this.setupTime += this.rec.MA_MOSteps_ActualSetupTime;

        if (!this.IsTimeEmpty(this.rec.MA_MOSteps_ActualProcessingTime))
            this.processingTime += this.rec.MA_MOSteps_ActualProcessingTime;   
            
        this.totalQty = +this.rec.MA_MOSteps_ProducedQty + 
                        +this.rec.MA_MOSteps_SecondRateQuantity +
                        +this.rec.MA_MOSteps_ScrapQuantity;

        if (this.totalQty > 0)
        {
            if (this.totalQty >= +this.rec.MA_MO_ProductionQty)
                this.producedQtyColor = '#00C851';
            else
                this.producedQtyColor = '#ffbb33';
        }
    }

    IsDateEmpty(d: string) {
        return (d === null || d === '1799-12-31');  
    }

    IsTimeEmpty(t: string) {
        return (t === null || +t === 0);  
    }
}


