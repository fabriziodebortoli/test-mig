import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_CASHENTRIES_WORKINGSESSIONService } from './IDD_CASHENTRIES_WORKINGSESSION.service';

@Component({
    selector: 'tb-IDD_CASHENTRIES_WORKINGSESSION',
    templateUrl: './IDD_CASHENTRIES_WORKINGSESSION.component.html',
    providers: [IDD_CASHENTRIES_WORKINGSESSIONService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
export class IDD_CASHENTRIES_WORKINGSESSIONComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_CASHENTRIES_WORKINGSESSIONService,
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
        
        const boService = this.document as BOService;
		boService.appendToModelStructure({'global':['CashSessionsEntries','PrefCurrency','AltCurrency','CashSessionsBalance','ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg'],'CashSessionsEntries':['l_Bmp','PostingDate','l_SessionEntryId','l_Symbol','l_AmountPos','l_AmountNeg','l_Balance','l_AltBalance','Notes','Reason','CashStubBook','DocNo','CustSupp','CustSuppDescri','Printed'],'CashSessions':['SessionNo','Posted','OpeningDate','ClosingDate','Cash','WorkerDesc'],'CashSessionsBalance':['Currency','Symbol','OpeningBalance','ClosingBalance']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_CASHENTRIES_WORKINGSESSIONFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_CASHENTRIES_WORKINGSESSIONComponent, resolver);
    }
} 