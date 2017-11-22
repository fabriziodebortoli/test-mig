import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_GANTT_MOService } from './IDD_GANTT_MO.service';

@Component({
    selector: 'tb-IDD_GANTT_MO',
    templateUrl: './IDD_GANTT_MO.component.html',
    providers: [IDD_GANTT_MOService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_GANTT_MOComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_GANTT_MOService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        private changeDetectorRef: ChangeDetectorRef) {
        super(document, eventData, resolver, ciService);
        this.eventData.change.subscribe(() => this.changeDetectorRef.detectChanges());
    }

    ngOnInit() {
        super.ngOnInit();
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['nHorizon','bAlsoPurchases','HFMO_From','HFMO_To','HFMO_All','HFMO_Range','HFMO_From','HFMOStatus_Created','HFMOStatus_Released','HFMOStatus_Processing','HFMODeliveryDate_All','HFMODeliveryDate_Range','HFMODeliveryDate_From','HFMODeliveryDate_To','HFItem_MakeOrBuy','HFItem_All','HFItem_Range','HFItem_From','HFItem_To','HFItem_Status','HFVariant_All','HFVariant_Range','HFVariant_From','HFVariant_To','HFCustomer_From','HFCustomer_To','HFCustomer_All','HFCustomer_Range','HFCustomer_From','HFMRPJob_AlsoEmpty','HFMRPJob_All','HFMRPJob_Range','HFMRPJob_From','HFMRPJob_To','HFCostCenters_All','HFCostCenters_Range','HFCostCenters_From','HFCostCenters_To','MOGanttSelect','MOGanttList','Created','Processing','Released','Confirmed'],'MOGanttSelect':['TMO_Selection','MONo','BOM','Variant','TMO_BOMDescri','RunDate','DeliveryDate','ProductionQty','MOStatus','Job','Customer','InternalOrdNo'],'MOGanttList':['TMO_SelectedMode','TMO_DocType','MONo','BOM','TMO_NewSimulationsStartingDate','SimStartDate','SimEndDate','Variant','TMO_BOMDescri','RunDate','DeliveryDate','ProductionQty','MOStatus','Job','Customer','InternalOrdNo']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_GANTT_MOFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_GANTT_MOComponent, resolver);
    }
} 