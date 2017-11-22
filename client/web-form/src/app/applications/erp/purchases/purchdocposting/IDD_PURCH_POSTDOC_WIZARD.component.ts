import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PURCH_POSTDOC_WIZARDService } from './IDD_PURCH_POSTDOC_WIZARD.service';

@Component({
    selector: 'tb-IDD_PURCH_POSTDOC_WIZARD',
    templateUrl: './IDD_PURCH_POSTDOC_WIZARD.component.html',
    providers: [IDD_PURCH_POSTDOC_WIZARDService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_PURCH_POSTDOC_WIZARDComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_PURCH_POSTDOC_DOCTYPE_itemSource: any;

    constructor(document: IDD_PURCH_POSTDOC_WIZARDService,
        eventData: EventDataService,
        resolver: ComponentFactoryResolver,
        private store: Store,
        ciService: ComponentInfoService,
        private changeDetectorRef: ChangeDetectorRef) {
        super(document, eventData, resolver, ciService);
        this.eventData.change.subscribe(() => this.changeDetectorRef.detectChanges());
    }

    ngOnInit() {
        super.ngOnInit();
        this.IDC_PURCH_POSTDOC_DOCTYPE_itemSource = {
  "name": "PurchDocTypeForPostingPurchDocCombo",
  "namespace": "ERP.Purchases.Documents.PurchDocTypeForPostingPurchDocCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['DocumentType','ElaboratorStartingDate','ElaboratorEndingDate','ElaboratorAllSupp','ElaboratorSuppsSel','ElaboratorSuppStart','ElaboratorSuppEnd','ElaboratorAllTaxJourBoll','ElaboratorTaxJournalSel','ElaboratorFromTaxJourBoll','ElaboratorToTaxJourBoll','ElaboratorToTaxJourBoll','ElaboratorAll_AccTempl','ElaboratorAccTemplSel','ElaboratorStartingAccTempl','ElaboratorEndingAccTempl','ElaboratorAllDocNo','ElaboratorDocNoSel','ElaboratorFromDocNo','ElaboratorToDocNo','ElaboratorAllIssue','ElaboratorNotIssued','ElaboratorIssued','ElaboratorAllInv','ElaboratorNotInInv','ElaboratorInInv','ElaboratorAllAccounting','ElaboratorNotInAccounting','ElaboratorInAccounting','ElaboratorAllPrinted','ElaboratorNoPrinted','ElaboratorPrinted','PurchaseOrdFulfilmentDetail','ElaboratorDocDatePost','ElaboratorAppDatePost','ElaboratorInsDatePost','OperationDate','bPrintMail','nCurrentElement','GaugeDescription','ProgressViewer'],'PurchaseOrdFulfilmentDetail':['DocGeneration','Supplier','CustomerCompanyName','DepartureDate','DocumentDate','DocNo','Payment','Currency','SendDocumentsTo'],'ProgressViewer':['TEnhProgressViewer_P1','TEnhProgressViewer_P2','TEnhProgressViewer_P3']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PURCH_POSTDOC_WIZARDFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PURCH_POSTDOC_WIZARDComponent, resolver);
    }
} 