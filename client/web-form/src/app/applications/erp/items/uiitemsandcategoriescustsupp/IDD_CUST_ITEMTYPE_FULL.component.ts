import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CUST_ITEMTYPE_FULLService } from './IDD_CUST_ITEMTYPE_FULL.service';

@Component({
    selector: 'tb-IDD_CUST_ITEMTYPE_FULL',
    templateUrl: './IDD_CUST_ITEMTYPE_FULL.component.html',
    providers: [IDD_CUST_ITEMTYPE_FULLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_CUST_ITEMTYPE_FULLComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CUST_ITEMTYPE_FULLService,
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
		boService.appendToModelStructure({'ItemTypeCustomers':['ItemType','Customer','DiscountFormula'],'HKLItemType':['Description','DiscountFormula'],'HKLCustomersItemType':['CompanyName'],'global':['Budget','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'Budget':['BudgetYear','BudgetMonth','SaleQty','SaleValue']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CUST_ITEMTYPE_FULLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CUST_ITEMTYPE_FULLComponent, resolver);
    }
} 