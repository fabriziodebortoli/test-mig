import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_SUPP_CTGCOMMODITY_FULLService } from './IDD_SUPP_CTGCOMMODITY_FULL.service';

@Component({
    selector: 'tb-IDD_SUPP_CTGCOMMODITY_FULL',
    templateUrl: './IDD_SUPP_CTGCOMMODITY_FULL.component.html',
    providers: [IDD_SUPP_CTGCOMMODITY_FULLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_SUPP_CTGCOMMODITY_FULLComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_SUPP_CTGCOMMODITY_FULLService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'CommodityCtgSuppliers':['Category','Disabled','Supplier','Notes','MinOrderQty','DaysForDelivery','ShippingCost','DiscountFormula','AdditionalCharges','PurchaseOffset','LastPurchaseDocType','LastPurchaseDocNo','LastPurchaseDocDate','LastPaymentTerm','LastPurchaseQty','LastDiscountFormula','LastPurchaseValue','LastRMADocNo','LastRMADocDate','LastRMAQty','LastRMAValue'],'HKLCtgCommodity':['Description','DiscountFormula'],'HKLSuppCtgCommodity':['CompanyName'],'HKLPurchaseOffset':['Description'],'HKLPymtTerm':['Description'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_SUPP_CTGCOMMODITY_FULLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_SUPP_CTGCOMMODITY_FULLComponent, resolver);
    }
} 