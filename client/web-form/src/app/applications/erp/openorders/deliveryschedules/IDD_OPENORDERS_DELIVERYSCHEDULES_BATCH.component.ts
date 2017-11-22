import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_OPENORDERS_DELIVERYSCHEDULES_BATCHService } from './IDD_OPENORDERS_DELIVERYSCHEDULES_BATCH.service';

@Component({
    selector: 'tb-IDD_OPENORDERS_DELIVERYSCHEDULES_BATCH',
    templateUrl: './IDD_OPENORDERS_DELIVERYSCHEDULES_BATCH.component.html',
    providers: [IDD_OPENORDERS_DELIVERYSCHEDULES_BATCHService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_OPENORDERS_DELIVERYSCHEDULES_BATCHComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_OPENORDERS_DELIVERYSCHEDULES_UOM_itemSource: any;

    constructor(document: IDD_OPENORDERS_DELIVERYSCHEDULES_BATCHService,
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
        this.IDC_OPENORDERS_DELIVERYSCHEDULES_UOM_itemSource = {
  "name": "UnitsOfMeasureDeliveryScheduleComboBox",
  "namespace": "ERP.OpenOrders.Documents.UnitsOfMeasureDeliveryScheduleComboBox"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['Contract','HFItems_All','HFItems_Range','HFItems_From','HFItems_To','DeliverySchedules','DailyDelivery'],'HKLCustContractsByType':['Description'],'DeliverySchedules':['BitmapBasket','Item','ItemDescr','UoM','Address','DeliveryPlaceDescr'],'DailyDelivery':['DeliveryDate','DayDate','DailyQty','Notes']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_OPENORDERS_DELIVERYSCHEDULES_BATCHFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_OPENORDERS_DELIVERYSCHEDULES_BATCHComponent, resolver);
    }
} 