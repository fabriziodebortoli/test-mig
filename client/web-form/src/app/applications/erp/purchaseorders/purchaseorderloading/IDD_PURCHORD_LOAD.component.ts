import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PURCHORD_LOADService } from './IDD_PURCHORD_LOAD.service';

@Component({
    selector: 'tb-IDD_PURCHORD_LOAD',
    templateUrl: './IDD_PURCHORD_LOAD.component.html',
    providers: [IDD_PURCHORD_LOADService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PURCHORD_LOADComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PURCHORD_LOADService,
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
        
        		this.bo.appendToModelStructure({'PurchaseOrderLoading':['InternalOrdNo','OrderDate','ExpectedDeliveryDate','Supplier','Payment','Currency','OurReference','YourReference','Notes'],'HKLSupplier':['CompNameCompleteWithTaxNumber'],'HKLPaymentTerms':['Description'],'HKLCurrencies':['Description'],'global':['PurchaseOrdDetailLoading'],'PurchaseOrdDetailLoading':['LineType','Drawing','Item','PurchaseOr_SupplierCode02','Description','UoM','QtyToDelivery','UnitValue','ExpectedDeliveryDate','Qty','DeliveredQty','Lot','DiscountFormula','TaxableAmount','SaleOrdNo','SaleOrdPos','CostCenter','Job'],'PurchaseOrdSummaryLoading':['GoodsAmount','ServiceAmounts','GeneralDiscountTot','PayableAmount','PayableAmountInBaseCurr']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PURCHORD_LOADFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PURCHORD_LOADComponent, resolver);
    }
} 