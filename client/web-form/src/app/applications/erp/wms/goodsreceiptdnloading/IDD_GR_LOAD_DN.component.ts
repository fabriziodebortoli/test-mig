import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_GR_LOAD_DNService } from './IDD_GR_LOAD_DN.service';

@Component({
    selector: 'tb-IDD_GR_LOAD_DN',
    templateUrl: './IDD_GR_LOAD_DN.component.html',
    providers: [IDD_GR_LOAD_DNService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_GR_LOAD_DNComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_GR_LOAD_DNService,
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
        
        		this.bo.appendToModelStructure({'DNLoading':['DocNo','DocumentDate','PostingDate','CustSupp','Payment','Currency','FixingDate','Fixing'],'HKLCustSupp':['CompNameComplete'],'HKLPaymentTerms':['Description'],'HKLCurrencies':['Description'],'global':['DNDetailLoading'],'DNDetailLoading':['DocDetDNGR_Selected','Invoiced','LineType','Item','Description','UoM','DocDetDNGR_QtaToIssue','Qty','InvoicedQty','UnitValue','DiscountFormula','TaxCode','Lot','CostCenter','Job'],'DNSummaryLoading':['GoodsAmount','ServiceAmounts','PayableAmount','PayableAmountInBaseCurr']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_GR_LOAD_DNFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_GR_LOAD_DNComponent, resolver);
    }
} 