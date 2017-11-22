import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_SALE_ORD_DEALLOCATIONService } from './IDD_SALE_ORD_DEALLOCATION.service';

@Component({
    selector: 'tb-IDD_SALE_ORD_DEALLOCATION',
    templateUrl: './IDD_SALE_ORD_DEALLOCATION.component.html',
    providers: [IDD_SALE_ORD_DEALLOCATIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_SALE_ORD_DEALLOCATIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_SALE_ORD_DEALLOCATIONService,
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
		boService.appendToModelStructure({'global':['bAllCustomer','bCustomerSel','CustomerStart','CustomerEnd','bAllOrdNo','bOrdNoSel','FromOrdNo','ToOrdNo','FromDate','EndingDate','FromExpectedDeliveryDate','EndingExpectedDeliveryDate','AllPriority','PrioritySel','FromPriority','ToPriority','bAllAllocationArea','bAllocationAreaSel','AllocationAreaStart','AllocationAreaEnd','bAllStorage','bStorageSel','Storage','bStorageSel','Storage','SpecificatorType','Specificator','bAllItem','bItemSel','ItemStart','ItemEnd','SaleOrdersDeallocation'],'SaleOrdersDeallocation':['StatusBmp','IsSelected','Item','ItemDescri','UoM','Qty','AllocatedQty','DeliveredQty','QtyToDeallocate','AllocationArea','ConfirmedDeliveryDate','InternalOrdNo','OrderDate','ExpectedDeliveryDate','StoragePhase1','Specificator1Type','SpecificatorPhase1','Customer','CustomerDescri','Note']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_SALE_ORD_DEALLOCATIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_SALE_ORD_DEALLOCATIONComponent, resolver);
    }
} 