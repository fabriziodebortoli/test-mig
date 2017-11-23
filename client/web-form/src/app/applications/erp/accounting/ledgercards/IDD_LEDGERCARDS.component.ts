import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_LEDGERCARDSService } from './IDD_LEDGERCARDS.service';

@Component({
    selector: 'tb-IDD_LEDGERCARDS',
    templateUrl: './IDD_LEDGERCARDS.component.html',
    providers: [IDD_LEDGERCARDSService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_LEDGERCARDSComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_LEDGERCARDSService,
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
		boService.appendToModelStructure({'global':['strByAccountCustSupp','Account','Description','CustSupp','bByPostDate','bByAccrDate','StartingDate','EndingDate','AllEntry','Nature','LedgerCards','OpenBalance','FinalBalance'],'LedgerCards':['l_TEnhLedgerCards_P01','PostingDate','AccrualDate','Nature','l_TEnhLedgerCards_P02','l_TEnhLedgerCards_P03','l_TEnhLedgerCards_P04','l_TEnhLedgerCards_P05','l_TEnhLedgerCards_P08','Currency','DocCurrAmount','l_TEnhLedgerCards_P06','l_TEnhLedgerCards_P07','l_TEnhLedgerCards_P11','AccRsn','l_TEnhLedgerCards_P10','Notes','l_TEnhLedgerCards_P09']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_LEDGERCARDSFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_LEDGERCARDSComponent, resolver);
    }
} 