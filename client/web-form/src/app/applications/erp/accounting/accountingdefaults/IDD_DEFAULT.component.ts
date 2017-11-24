import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_DEFAULTService } from './IDD_DEFAULT.service';

@Component({
    selector: 'tb-IDD_DEFAULT',
    templateUrl: './IDD_DEFAULT.component.html',
    providers: [IDD_DEFAULTService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_DEFAULTComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_DEFAULTService,
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
        
        		this.bo.appendToModelStructure({'global':['PurchasesTaxAccounts','SalesTaxAccounts','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'PurchasesTaxAccounts':['l_All','TaxCode','TaxAccount','TaxSuspendedAccount'],'HKLPurchasesTaxCode':['Description'],'HKLPurchasesTaxAccounts':['Description'],'HKLPurchasesSuspendedTaxAccounts':['Description'],'SalesTaxAccounts':['l_All','TaxCode','TaxAccount','TaxSuspendedAccount'],'HKLSalesTaxCode':['Description'],'HKLSalesTaxAccounts':['Description'],'HKLSalesSuspendedTaxAccounts':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_DEFAULTFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_DEFAULTComponent, resolver);
    }
} 