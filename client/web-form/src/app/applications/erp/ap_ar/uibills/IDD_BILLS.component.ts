import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BILLSService } from './IDD_BILLS.service';

@Component({
    selector: 'tb-IDD_BILLS',
    templateUrl: './IDD_BILLS.component.html',
    providers: [IDD_BILLSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_BILLSComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_BILLS_BILLSTATUS_itemSource: any;

    constructor(document: IDD_BILLSService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        changeDetectorRef: ChangeDetectorRef) {
		super(document, eventData, ciService, changeDetectorRef, resolver);
        this.eventData.change.subscribe(() => this.changeDetectorRef.detectChanges());
    }

    ngOnInit() {
        super.ngOnInit();
        this.IDC_BILLS_BILLSTATUS_itemSource = {
  "name": "BillStatusEnumCombo",
  "namespace": "ERP.AP_AR.Documents.BillStatusEnumCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'Bills':['BillCode','BillType','Description','FiscalNo','Amount','DueDate','BillStatus','IssuerBank','IssueDate','IssuePlace','IssuerName','Customer','ReceiptDate','Supplier','TransferDate','PresentationBank','PresentationCA','PresentationDate','CollectionDate','OutstandingDate','BillCode','Disabled','BillType','IssuerBank','IssuerBankCA','IssuerBankCA','FiscalNo','Amount','DueDate','BillStatus','Description','IssueDate','IssuePlace','IssuerName','IssuerTaxIdNumber','IssuerFiscalCode','Customer','ReceiptDate','Supplier','TransferDate','PresentationBank','PresentationCA','PresentationDate','Return1Date','Return1Reason','RePresentationDate','Return2Date','Return2Reason','CollectionDate','OutstandingDate','BillCode','Disabled','BillType','IssuerBank','IssuerBankCA','IssuerBankCA','FiscalNo','Amount','DueDate','BillStatus','Description','IssueDate','IssuePlace','IssuerName','IssuerTaxIdNumber','IssuerFiscalCode','Supplier','CollectionDate','OutstandingDate'],'HKLIssuerBank':['Description'],'HKLCustomers':['CompanyName','CompanyName'],'HKLSuppliers':['CompanyName','CompanyName','CompanyName'],'HKLPresBank':['Description'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg','DBTLinksTable'],'DBTLinksTable':['Image','Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BILLSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BILLSComponent, resolver);
    }
} 