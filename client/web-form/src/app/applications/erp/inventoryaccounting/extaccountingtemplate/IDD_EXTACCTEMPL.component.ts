import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_EXTACCTEMPLService } from './IDD_EXTACCTEMPL.service';

@Component({
    selector: 'tb-IDD_EXTACCTEMPL',
    templateUrl: './IDD_EXTACCTEMPL.component.html',
    providers: [IDD_EXTACCTEMPLService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_EXTACCTEMPLComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_EXTACCTEMPL_DOCDATEFORMULA_itemSource: any;
public IDC_EXTACCTEMPL_BODY_CUSTSUPP_itemSource: any;
public IDC_EXTACCTEMPL_BODY_ACCOUNT_itemSource: any;

    constructor(document: IDD_EXTACCTEMPLService,
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
        this.IDC_EXTACCTEMPL_DOCDATEFORMULA_itemSource = {
  "name": "DateFormulaCombo",
  "namespace": "ERP.InventoryAccounting.Components.DateFormulaCombo"
}; 
this.IDC_EXTACCTEMPL_BODY_CUSTSUPP_itemSource = {
  "name": "CustomerSupplierCombo",
  "namespace": "ERP.InventoryAccounting.Components.CustomerSupplierCombo"
}; 
this.IDC_EXTACCTEMPL_BODY_ACCOUNT_itemSource = {
  "name": "OffsetCombo",
  "namespace": "ERP.InventoryAccounting.Components.OffsetCombo",
  "parameter": "DataFile.ERP.InventoryAccounting.OffsetSymbols",
  "useProductLanguage": true
}; 

        		this.bo.appendToModelStructure({'ExtAccountingTemplate':['Template','Description','AccountingTemplate','DocDateFormula','SwitchCreditDebit','GroupRepeatedLines','UseBaseCurrency'],'HKLAccountingTemplate':['Description'],'global':['Detail','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'Detail':['Repeat','AccountingReason','LineType','CustSuppFormula','AccountFormula','DebitCredit','AmountFormula','AmountType','OffsetGroupNo','StorageNo']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_EXTACCTEMPLFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_EXTACCTEMPLComponent, resolver);
    }
} 