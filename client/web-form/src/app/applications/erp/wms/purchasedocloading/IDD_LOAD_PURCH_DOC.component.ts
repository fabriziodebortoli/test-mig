﻿import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_LOAD_PURCH_DOCService } from './IDD_LOAD_PURCH_DOC.service';

@Component({
    selector: 'tb-IDD_LOAD_PURCH_DOC',
    templateUrl: './IDD_LOAD_PURCH_DOC.component.html',
    providers: [IDD_LOAD_PURCH_DOCService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_LOAD_PURCH_DOCComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_LOAD_PURCH_DOCService,
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
		boService.appendToModelStructure({'PurchaseDocLoading':['DocNo','DocumentDate','Supplier','Payment','PostingDate','Currency','FixingDate','Fixing'],'HKLCustSupp':['CompNameComplete'],'HKLPaymentTerms':['Description'],'HKLCurrencies':['Description'],'global':['PurchaseDocLoadingSlave','GeneralDiscountTot'],'PurchaseDocLoadingSlave':['PurchaseDo_Selected','LineType','Item','Description','UoM','PurchaseDo_QtyToReturn','UnitValue','DiscountFormula','TaxCode','Lot','CostCenter','Job'],'PurchaseDocSummaryLoading':['GoodsAmount','ServiceAmounts','PayableAmount','PayableAmountInBaseCurr']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_LOAD_PURCH_DOCFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_LOAD_PURCH_DOCComponent, resolver);
    }
} 