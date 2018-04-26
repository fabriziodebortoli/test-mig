import { ComponentService, DocumentComponent, EventDataService, LayoutService, DataService, InfoService } from '@taskbuilder/core';
import { Component, Input, OnInit, ComponentFactoryResolver, ChangeDetectorRef } from '@angular/core';

@Component({
    selector: 'workCenter',
    templateUrl: './workCenter-component.html',
    styleUrls: ['./workCenter-component.scss']
})

export class workCenterComponent implements OnInit {
    @Input() rec: any;
    @Input() backgroundColor: string = '#45526e';
    
    workCenter: string;
    workCenterImage: string;
    totalQty: number;
    producedQtyColor: string = "#929fba";

    setupTime: string = "Setup: ";
    processingTime: string = "Processing: ";

    constructor(private infoService: InfoService) { }

    ngOnInit() {

        this.workCenterImage = this.infoService.getUrlImage('Image.sfm.sfmcore.images.work-centers.png');
        this.workCenter = this.rec.MA_MOSteps_WC + ' ' + this.rec.MA_WorkCenters_Description;

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


