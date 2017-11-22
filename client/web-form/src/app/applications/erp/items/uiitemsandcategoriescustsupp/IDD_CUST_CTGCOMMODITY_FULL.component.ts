import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CUST_CTGCOMMODITY_FULLService } from './IDD_CUST_CTGCOMMODITY_FULL.service';

@Component({
    selector: 'tb-IDD_CUST_CTGCOMMODITY_FULL',
    templateUrl: './IDD_CUST_CTGCOMMODITY_FULL.component.html',
    providers: [IDD_CUST_CTGCOMMODITY_FULLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_CUST_CTGCOMMODITY_FULLComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CUST_CTGCOMMODITY_FULLService,
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
		boService.appendToModelStructure({'CommodityCtgCustomers':['Category','Disabled','PriceList','Customer','Notes','MinOrderQty','DaysForDelivery','ShippingCost','DiscountFormula','AdditionalCharges','SaleOffset','LastSaleDocType','LastSaleDocNo','LastSaleDocDate','LastPaymentTerm','LastSaleQty','LastDiscountFormula','LastSaleValue','LastRMADocNo','LastRMADocDate','LastRMAQty','LastRMAValue'],'HKLCtgCommodity':['Description','DiscountFormula'],'HKLPriceLists':['Description'],'HKLCustomersCommodityCtg':['CompanyName'],'HKLSaleOffset':['Description'],'global':['Budget','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'Budget':['BudgetYear','BudgetMonth','SaleQty','SaleValue'],'HKLPymtTermCommodityCustomersCtg':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CUST_CTGCOMMODITY_FULLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CUST_CTGCOMMODITY_FULLComponent, resolver);
    }
} 