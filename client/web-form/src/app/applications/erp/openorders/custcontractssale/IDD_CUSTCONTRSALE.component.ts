import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CUSTCONTRSALEService } from './IDD_CUSTCONTRSALE.service';

@Component({
    selector: 'tb-IDD_CUSTCONTRSALE',
    templateUrl: './IDD_CUSTCONTRSALE.component.html',
    providers: [IDD_CUSTCONTRSALEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_CUSTCONTRSALEComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_CUSTCONTR_CUSTCONTRLINES_UOM_itemSource: any;

    constructor(document: IDD_CUSTCONTRSALEService,
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
        this.IDC_CUSTCONTR_CUSTCONTRLINES_UOM_itemSource = {
  "allowChanges": false,
  "name": "UnitsOfMeasureBOMComboBox",
  "namespace": "ERP.OpenOrders.Documents.UnitsOfMeasureCustContrSaleComboBox",
  "useProductLanguage": false
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'CustContr':['ContractType','ContractNo','Description','Customer','StartValidityDate','EndValidityDate','Disabled','Payment','Notes','FixingDate','Fixing'],'HKLCustomer':['CompanyName','PriceList','Currency'],'HKLPaymentTerms':['Description'],'HKLPriceList':['Description'],'HKLCurrency':['Description'],'global':['CustContrLines','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'CustContrLines':['LineType','Item','Description','UoM','MinimumQty','Quantity','UnitValue','DiscountFormula','Price','BudgetQty','BudgetValue','Notes']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CUSTCONTRSALEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CUSTCONTRSALEComponent, resolver);
    }
} 