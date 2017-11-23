import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_GANTT_STEPService } from './IDD_GANTT_STEP.service';

@Component({
    selector: 'tb-IDD_GANTT_STEP',
    templateUrl: './IDD_GANTT_STEP.component.html',
    providers: [IDD_GANTT_STEPService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_GANTT_STEPComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_GANTT_STEPService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        changeDetectorRef: ChangeDetectorRef) {
		super(document, eventData, ciService, changeDetectorRef, resolver);
        this.eventData.change.subscribe(() => this.changeDetectorRef.detectChanges());
    }

    ngOnInit() {
        super.ngOnInit();
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['nHorizon','bAlsoPurchases','HFMO_From','HFMO_To','HFMO_All','HFMO_Range','HFMO_From','HFMOStatus_Created','HFMOStatus_Released','HFMOStatus_Processing','HFMODeliveryDate_All','HFMODeliveryDate_Range','HFMODeliveryDate_From','HFMODeliveryDate_To','HFItem_MakeOrBuy','HFItem_All','HFItem_Range','HFItem_From','HFItem_To','HFItem_Status','HFVariant_All','HFVariant_Range','HFVariant_From','HFVariant_To','HFCustomer_From','HFCustomer_To','HFCustomer_All','HFCustomer_Range','HFCustomer_From','HFCostCenters_All','HFCostCenters_Range','HFCostCenters_From','HFCostCenters_To','HFMRPJob_AlsoEmpty','HFMRPJob_All','HFMRPJob_Range','HFMRPJob_From','HFMRPJob_To','MOGanttSelect','RtgStepsGantt','Created','Processing','Released','Confirmed'],'MOGanttSelect':['TMO_Selection','MONo','BOM','Variant','TMO_BOMDescri','RunDate','DeliveryDate','ProductionQty','MOStatus','Job','Customer','InternalOrdNo'],'RtgStepsGantt':['Selection','MONo','DocType','NewSimulationsStartingDate','NewSimulationsEndingDate','SimStartDate','SimStartDate','NewWC','EstimatedWC','RtgStep','Alternate','AltRtgStep','MOStatus','BOM','Variant','BOMDescri','Job','Customer','SaleOrdNo','StartingDate','EndingDate']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_GANTT_STEPFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_GANTT_STEPComponent, resolver);
    }
} 