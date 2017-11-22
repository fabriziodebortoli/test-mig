import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_LOAD_SUPPQUOTAService } from './IDD_LOAD_SUPPQUOTA.service';

@Component({
    selector: 'tb-IDD_LOAD_SUPPQUOTA',
    templateUrl: './IDD_LOAD_SUPPQUOTA.component.html',
    providers: [IDD_LOAD_SUPPQUOTAService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_LOAD_SUPPQUOTAComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_LOAD_SUPPQUOTAService,
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
		boService.appendToModelStructure({'SuppQuotaLoading':['QuotationNo','QuotationDate','SupplierDocNo','Supplier','ProspectiveSupplier','Payment','Currency','ExpectedDeliveryDate','ValidityEndingDate','OurReference','YourReference','Notes'],'global':['SupplierDescri','SuppQuotaDetailLoading'],'HKLPaymentTerms':['Description'],'HKLCurrencies':['Description'],'SuppQuotaDetailLoading':['Selected','ExpectedDeliveryDate','LineType','Item','SupplierCode','Description','UoM','QtyToReceipt','UnitValue','TaxableAmount','DiscountFormula','TaxCode','NoPrint','NoCopyOnOrder'],'SuppQuotaSummaryLoading':['GoodsAmount','ServiceAmounts','GeneralDiscountTot','PayableAmount','PayableAmountInBaseCurr']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_LOAD_SUPPQUOTAFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_LOAD_SUPPQUOTAComponent, resolver);
    }
} 