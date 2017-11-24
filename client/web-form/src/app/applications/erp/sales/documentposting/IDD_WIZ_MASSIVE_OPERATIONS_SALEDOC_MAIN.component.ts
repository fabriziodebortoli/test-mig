import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_WIZ_MASSIVE_OPERATIONS_SALEDOC_MAINService } from './IDD_WIZ_MASSIVE_OPERATIONS_SALEDOC_MAIN.service';

@Component({
    selector: 'tb-IDD_WIZ_MASSIVE_OPERATIONS_SALEDOC_MAIN',
    templateUrl: './IDD_WIZ_MASSIVE_OPERATIONS_SALEDOC_MAIN.component.html',
    providers: [IDD_WIZ_MASSIVE_OPERATIONS_SALEDOC_MAINService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_WIZ_MASSIVE_OPERATIONS_SALEDOC_MAINComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_SALES_POSTDOC_DOCTYPE_itemSource: any;

    constructor(document: IDD_WIZ_MASSIVE_OPERATIONS_SALEDOC_MAINService,
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
        this.IDC_SALES_POSTDOC_DOCTYPE_itemSource = {
  "name": "DocumentTypeCombo",
  "namespace": "ERP.Sales.Components.SaleDocTypeForPostingSaleDocCombo"
}; 

        		this.bo.appendToModelStructure({'global':['DocumentType','ElaboratorStartingDate','ElaboratorEndingDate','ElaboratorAllCustomer','ElaboratorCustomerSel','ElaboratorFromCustomer','ElaboratorToCustomer','ElaboratorAllDocNo','ElaboratorDocNoSel','ElaboratorFromDocNo','ElaboratorToDocNo','ElaboratorAllStubBooks','ElaboratorStubBooksSelection','ElaboratorFromStubBook','ElaboratorToStubBook','ElaboratorAllTaxJournal','ElaboratorTaxJournalSel','ElaboratorFromStubBook','ElaboratorToTaxJournal','ElaboratorAllIssue','ElaboratorNotIssued','ElaboratorIssued','ElaboratorAllInv','ElaboratorNotInInv','ElaboratorInInv','ElaboratorAllArchived','ElaboratorNotArchived','ElaboratorArchived','ElaboratorAllAccounting','ElaboratorNotInAccounting','ElaboratorInAccounting','ElaboratorAll_AccTempl','ElaboratorAccTemplSel','ElaboratorStartingAccTempl','ElaboratorEndingAccTempl','ElaboratorAllPrinted','ElaboratorNoPrinted','ElaboratorPrinted','ElaboratorAllPostaLite','ElaboratorPostaLiteNo','ElaboratorPostaLiteYes','ElaboratorAllMailed','ElaboratorMailNo','ElaboratorMailYes','ElaboratorAllSalesPeople','ElaboratorSalesPeopleSel','ElaboratorFromSalesperson','ElaboratorToSalesperson','SaleOrdFulfilmentDetail','ElaboratorDocDatePost','ElaboratorAppDatePost','ElaboratorInsDatePost','OperationDate','bPrintMail','eMailAddressType','bPrintPostaLite','sReportTemplate','nCurrentElement','GaugeDescription','ProgressViewer'],'SaleOrdFulfilmentDetail':['DocGeneration','CustSupp','CustomerCompanyName','Salesperson','DepartureDate','DocumentDate','DocNo','Payment','Currency','SendDocumentsTo'],'ProgressViewer':['TEnhProgressViewer_P1','TEnhProgressViewer_P2','TEnhProgressViewer_P3']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_WIZ_MASSIVE_OPERATIONS_SALEDOC_MAINFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_WIZ_MASSIVE_OPERATIONS_SALEDOC_MAINComponent, resolver);
    }
} 