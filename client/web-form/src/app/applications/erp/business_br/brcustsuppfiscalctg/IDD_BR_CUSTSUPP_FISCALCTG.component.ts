import { Component, OnInit, OnDestroy, ComponentFactoryResolver, ChangeDetectionStrategy, ChangeDetectorRef } from '@angular/core';
import { ComponentService, ComponentInfoService, EventDataService, BOComponent, ControlComponent, BOService  } from '@taskbuilder/core';
import { Store, createSelectorByMap } from '@taskbuilder/core';

import { IDD_BR_CUSTSUPP_FISCALCTGService } from './IDD_BR_CUSTSUPP_FISCALCTG.service';

@Component({
    selector: 'tb-IDD_BR_CUSTSUPP_FISCALCTG',
    templateUrl: './IDD_BR_CUSTSUPP_FISCALCTG.component.html',
    providers: [IDD_BR_CUSTSUPP_FISCALCTGService, ComponentInfoService],
    changeDetection: ChangeDetectionStrategy.OnPush
})
    export class IDD_BR_CUSTSUPP_FISCALCTGComponent extends BOComponent implements OnInit, OnDestroy {
     
    constructor(document: IDD_BR_CUSTSUPP_FISCALCTGService,
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
		boService.appendToModelStructure({'DBTBRCustSuppFiscalCtg':['CustSuppFiscalCtg','Description','Disabled'],'global':['ValidationStatusPicture','ValidationStatus','SynchStatusPicture','SynchDate','SynchDirection','SynchStatusHints','SynchMsg']});

    }

    ngOnDestroy() {
        super.ngOnDestroy();
    }
}

@Component({
    template: ''
})
export class IDD_BR_CUSTSUPP_FISCALCTGFactoryComponent {
    constructor(componentService: ComponentService, resolver: ComponentFactoryResolver) {
        componentService.createComponent(IDD_BR_CUSTSUPP_FISCALCTGComponent, resolver);
    }
} 