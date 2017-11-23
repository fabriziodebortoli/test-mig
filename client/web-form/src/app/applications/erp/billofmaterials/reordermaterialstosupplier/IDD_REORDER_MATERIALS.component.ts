import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_REORDER_MATERIALSService } from './IDD_REORDER_MATERIALS.service';

@Component({
    selector: 'tb-IDD_REORDER_MATERIALS',
    templateUrl: './IDD_REORDER_MATERIALS.component.html',
    providers: [IDD_REORDER_MATERIALSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_REORDER_MATERIALSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_REORDER_MATERIALSService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        changeDetectorRef: ChangeDetectorRef) {
		super(document, eventData, ciService, changeDetectorRef, resolver);
        this.subscriptions.push(this.eventData.change.subscribe(() => changeDetectorRef.detectChanges()));
    }

    ngOnInit() {
        super.ngOnInit();
        
        		this.bo.appendToModelStructure({'global':['NrPlan','bSplitForJob','bSplitForCostCnt','bSplitForSaleOrder','ReorderMaterialsToSupplier'],'HKLPlan':['Description'],'ReorderMaterialsToSupplier':['PrepareSubcntOrd','CodeType','Component','ComponentDescription','UoM','RequProductionPlanQty','RequInducedRequirements','RequReservedQty','OnHandStorageQty','RequIncomingQty','RequMinimumStock','RequQtyToOrder','RequAvailaibility','Supplier','MinOrderQty','RequReorderingLotSize','Job','CostCenter','SaleOrdNo']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_REORDER_MATERIALSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_REORDER_MATERIALSComponent, resolver);
    }
} 