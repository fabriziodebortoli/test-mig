import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_ACCOUNTINGPOSTING_WIZARDService } from './IDD_ACCOUNTINGPOSTING_WIZARD.service';

@Component({
    selector: 'tb-IDD_ACCOUNTINGPOSTING_WIZARD',
    templateUrl: './IDD_ACCOUNTINGPOSTING_WIZARD.component.html',
    providers: [IDD_ACCOUNTINGPOSTING_WIZARDService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_ACCOUNTINGPOSTING_WIZARDComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_ACCOUNTINGPOSTING_WIZARDService,
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
        
        		this.bo.appendToModelStructure({'global':['Session','Cash','WorkerDesc','OpeningDate','ClosingDate','PostingDate','DocDate','CashAccountingPosting','Session','OpeningDate','ClosingDate','Cash','WorkerDesc'],'CashAccountingPosting':['l_TEnhCashSessionsEntries_P01','PostingDate','l_SessionEntryId','l_Symbol','l_AmountPos','l_AmountNeg','Notes','CustSupp','l_TEnhCashSessionsEntries_P10','l_TEnhCashSessionsEntries_P06','l_TEnhCashSessionsEntries_P08','CostCenter','Job'],'HKLAccount':['Description'],'HKLAccRsn':['Description']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_ACCOUNTINGPOSTING_WIZARDFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_ACCOUNTINGPOSTING_WIZARDComponent, resolver);
    }
} 