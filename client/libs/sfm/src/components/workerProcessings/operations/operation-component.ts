import { ComponentService, DocumentComponent, EventDataService, LayoutService, DataService, InfoService } from '@taskbuilder/core';
import { Component, Input, OnInit, ComponentFactoryResolver, ChangeDetectorRef } from '@angular/core';

@Component({
    selector: 'operation',
    templateUrl: './operation-component.html',
    styleUrls: ['./operation-component.scss']
})

export class operationComponent implements OnInit {

    @Input() rec: any;
    @Input() backgroundColor: string = '#45526e'
    
    operation: string;
    operationImage: string;
    totalQty: number;
    status: string = "Unknown";
    producedQtyColor: string = "#929fba";
    workerColor: string = 'badge-secondary';

    setupTime: string = "Setup: ";
    processingTime: string = "Processing: ";

    constructor(private infoService: InfoService) {
    }

    ngOnInit() {

        this.operationImage = this.infoService.getUrlImage('Image.sfm.sfmcore.images.operations.png');
        this.operation = this.rec.MA_MOSteps_Operation + ' ' + this.rec.MA_Operations_Description;

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
                this.producedQtyColor = 'bg-success';
            else
                this.producedQtyColor = 'bg-warning';

            // if (+this.rec.MA_MO_ProductionQty > 0)
            //     this.percentage = this.totalQty * 100 / +this.rec.MA_MO_ProductionQty + "%";
        }
    }

    IsTimeEmpty(t: string) {
        return (t === null || +t === 0);  
    }
}


