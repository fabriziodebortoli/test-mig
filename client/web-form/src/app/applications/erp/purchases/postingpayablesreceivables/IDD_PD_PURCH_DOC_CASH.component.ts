import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_PD_PURCH_DOC_CASHService } from './IDD_PD_PURCH_DOC_CASH.service';

@Component({
    selector: 'tb-IDD_PD_PURCH_DOC_CASH',
    templateUrl: './IDD_PD_PURCH_DOC_CASH.component.html',
    providers: [IDD_PD_PURCH_DOC_CASHService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_PD_PURCH_DOC_CASHComponent extends BOComponent implements OnInit, OnDestroy {
     public IDC_PURCH_DOC_CASH_CASH_itemSource: any;
public IDC_PURCH_DOC_CASH_STUBBOOK_itemSource: any;

    constructor(document: IDD_PD_PURCH_DOC_CASHService,
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
        this.IDC_PURCH_DOC_CASH_CASH_itemSource = {
  "name": "PurchaseCashItemSource",
  "namespace": "ERP.Purchases.Documents.PurchaseCashItemSource"
}; 
this.IDC_PURCH_DOC_CASH_STUBBOOK_itemSource = {
  "name": "PurchaseCashStubBookItemSource",
  "namespace": "ERP.Purchases.Documents.PurchaseCashStubBookItemSource"
}; 

        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['Cash','SessionNo','CashReason','CashStubBook','CashDocNo','bAutoPrint','CashCurrency','CashTotal','CashTotalRounded','CashAmount','CashClosed','CashChange','CashRoundChange']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_PD_PURCH_DOC_CASHFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_PD_PURCH_DOC_CASHComponent, resolver);
    }
} 