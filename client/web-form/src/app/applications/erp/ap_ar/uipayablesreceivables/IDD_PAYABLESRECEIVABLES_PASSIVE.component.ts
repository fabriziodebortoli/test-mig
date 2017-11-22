import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PAYABLESRECEIVABLES_PASSIVEService } from './IDD_PAYABLESRECEIVABLES_PASSIVE.service';

@Component({
    selector: 'tb-IDD_PAYABLESRECEIVABLES_PASSIVE',
    templateUrl: './IDD_PAYABLESRECEIVABLES_PASSIVE.component.html',
    providers: [IDD_PAYABLESRECEIVABLES_PASSIVEService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_PAYABLESRECEIVABLES_PASSIVEComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_PAYABLESRECEIVABLES_PASSIVEService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'AP_AR':['CustSupp','DocNo','LogNo','DocumentDate','Blocked','Currency','Settled','Payment','TotalAmount','TaxAmount','Advance','CreditNote','WithholdingTaxManagement','AmountsWithWHTax','InstallmStartDate','SendDocumentsTo','ContractCode','ProjectCode','Group1','Group2','Description','Notes','ESRReferenceNumber','ESRCheckDigit'],'HKLCustSupp':['CompNameComplete'],'HKLCurrencies':['Description'],'global':['StatusTileValue','StatusTileDescription','StatusTileImage','ClosingAmount','IntallmentRegenerate','AmountRegenerate','ClosingRegenerate','Detail','PymtSchedTot','OpenOutsTot','ClosingTot','PymtsTot','Balance','Delta','BranchDescri','CaptionRef1','CaptionRef2','Detail','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'HKLPymtTerm':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PAYABLESRECEIVABLES_PASSIVEFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PAYABLESRECEIVABLES_PASSIVEComponent, resolver);
    }
} 