import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_LOAD_GOODSRECPURCHASEORDERService } from './IDD_LOAD_GOODSRECPURCHASEORDER.service';

@Component({
    selector: 'tb-IDD_LOAD_GOODSRECPURCHASEORDER',
    templateUrl: './IDD_LOAD_GOODSRECPURCHASEORDER.component.html',
    providers: [IDD_LOAD_GOODSRECPURCHASEORDERService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_LOAD_GOODSRECPURCHASEORDERComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_LOAD_GOODSRECPURCHASEORDERService,
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
		boService.appendToModelStructure({'GoodsRecPurchaseOrderLoading':['InternalOrdNo','OrderDate','ExpectedDeliveryDate','Supplier','Payment','Currency','OurReference','YourReference','Notes'],'HKLSuppGoodsRecPurchaseOrder':['CompNameComplete'],'HKLPaymentTerms':['Description'],'HKLCurrencies':['Description'],'global':['Detail'],'Detail':['PurchaseOr_Selected','Position','LineType','Item','SupplierCode','Description','Qty','QtyToReceipt','ReceiptQty','PaidQty','UoM','UnitValue','ExpectedDeliveryDate','ConfirmedDeliveryDate','Delivered','Paid','TaxableAmount','DiscountFormula','TaxCode','NoPrint','NoDN','NoInvoice','Lot','Job'],'PurchaseOrdSummaryLoading':['GoodsAmount','ServiceAmounts','GeneralDiscountTot','PayableAmount','PayableAmountInBaseCurr']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_LOAD_GOODSRECPURCHASEORDERFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_LOAD_GOODSRECPURCHASEORDERComponent, resolver);
    }
} 