import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_WIZVIEW_DEFINVService } from './IDD_WIZVIEW_DEFINV.service';

@Component({
    selector: 'tb-IDD_WIZVIEW_DEFINV',
    templateUrl: './IDD_WIZVIEW_DEFINV.component.html',
    providers: [IDD_WIZVIEW_DEFINVService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_WIZVIEW_DEFINVComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_WIZVIEW_DEFINVService,
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
		boService.appendToModelStructure({'global':['FullfillmentStartingDate','FullfillmentEndingDate','FullfillmentAllCustomer','FullfillmentCustomerSel','FullfillmentFromCustomer','FullfillmentToCustomer','FullfillmentAllDocNo','FullfillmentDocNoSel','FullfillmentFromDocNo','FullfillmentToDocNo','FullfillmentAllStubBooks','FullfillmentStubBooksSelection','FullfillmentFromStubBook','FullfillmentToStubBook','FullfillmentAllTaxJournal','FullfillmentTaxJournalSel','FullfillmentFromTaxJournal','FullfillmentToTaxJournal','FullfillmentAllGroup','FullfillmentGroupSel','FullfillmentGroupCode','FullfillmentAllStorage2','FullfillmentStorage2Sel','FullfillmentFromStorage2','FullfillmentToStorage2','FullfillmentAllSalesPeople','FullfillmentSalesPeopleSel','FullfillmentFromSalesperson','FullfillmentToSalesperson','DeferredInvoicing','OperationDate','DefInvLastData','TrialDefInv','PrintMail','eMailAddressType','PrintPostaLite','bOneInvoicePerDN','nCurrentElement','GaugeDescription','ProgressViewer'],'DeferredInvoicing':['Inv','CustSupp','CustDescription','InvoicingCustomer','InvoicingCustDescription','StoragePhase2','Payment','Salesperson','DepartureDate','DocumentDate','DocNo','StubBook','InvoicingTaxJournal','Currency','SendDocumentsTo','Job','CostCenter','DocumentType'],'DefInvLastData':['Title','Value'],'ProgressViewer':['TEnhProgressViewer_P1','TEnhProgressViewer_P2','TEnhProgressViewer_P3']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_WIZVIEW_DEFINVFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_WIZVIEW_DEFINVComponent, resolver);
    }
} 