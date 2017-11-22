import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_OUTSTANDINGBILLSService } from './IDD_OUTSTANDINGBILLS.service';

@Component({
    selector: 'tb-IDD_OUTSTANDINGBILLS',
    templateUrl: './IDD_OUTSTANDINGBILLS.component.html',
    providers: [IDD_OUTSTANDINGBILLSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_OUTSTANDINGBILLSComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_BILLSINS_ACTION_itemSource: any;

    constructor(document: IDD_OUTSTANDINGBILLSService,
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
        this.IDC_BILLSINS_ACTION_itemSource = {
  "name": "ActionAmountAdmCasesCombo",
  "namespace": "ERP.AP_AR_Plus.Documents.ActionAmountAdmCasesCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['Bank','NrBillsCAs','Presentation','CustDataSel','BillNoSel','NrEffect','MandateCodeSel','MandateCode','StartingDate','EndingDate','Customer','WithOutstanding','Collected','NotCollected','Both','bOrderByNo','bOrderByCustomer','PostingDate','DocDate','NrDoc','Charges','ReopenAll','Block','Bills','TotalAmount'],'Bills':['l_TEnhOutstandingBills_P01','BillNo','MandateCode','CustSupp','l_TEnhOutstandingBills_P02','l_TEnhOutstandingBills_P11','PaymentTerm','Collected','Outstanding','OpenedAdmCases','PayableAmountInBaseCurr','PresentationAmountBaseCurr','l_TEnhOutstandingBills_P06','l_TEnhOutstandingBills_P10','l_TEnhOutstandingBills_P09','OutstandingAmountBaseCurr','l_TEnhOutstandingBills_P08','l_TEnhOutstandingBills_P07','l_TEnhOutstandingBills_P03']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_OUTSTANDINGBILLSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_OUTSTANDINGBILLSComponent, resolver);
    }
} 