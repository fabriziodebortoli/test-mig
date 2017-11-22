import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CASHENTRIESService } from './IDD_CASHENTRIES.service';

@Component({
    selector: 'tb-IDD_CASHENTRIES',
    templateUrl: './IDD_CASHENTRIES.component.html',
    providers: [IDD_CASHENTRIESService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_CASHENTRIESComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_CASHENTRIESOPEN_CASH_itemSource: any;

    constructor(document: IDD_CASHENTRIESService,
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
        this.IDC_CASHENTRIESOPEN_CASH_itemSource = {
  "name": "CashCombo",
  "namespace": "ERP.Cash.Documents.CashCombo"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['RadioSession','CashSessionsBalance','CashSessionsEntries','PrefCurrency','AltCurrency','CashSessionsBalance','bCloseSession','bPrint','bDontCloseSession','PrefCurrency','AltCurrency','CashSessionsBalance'],'CashSessions':['Cash','l_WorkerDesc','SessionNo','Posted','OpeningDate','ClosingDate','Cash','WorkerDesc','SessionNo','Posted','OpeningDate','ClosingDate','Cash','WorkerDesc'],'CashSessionsBalance':['Currency','Symbol','OpeningBalance','Currency','Symbol','OpeningBalance','ClosingBalance','Currency','Symbol','OpeningBalance','ClosingBalance'],'CashSessionsEntries':['l_Bmp','PostingDate','l_SessionEntryId','l_Symbol','l_AmountPos','l_AmountNeg','l_Balance','l_AltBalance','Notes','Reason','CashStubBook','DocNo','CustSupp','CustSuppDescri','Printed']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CASHENTRIESFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CASHENTRIESComponent, resolver);
    }
} 