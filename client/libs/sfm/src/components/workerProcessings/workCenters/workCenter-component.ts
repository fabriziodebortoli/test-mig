import { ComponentService, DocumentComponent, EventDataService, LayoutService, DataService } from '@taskbuilder/core';
import { Component, Input, OnInit, ComponentFactoryResolver, ChangeDetectorRef } from '@angular/core';

@Component({
    selector: 'workCenter',
    templateUrl: './workCenter-component.html',
    styleUrls: ['./workCenter-component.scss']
})

export class workCenterComponent implements OnInit {
    @Input() rec: any;

    workCenter: string;
    workCenterImage: string;
    totalQty: number;
    producedQtyColor: string = "#929fba";

    setupTime: string = "Setup: ";
    processingTime: string = "Processing: ";

    ngOnInit() {
        this.workCenter = this.rec.MA_MOSteps_WC + ' ' + this.rec.MA_WorkCenters_Description;
        this.workCenterImage = 'https://cdn4.iconfinder.com/data/icons/free-large-android-icons/512/Industrial_robot_with_shadow.png';

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

    IsTimeEmpty(t: string) {
        return (t === null || +t === 0);  
    }
}


