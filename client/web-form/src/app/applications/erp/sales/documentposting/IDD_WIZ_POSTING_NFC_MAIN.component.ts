import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_WIZ_POSTING_NFC_MAINService } from './IDD_WIZ_POSTING_NFC_MAIN.service';

@Component({
    selector: 'tb-IDD_WIZ_POSTING_NFC_MAIN',
    templateUrl: './IDD_WIZ_POSTING_NFC_MAIN.component.html',
    providers: [IDD_WIZ_POSTING_NFC_MAINService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_WIZ_POSTING_NFC_MAINComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_NFC_POSTDOC_STATUS_NFE_itemSource: any;

    constructor(document: IDD_WIZ_POSTING_NFC_MAINService,
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
        this.IDC_NFC_POSTDOC_STATUS_NFE_itemSource = {
  "name": "BRNFeStatusForRecalculationCombo",
  "namespace": "ERP.Business_BR.Components.BRNFeStatusForRecalculationCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['ElaboratorStartingDate','ElaboratorEndingDate','ElaboratorStatusNFe','ElaboratorAllCustomer','ElaboratorCustomerSel','ElaboratorFromCustomer','ElaboratorToCustomer','ElaboratorAllInv','ElaboratorNotInInv','ElaboratorInInv','ElaboratorAllSalesPeople','ElaboratorSalesPeopleSel','ElaboratorFromSalesperson','ElaboratorToSalesperson','SaleOrdFulfilmentDetail','nCurrentElement','GaugeDescription','ProgressViewer'],'SaleOrdFulfilmentDetail':['DocGeneration','CustSupp','CustomerCompanyName','Salesperson','DepartureDate','DocumentDate','DocNo','Payment','Currency','SendDocumentsTo'],'ProgressViewer':['TEnhProgressViewer_P1','TEnhProgressViewer_P2','TEnhProgressViewer_P3']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_WIZ_POSTING_NFC_MAINFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_WIZ_POSTING_NFC_MAINComponent, resolver);
    }
} 