import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PAYABLESRECEIVABLES_ACTIVEService } from './IDD_PAYABLESRECEIVABLES_ACTIVE.service';

@Component({
    selector: 'tb-IDD_PAYABLESRECEIVABLES_ACTIVE',
    templateUrl: './IDD_PAYABLESRECEIVABLES_ACTIVE.component.html',
    providers: [IDD_PAYABLESRECEIVABLES_ACTIVEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PAYABLESRECEIVABLES_ACTIVEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PAYABLESRECEIVABLES_ACTIVEService,
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
		boService.appendToModelStructure({'AP_AR':['CustSupp','DocNo','LogNo','DocumentDate','Blocked','Currency','Settled','Payment','TotalAmount','TaxAmount','Advance','CreditNote','WithholdingTaxManagement','AmountsWithWHTax','InstallmStartDate','SendDocumentsTo','ContractCode','ProjectCode','Group1','Group2','Description','Notes','Salesperson','Area','WHTaxable','WHTaxableCN','TotalAmountCN','Salesperson','Area','WHTaxable','WHTaxableCN','TotalAmountCN'],'HKLCustSupp':['CompNameComplete'],'HKLCurrencies':['Description'],'global':['StatusTileValue','StatusTileDescription','StatusTileImage','ClosingAmount','IntallmentRegenerate','AmountRegenerate','ClosingRegenerate','Detail','PymtSchedTot','OpenOutsTot','ClosingTot','PymtsTot','Balance','Delta','BranchDescri','CaptionRef1','CaptionRef2','Detail','Detail','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'HKLPymtTerm':['Description'],'HKLSalesPeople':['Name','Name'],'HKLSaleAreas':['Description','Description'],'Detail':['InstallmentType','InstallmentNo','InstallmentDate','PaymentTerm','NotPresentable','Presented','BillNo','Slip','PresentationDate','PresentationAmount','PresentationAmountBaseCurr','PresentationBank','CA','Printed','Approved','ApprovalDate','ApprovedAmount','ApprovedAmountBaseCurr','Collected','CollectionDate','Outstanding','OutstandingDate','OutstandingAmount','OutstandingAmountBaseCurr','PresentationNotes']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PAYABLESRECEIVABLES_ACTIVEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PAYABLESRECEIVABLES_ACTIVEComponent, resolver);
    }
} 