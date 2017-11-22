import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_SALE_ORDER_ALLOCATION_DETAILService } from './IDD_SALE_ORDER_ALLOCATION_DETAIL.service';

@Component({
    selector: 'tb-IDD_SALE_ORDER_ALLOCATION_DETAIL',
    templateUrl: './IDD_SALE_ORDER_ALLOCATION_DETAIL.component.html',
    providers: [IDD_SALE_ORDER_ALLOCATION_DETAILService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_SALE_ORDER_ALLOCATION_DETAILComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_SALE_ORDER_ALLOCATION_DETAILService,
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
		boService.appendToModelStructure({'global':['SaleOrdNo','SaleOrdersAllocation'],'SaleOrdersAllocation':['StatusBmp','IsSelected','Item','ItemDescri','UoM','Lot','Qty','AllocableQty','AllocatedQty','DeliveredQty','BaseUoM','AvailableQty','ProgressiveAvailableQty','AreaQty','AllocationArea','ConfirmedDeliveryDate','InternalOrdNo','OrderDate','ExpectedDeliveryDate']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_SALE_ORDER_ALLOCATION_DETAILFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_SALE_ORDER_ALLOCATION_DETAILComponent, resolver);
    }
} 